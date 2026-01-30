using Console_Dungeon.Enums;
using Console_Dungeon.Managers;
using Console_Dungeon.Models;

namespace Console_Dungeon.Generation
{
    public class LevelGenerator
    {
        private readonly int _seed;
        private readonly int _width;
        private readonly int _height;

        public LevelGenerator(int seed, int width, int height)
        {
            _seed = seed;
            _width = Math.Max(1, width);
            _height = Math.Max(1, height);

            // Ensure descriptions are loaded
            RoomDescriptionManager.LoadDescriptions();
        }

        public Room[,] Generate(int targetRoomCount, int startX, int startY)
        {
            // Ensure target doesn't exceed grid capacity
            int maxPossible = _width * _height;
            int target = Math.Min(targetRoomCount, maxPossible);

            // Try to generate a connected path
            var path = GenerateConnectedPath(startX, startY, target);

            // CRITICAL: Ensure starting position is in the path
            if (!path.Contains((startX, startY)))
            {
                DebugLogger.Log($"WARNING: Start position ({startX},{startY}) not in generated path! Adding it.");
                path.Add((startX, startY));
            }

            // Convert path to room grid
            return CreateRoomGrid(path, startX, startY);
        }

        private Room[,] CreateRoomGrid(HashSet<(int x, int y)> path, int startX, int startY)
        {
            var rooms = new Room[_width, _height];
            var rng = new Random(_seed);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (path.Contains((x, y)))
                    {
                        // Walkable room
                        string description;

                        if (x == startX && y == startY)
                        {
                            description = "The entrance to the dungeon. Your journey begins here.";
                            rooms[x, y] = new Room(description, hasEncounter: false, isBlocked: false);
                            // Start room: no encounter
                            rooms[x, y].Encounter = EncounterKind.None;
                            rooms[x, y].HasEncounter = false;
                        }
                        else
                        {
                            description = RoomDescriptionManager.GetRandomStandardRoom(rng);
                            // Assign encounter kind deterministically based on RNG and the seed.
                            // Maintain original probabilities:
                            // 1-3 => Treasure (30%), 4-8 => Combat (50%), 9-10 => Empty (20%)
                            int roll = rng.Next(1, 11);
                            EncounterKind kind;
                            if (roll <= 3)
                            {
                                kind = EncounterKind.Treasure;
                            }
                            else if (roll <= 8)
                            {
                                kind = EncounterKind.Combat;
                            }
                            else
                            {
                                kind = EncounterKind.None;
                            }

                            rooms[x, y] = new Room(description, hasEncounter: kind != EncounterKind.None, isBlocked: false);
                            rooms[x, y].Encounter = kind;
                            rooms[x, y].HasEncounter = (kind != EncounterKind.None);
                        }
                    }
                    else
                    {
                        // Blocked room
                        string description = RoomDescriptionManager.GetRandomBlockedRoom(rng, x, y);
                        rooms[x, y] = new Room(description, hasEncounter: false, isBlocked: true);
                        rooms[x, y].Encounter = EncounterKind.None;
                        rooms[x, y].HasEncounter = false;
                    }
                }
            }

            return rooms;
        }

        private HashSet<(int x, int y)> GenerateConnectedPath(int startX, int startY, int targetCount)
        {
            const int maxAttempts = 100;

            DebugLogger.Log($"Generating path: start=({startX},{startY}), target={targetCount} rooms");

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var path = TryGeneratePath(startX, startY, targetCount, _seed + attempt);

                if (path != null && path.Count == targetCount)
                {
                    DebugLogger.Log($"Path generated successfully on attempt {attempt + 1}: {path.Count} rooms");

                    // Verify starting position is in path
                    if (!path.Contains((startX, startY)))
                    {
                        DebugLogger.Log($"ERROR: Start position not in path! Adding it.");
                        path.Add((startX, startY));
                    }

                    return path;
                }
            }

            // Fallback: create simple path if generation fails
            DebugLogger.Log($"Path generation failed after {maxAttempts} attempts, using fallback");
            return GenerateFallbackPath(startX, startY, targetCount);
        }

        private HashSet<(int x, int y)>? TryGeneratePath(int startX, int startY, int targetCount, int seed)
        {
            var rng = new Random(seed);
            var path = new HashSet<(int x, int y)>();
            var stack = new List<(int x, int y)>();

            path.Add((startX, startY));
            stack.Add((startX, startY));

            while (path.Count < targetCount && stack.Count > 0)
            {
                var current = stack[stack.Count - 1];
                var candidates = GetUnvisitedNeighbors(current.x, current.y, path);

                if (candidates.Count == 0)
                {
                    // Backtrack
                    stack.RemoveAt(stack.Count - 1);
                    continue;
                }

                // Pick random neighbor and carve it
                var chosen = candidates[rng.Next(candidates.Count)];
                path.Add(chosen);
                stack.Add(chosen);
            }

            return path.Count == targetCount ? path : null;
        }

        private List<(int x, int y)> GetUnvisitedNeighbors(int x, int y, HashSet<(int x, int y)> visited)
        {
            var candidates = new List<(int x, int y)>();
            var neighbors = GetOrthogonalNeighbors(x, y);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    candidates.Add(neighbor);
                }
            }

            return candidates;
        }

        private List<(int x, int y)> GetOrthogonalNeighbors(int x, int y)
        {
            var neighbors = new List<(int x, int y)>();

            if (x > 0) neighbors.Add((x - 1, y));
            if (x < _width - 1) neighbors.Add((x + 1, y));
            if (y > 0) neighbors.Add((x, y - 1));
            if (y < _height - 1) neighbors.Add((x, y + 1));

            return neighbors;
        }

        private HashSet<(int x, int y)> GenerateFallbackPath(int startX, int startY, int targetCount)
        {
            var path = new HashSet<(int x, int y)>();

            // Always start with the starting position
            path.Add((startX, startY));

            int cx = startX;
            int cy = startY;

            var directions = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            int dirIdx = 0;
            int spiralExpansion = 0;

            while (path.Count < targetCount)
            {
                var (dx, dy) = directions[dirIdx];
                int nx = cx + dx;
                int ny = cy + dy;

                if (nx >= 0 && nx < _width && ny >= 0 && ny < _height && !path.Contains((nx, ny)))
                {
                    path.Add((nx, ny));
                    cx = nx;
                    cy = ny;
                }

                dirIdx = (dirIdx + 1) % directions.Length;

                // Spiral outward if stuck
                if (dirIdx == 0 && path.Count < targetCount)
                {
                    spiralExpansion++;
                    cx = Math.Clamp(startX - spiralExpansion, 0, _width - 1);
                    cy = Math.Clamp(startY - spiralExpansion, 0, _height - 1);

                    // Make sure we don't spiral forever
                    if (spiralExpansion > Math.Max(_width, _height))
                    {
                        DebugLogger.Log($"Fallback spiral exceeded grid size, stopping at {path.Count} rooms");
                        break;
                    }
                }
            }

            DebugLogger.Log($"Fallback path generated: {path.Count} rooms");
            return path;
        }
    }
}
