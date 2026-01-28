using System;
using System.Numerics;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class GameState
    {
        // Core game data
        public Player Player { get; set; }
        public DungeonLevel CurrentLevel { get; set; }
        public int Seed { get; set; }

        // Meta information
        public DateTime CreatedAt { get; set; }
        public DateTime LastPlayedAt { get; set; }
        public int TurnCount { get; set; }

        // Game flags (for quest states, unlocks, etc.)
        public Dictionary<string, bool> Flags { get; set; }

        // Constructor for new game
        public GameState(int seed)
        {
            Seed = seed;
            CreatedAt = DateTime.Now;
            LastPlayedAt = DateTime.Now;
            TurnCount = 0;
            Flags = new Dictionary<string, bool>();

            // Initialize player and first level
            Player = new Player("Adventurer");

            // Generate level: 5x5 grid with a random connected path of 10 rooms.
            int gridW = 5;
            int gridH = 5;
            int roomsThisLevel = 10;
            CurrentLevel = new DungeonLevel(1, seed, gridW, gridH, roomsThisLevel);

            // Place player in the center of the grid (the generator starts here).
            // TODO: This isn't working as expected the player is placed top left and disconnected from the level
            Player.PositionX = CurrentLevel.Width / 2;
            Player.PositionY = CurrentLevel.Height / 2;

            // Mark starting room visited if it's not blocked. If blocked for some reason, find nearest walkable.
            var startRoom = CurrentLevel.GetRoom(Player.PositionX, Player.PositionY);
            if (startRoom.IsBlocked)
            {
                // find first walkable cell (shouldn't normally happen because generator starts at center)
                bool found = false;
                for (int x = 0; x < CurrentLevel.Width && !found; x++)
                {
                    for (int y = 0; y < CurrentLevel.Height && !found; y++)
                    {
                        var r = CurrentLevel.GetRoom(x, y);
                        if (!r.IsBlocked)
                        {
                            Player.PositionX = x;
                            Player.PositionY = y;
                            startRoom = r;
                            found = true;
                        }
                    }
                }
            }

            if (!startRoom.Visited && !startRoom.IsBlocked)
            {
                startRoom.Visited = true;
                CurrentLevel.RoomsExplored = 1;
            }
            // CRITICAL: Ensure starting room is NOT a boss room
            if (startRoom.IsBossRoom)
            {
                DebugLogger.Log("ERROR: Starting room was marked as boss room! Fixing...");
                startRoom.IsBossRoom = false;
                startRoom.Description = "The entrance to the dungeon. Your journey begins here.";
            }
        }

        // Parameterless constructor for deserialization
        public GameState()
        {
            Flags = new Dictionary<string, bool>();
            Player = new Player();
            CurrentLevel = new DungeonLevel();
        }

        // Update last played timestamp
        public void UpdateLastPlayed()
        {
            LastPlayedAt = DateTime.Now;
        }
    }
}
