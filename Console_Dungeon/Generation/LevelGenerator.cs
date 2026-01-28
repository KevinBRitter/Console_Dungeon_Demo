using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using System;
using System.Collections.Generic;

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

            // Convert path to room grid
            return CreateRoomGrid(path);
        }

        private HashSet<(int x, int y)> GenerateConnectedPath(int startX, int startY, int targetCount)
        {
            const int maxAttempts = 100;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var path = TryGeneratePath(startX, startY, targetCount, _seed + attempt);

                if (path != null && path.Count == targetCount)
                {
                    return path;
                }
            }

            // Fallback: create simple path if generation fails
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
            int cx = startX;
            int cy = startY;

            path.Add((cx, cy));

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
                if (dirIdx == 0)
                {
                    spiralExpansion++;
                    cx = Math.Clamp(startX - spiralExpansion, 0, _width - 1);
                    cy = Math.Clamp(startY - spiralExpansion, 0, _height - 1);
                }
            }

            return path;
        }

        private Room[,] CreateRoomGrid(HashSet<(int x, int y)> path)
        {
            var rooms = new Room[_width, _height];
            var rng = new Random(_seed);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (path.Contains((x, y)))
                    {
                        // Walkable room - use random description from JSON
                        string description = RoomDescriptionManager.GetRandomStandardRoom(rng);
                        rooms[x, y] = new Room(description, hasEncounter: true, isBlocked: false);
                    }
                    else
                    {
                        // Blocked room - use random blocked description from JSON
                        string description = RoomDescriptionManager.GetRandomBlockedRoom(rng, x, y);
                        rooms[x, y] = new Room(description, hasEncounter: false, isBlocked: true);
                    }
                }
            }

            return rooms;
        }
    }
}
