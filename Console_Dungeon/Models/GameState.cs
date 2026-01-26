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
            CurrentLevel = new DungeonLevel(1, seed);
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
