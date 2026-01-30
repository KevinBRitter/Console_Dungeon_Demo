using Console_Dungeon.Enums;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class Room
    {
        public bool Visited { get; set; }
        public string Description { get; set; }

        // Backwards-compatible boolean flag retained for callers that used it.
        // Keep in sync with Encounter (set during construction / generation).
        public bool HasEncounter { get; set; }

        public EncounterKind Encounter { get; set; }

        public bool EncounterTriggered { get; set; } // Track if encounter was already triggered
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

            // Preserve previous default behavior: if hasEncounter true, default to Combat.
            Encounter = hasEncounter ? EncounterKind.Combat : EncounterKind.None;
        }

        // Helper property for clarity
        public bool CanTriggerEncounter => Encounter != EncounterKind.None && !EncounterTriggered;
    }
}
