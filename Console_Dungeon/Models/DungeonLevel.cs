using Console_Dungeon.Generation;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class DungeonLevel
    {
        // Level metadata
        public int LevelNumber { get; set; }
        public int Seed { get; set; }
        public bool IsExplored { get; set; }
        public bool IsBossDefeated { get; set; }

        // Grid properties
        public int Width { get; set; }
        public int Height { get; set; }
        public Room[,] Rooms { get; set; }

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

            Width = Math.Max(1, width);
            Height = Math.Max(1, height);

            // Generate level layout
            GenerateLevel(roomCount);
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
            // Ensure room count doesn't exceed grid capacity
            int maxPossible = Width * Height;
            int targetRooms = Math.Min(roomCount, maxPossible);

            // Starting position (center of grid)
            int startX = Width / 2;
            int startY = Height / 2;

            // Generate connected path
            var generator = new LevelGenerator(Seed, Width, Height);
            Rooms = generator.Generate(targetRooms, startX, startY);

            // Place boss room at the furthest walkable room from start
            PlaceBossRoom(startX, startY);

            // Count actual walkable rooms
            TotalRooms = CountWalkableRooms();
            RoomsExplored = 0;
        }

        private void PlaceBossRoom(int startX, int startY)
        {
            // Find the furthest walkable room from start using Manhattan distance
            int maxDistance = 0;
            int bossX = startX;
            int bossY = startY;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!Rooms[x, y].IsBlocked)
                    {
                        int distance = Math.Abs(x - startX) + Math.Abs(y - startY);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            bossX = x;
                            bossY = y;
                        }
                    }
                }
            }

            // Mark as boss room with description from JSON
            BossRoomX = bossX;
            BossRoomY = bossY;
            Rooms[bossX, bossY].IsBossRoom = true;

            var rng = new Random(Seed);
            Rooms[bossX, bossY].Description = RoomDescriptionManager.GetRandomBossRoom(rng);
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
    }
}
