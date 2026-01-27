using System;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class Room
    {
        public bool Visited { get; set; }
        public string Description { get; set; }
        public bool HasEncounter { get; set; }
        public bool EncounterTriggered { get; set; } // NEW: Track if encounter was already triggered
        public bool IsBlocked { get; set; }
        public bool IsBossRoom { get; set; } // Boss room flag

        public Room(string description = "A cold stone chamber.", bool hasEncounter = true, bool isBlocked = false)
        {
            Visited = false;
            Description = description;
            HasEncounter = hasEncounter;
            EncounterTriggered = false; // Start as not triggered
            IsBlocked = isBlocked;
            IsBossRoom = false;
        }

        // Helper property for clarity
        public bool CanTriggerEncounter => HasEncounter && !EncounterTriggered;
    }
}
