namespace Console_Dungeon.Models
{
    [Serializable]
    public class EncounterData
    {
        public List<string> TreasureEncounters { get; set; } = new List<string>();
        public List<string> EmptyRooms { get; set; } = new List<string>();
        public Dictionary<string, EnemyData> Enemies { get; set; } = new Dictionary<string, EnemyData>();
    }

    [Serializable]
    public class EnemyData
    {
        public string Name { get; set; } = string.Empty;
        public int DamageMin { get; set; }
        public int DamageMax { get; set; }
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
        public List<string> EncounterMessages { get; set; } = new List<string>();
        public List<string> LootMessages { get; set; } = new List<string>();
    }
}
