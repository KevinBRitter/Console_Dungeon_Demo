using System;
using System.Collections.Generic;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class DungeonLevel
    {
        public int LevelNumber { get; set; }
        public int Seed { get; set; }
        public bool IsExplored { get; set; }
        public bool IsBossDefeated { get; set; }

        // Grid
        public int Width { get; set; }
        public int Height { get; set; }
        public Room[,] Rooms { get; set; }

        // Simple room/encounter tracking
        public int RoomsExplored { get; set; }
        public int TotalRooms { get; set; }

        // Constructor for new level with explicit size and target walk length (number of valid rooms)
        // startRoomsCount: how many walkable rooms to carve (e.g., 10 for level 1)
        // The generation creates a single connected path of exactly startRoomsCount rooms,
        // starting from the provided start coordinates (center by default).
        public DungeonLevel(int levelNumber, int seed, int width = 5, int height = 5, int startRoomsCount = 10)
        {
            LevelNumber = levelNumber;
            Seed = seed;
            IsExplored = false;
            IsBossDefeated = false;

            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Rooms = new Room[Width, Height];

            // Initialize all cells as blocked (walls) first
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Rooms[x, y] = new Room($"Blocked space ({x},{y})", hasEncounter: false, isBlocked: true);
                }
            }

            // Ensure requested total rooms doesn't exceed available grid cells
            int maxPossible = Width * Height;
            int target = Math.Min(startRoomsCount, maxPossible);

            // Start in center by default
            int startX = Width / 2;
            int startY = Height / 2;

            // Generate a connected path of 'target' walkable rooms starting at center.
            GenerateConnectedPath(startX, startY, target, seed);

            RoomsExplored = 0;
            TotalRooms = target;
        }

        // Parameterless constructor for deserialization (creates full open grid)
        public DungeonLevel()
        {
            Width = 5;
            Height = 5;
            Rooms = new Room[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Rooms[x, y] = new Room($"A dim chamber at ({x},{y}).", isBlocked: false);
                }
            }

            TotalRooms = Width * Height;
        }

        public Room GetRoom(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException(nameof(x), "Room coordinates out of range.");
            return Rooms[x, y];
        }

        // Helper property
        public bool IsComplete => RoomsExplored >= TotalRooms || IsBossDefeated;

        // Generate a connected path of exactly 'targetCount' cells starting from (startX,startY).
        // Algorithm: randomized depth-first carve with backtracking. If an attempt fails to reach the
        // target count (rare for small grids), retry up to a limited number of attempts with different RNG.
        private void GenerateConnectedPath(int startX, int startY, int targetCount, int seed)
        {
            const int maxAttempts = 100;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var rng = new Random(seed + attempt);

                var path = new HashSet<(int x, int y)>();
                var stack = new List<(int x, int y)>();

                path.Add((startX, startY));
                stack.Add((startX, startY));

                while (path.Count < targetCount && stack.Count > 0)
                {
                    var current = stack[stack.Count - 1];
                    var neighbors = GetOrthogonalNeighbors(current.x, current.y);

                    // filter neighbors that are not yet in the path
                    var candidates = new List<(int x, int y)>();
                    foreach (var n in neighbors)
                    {
                        if (!path.Contains(n))
                            candidates.Add(n);
                    }

                    if (candidates.Count == 0)
                    {
                        // backtrack
                        stack.RemoveAt(stack.Count - 1);
                        continue;
                    }

                    // pick random neighbor and carve it
                    var chosen = candidates[rng.Next(candidates.Count)];
                    path.Add(chosen);
                    stack.Add(chosen);
                }

                if (path.Count == targetCount)
                {
                    // Carve path into Rooms: mark path cells as walkable, others remain blocked.
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            if (path.Contains((x, y)))
                            {
                                Rooms[x, y] = new Room($"A dim chamber at ({x},{y}).", hasEncounter: true, isBlocked: false);
                            }
                            else
                            {
                                Rooms[x, y] = new Room($"Stone wall ({x},{y}).", hasEncounter: false, isBlocked: true);
                            }
                        }
                    }

                    return; // successful generation
                }

                // otherwise retry with a different attempt seed
            }

            // Fallback: if generation failed after attempts, carve a simple plus-shaped path around center
            var fallbackPath = new HashSet<(int, int)>();
            int cx = startX, cy = startY;
            fallbackPath.Add((cx, cy));
            int added = 1;
            var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            int dirIdx = 0;
            while (added < targetCount)
            {
                var nx = cx + dirs[dirIdx].dx;
                var ny = cy + dirs[dirIdx].dy;
                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height && !fallbackPath.Contains((nx, ny)))
                {
                    fallbackPath.Add((nx, ny));
                    added++;
                }
                dirIdx = (dirIdx + 1) % dirs.Length;
                if (dirIdx == 0 && added == 1) // expand outward if needed
                {
                    cx = Math.Max(0, cx - 1);
                    cy = Math.Max(0, cy - 1);
                }
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (fallbackPath.Contains((x, y)))
                    {
                        Rooms[x, y] = new Room($"A dim chamber at ({x},{y}).", hasEncounter: true, isBlocked: false);
                    }
                    else
                    {
                        Rooms[x, y] = new Room($"Stone wall ({x},{y}).", hasEncounter: false, isBlocked: true);
                    }
                }
            }
        }

        private List<(int x, int y)> GetOrthogonalNeighbors(int x, int y)
        {
            var list = new List<(int, int)>();
            if (x > 0) list.Add((x - 1, y));
            if (x < Width - 1) list.Add((x + 1, y));
            if (y > 0) list.Add((x, y - 1));
            if (y < Height - 1) list.Add((x, y + 1));
            return list;
        }
    }
}
