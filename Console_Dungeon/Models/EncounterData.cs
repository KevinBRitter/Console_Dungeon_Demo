namespace Console_Dungeon.Models
{
    [Serializable]
    public class EncounterData
    {
        public List<string> TreasureEncounters { get; set; } = new List<string>();
        public List<string> EmptyRooms { get; set; } = new List<string>();
        public List<string> AlreadySearched { get; set; } = new List<string>();
        public List<string> NeverHadEncounter { get; set; } = new List<string>();
        public Dictionary<string, EnemyType> EnemyTypes { get; set; } = new Dictionary<string, EnemyType>();
        public List<CombatEncounter> CombatEncounters { get; set; } = new List<CombatEncounter>();
    }

    [Serializable]
    public class EnemyType
    {
        public string Name { get; set; } = string.Empty;
        public int DamageMin { get; set; }
        public int DamageMax { get; set; }
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
    }

    [Serializable]
    public class CombatEncounter
    {
        public string Id { get; set; } = string.Empty;
        public int Weight { get; set; } = 10;
        public int MinLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 99;
        public bool IsBoss { get; set; } = false; // Default to false
        public List<EnemyGroup> Enemies { get; set; } = new List<EnemyGroup>();
        public List<string> EncounterMessages { get; set; } = new List<string>();
        public List<string> VictoryMessages { get; set; } = new List<string>();
        public List<string> LootMessages { get; set; } = new List<string>();
    }

    [Serializable]
    public class EnemyGroup
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
    }
}
