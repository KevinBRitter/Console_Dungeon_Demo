using System;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class Room
    {
        public bool Visited { get; set; }
        public string Description { get; set; }
        public bool HasEncounter { get; set; }

        // New: blocked rooms represent walls / unreachable spaces
        public bool IsBlocked { get; set; }

        public Room(string description = "A cold stone chamber.", bool hasEncounter = true, bool isBlocked = false)
        {
            Visited = false;
            Description = description;
            HasEncounter = hasEncounter;
            IsBlocked = isBlocked;
        }
    }
}