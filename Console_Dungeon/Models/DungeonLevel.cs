using Console_Dungeon.Generation;
using Console_Dungeon.Managers;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class DungeonLevel
    {
        // TODO: add that only one heal action per level for now, resting respawns enemies
        // Level metadata
        public int LevelNumber { get; set; }
        public int Seed { get; set; }
        public bool IsExplored { get; set; }
        public bool IsBossDefeated { get; set; }

        // Grid properties
        public int Width { get; set; }
        public int Height { get; set; }
        public Room[,] Rooms { get; set; }

        // Start position for this level (determined during generation)
        public int StartX { get; private set; }
        public int StartY { get; private set; }

        // Progress tracking
        public int RoomsExplored { get; set; }
        public int TotalRooms { get; set; }

        // Boss room location (will be set during generation)
        public int BossRoomX { get; set; }
        public int BossRoomY { get; set; }

        // Constructor for new level
        public DungeonLevel(int levelNumber, int seed, int width = 5, int height = 5, int roomCount = 10)
        {
            LevelNumber = levelNumber;
            Seed = seed;
            IsExplored = false;
            IsBossDefeated = false;

            // Default values (may be overridden in GenerateLevel to produce 5x10 oriented grids)
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);

            // ENSURE MINIMUM ROOM COUNT - Need at least 2 rooms (start + boss)
            int minRooms = 2;
            int adjustedRoomCount = Math.Max(minRooms, roomCount);

            // Generate level layout (this will configure Width/Height/start position and Rooms)
            GenerateLevel(adjustedRoomCount);
        }

        // Parameterless constructor for deserialization
        public DungeonLevel()
        {
            Width = 5;
            Height = 5;
            Rooms = new Room[Width, Height];

            // Create simple open grid as fallback
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Rooms[x, y] = new Room($"A dim chamber at ({x},{y}).", isBlocked: false);
                }
            }

            TotalRooms = Width * Height;
            BossRoomX = Width - 1;
            BossRoomY = Height - 1;
            StartX = Width / 2;
            StartY = Height / 2;
        }

        // Helper methods
        public Room GetRoom(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Room coordinates out of range.");
            }
            return Rooms[x, y];
        }

        public bool IsComplete => IsBossDefeated;

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height && !Rooms[x, y].IsBlocked;
        }

        private void GenerateLevel(int roomCount)
        {
            // Determine grid orientation and size: use a 5x10 area (either 10x5 or 5x10).
            // Keep deterministic per level by using the level seed.
            var rng = new Random(Seed + LevelNumber);

            // Decide orientation: 0 => horizontal (width=10, height=5), 1 => vertical (width=5, height=10)
            bool horizontal = rng.Next(0, 2) == 0;
            Width = horizontal ? 10 : 5;
            Height = horizontal ? 5 : 10;

            // Determine target room count based on level ranges:
            // Levels 1-5: 6-10
            // Levels 6-10: 11-15
            // Levels 11+: 16-20
            int minRooms, maxRooms;
            if (LevelNumber <= 5)
            {
                minRooms = 6;
                maxRooms = 10;
            }
            else if (LevelNumber <= 10)
            {
                minRooms = 11;
                maxRooms = 15;
            }
            else
            {
                minRooms = 16;
                maxRooms = 20;
            }

            int targetRooms = Math.Min(roomCount, Width * Height);
            // If roomCount passed in is a placeholder, pick a value in desired range deterministically.
            if (roomCount <= 1 || roomCount > Width * Height)
            {
                targetRooms = rng.Next(minRooms, Math.Min(maxRooms, Width * Height) + 1);
            }
            else
            {
                // clamp to the desired band if caller provided a specific number
                targetRooms = Math.Clamp(roomCount, minRooms, Math.Min(maxRooms, Width * Height));
            }

            // Starting position: choose somewhere within the central 5x5 region.
            // Compute offset for central 5x5 block (works for 10x5 or 5x10).
            int offsetX = Math.Max(0, (Width - 5) / 2);
            int offsetY = Math.Max(0, (Height - 5) / 2);

            StartX = offsetX + rng.Next(0, Math.Min(5, Width - offsetX));
            StartY = offsetY + rng.Next(0, Math.Min(5, Height - offsetY));

            DebugLogger.Log($"Generating level {LevelNumber} size {Width}x{Height} orientation={(horizontal ? "H" : "V")} targetRooms={targetRooms} start=({StartX},{StartY})");

            // Generate connected path from start
            var generator = new LevelGenerator(Seed, Width, Height);
            Rooms = generator.Generate(targetRooms, StartX, StartY);

            // Place boss room at the furthest walkable room from start
            PlaceBossRoom(StartX, StartY);

            // Count actual walkable rooms
            TotalRooms = CountWalkableRooms();
            RoomsExplored = 0;

            DebugLogger.Log($"Level generated: {TotalRooms} walkable rooms");
            DebugPrintLevel(); // Show the layout
        }

        private void PlaceBossRoom(int startX, int startY)
        {
            // Use BFS to find the furthest walkable room by actual path distance
            var distances = CalculatePathDistances(startX, startY);

            int maxDistance = 0;
            int bossX = startX;
            int bossY = startY;

            foreach (var kvp in distances)
            {
                var (x, y) = kvp.Key;
                int distance = kvp.Value;

                // Skip starting position
                if (x == startX && y == startY)
                {
                    continue;
                }

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bossX = x;
                    bossY = y;
                }
            }

            // Verify boss room is not at starting position
            if (bossX == startX && bossY == startY)
            {
                DebugLogger.Log($"ERROR: Boss room would be at start position! Finding alternative...");

                // Find ANY other walkable room
                bool foundAlternative = false;
                for (int x = 0; x < Width && !foundAlternative; x++)
                {
                    for (int y = 0; y < Height && !foundAlternative; y++)
                    {
                        if (!Rooms[x, y].IsBlocked && (x != startX || y != startY))
                        {
                            bossX = x;
                            bossY = y;
                            foundAlternative = true;
                            DebugLogger.Log($"Alternative boss room found at ({bossX},{bossY})");
                        }
                    }
                }

                if (!foundAlternative)
                {
                    DebugLogger.Log($"WARNING: Could not find alternative boss room! Only 1 walkable room exists!");
                }
            }

            // Mark as boss room with description from JSON
            BossRoomX = bossX;
            BossRoomY = bossY;
            Rooms[bossX, bossY].IsBossRoom = true;
            Rooms[bossX, bossY].HasEncounter = true; // ENSURE THIS IS SET
            Rooms[bossX, bossY].IsBlocked = false;

            var rng = new Random(Seed);
            Rooms[bossX, bossY].Description = RoomDescriptionManager.GetRandomBossRoom(rng);

            DebugLogger.Log($"Boss room placed at ({bossX}, {bossY})");
            DebugLogger.Log($"Boss room - IsBossRoom: {Rooms[bossX, bossY].IsBossRoom}");
            DebugLogger.Log($"Boss room - HasEncounter: {Rooms[bossX, bossY].HasEncounter}");
            DebugLogger.Log($"Boss room - IsBlocked: {Rooms[bossX, bossY].IsBlocked}");
        }
        // BFS to calculate actual path distances from start to all reachable rooms
        private Dictionary<(int x, int y), int> CalculatePathDistances(int startX, int startY)
        {
            var distances = new Dictionary<(int x, int y), int>();
            var queue = new Queue<(int x, int y, int dist)>();
            var visited = new HashSet<(int x, int y)>();

            queue.Enqueue((startX, startY, 0));
            visited.Add((startX, startY));
            distances[(startX, startY)] = 0;

            while (queue.Count > 0)
            {
                var (x, y, dist) = queue.Dequeue();

                // Check all 4 orthogonal neighbors
                var neighbors = new[]
                {
                    (x + 1, y), (x - 1, y),
                    (x, y + 1), (x, y - 1)
                };

                foreach (var (nx, ny) in neighbors)
                {
                    // Check bounds
                    if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
                        continue;

                    // Check if walkable and not visited
                    if (Rooms[nx, ny].IsBlocked || visited.Contains((nx, ny)))
                        continue;

                    visited.Add((nx, ny));
                    distances[(nx, ny)] = dist + 1;
                    queue.Enqueue((nx, ny, dist + 1));
                }
            }

            DebugLogger.Log($"Path distance calculation: {distances.Count} reachable rooms from start");
            return distances;
        }
        private int CountWalkableRooms()
        {
            int count = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!Rooms[x, y].IsBlocked)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public void DebugPrintLevel()
        {
            DebugLogger.Log("=== Level Layout ===");
            for (int y = 0; y < Height; y++)
            {
                string row = "";
                for (int x = 0; x < Width; x++)
                {
                    if (x == StartX && y == StartY)
                        row += "S"; // Start
                    else if (x == BossRoomX && y == BossRoomY)
                        row += "B"; // Boss
                    else if (Rooms[x, y].IsBlocked)
                        row += "#"; // Wall
                    else
                        row += "."; // Room
                }
                DebugLogger.Log(row);
            }
        }
    }
}
