using System;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class DungeonLevel
    {
        public int LevelNumber { get; set; }
        public int Seed { get; set; }
        public bool IsExplored { get; set; }
        public bool IsBossDefeated { get; set; }

        // Simple room/encounter tracking
        public int RoomsExplored { get; set; }
        public int TotalRooms { get; set; }

        // Constructor for new level
        public DungeonLevel(int levelNumber, int seed)
        {
            LevelNumber = levelNumber;
            Seed = seed;
            IsExplored = false;
            IsBossDefeated = false;
            RoomsExplored = 0;
            TotalRooms = 10; // Could be randomized based on seed
        }

        // Parameterless constructor for deserialization
        public DungeonLevel()
        {
            TotalRooms = 10;
        }

        // Helper property
        public bool IsComplete => RoomsExplored >= TotalRooms || IsBossDefeated;
    }
}