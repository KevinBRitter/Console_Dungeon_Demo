namespace Console_Dungeon.Models
{
    [Serializable]
    public class CharacterClassCollection
    {
        public List<CharacterClassData> Classes { get; set; } = new List<CharacterClassData>();
    }

    [Serializable]
    public class CharacterClassData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FlavorText { get; set; } = string.Empty;
        public ClassStats StartingStats { get; set; } = new ClassStats();
        public ClassStats LevelUpGains { get; set; } = new ClassStats();
    }

    [Serializable]
    public class ClassStats
    {
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MaxStamina { get; set; }
        public int MaxMana { get; set; }
    }
}
