namespace Console_Dungeon.Models
{
    public class CombatResult
    {
        public int TotalDamage { get; set; }
        public int TotalGold { get; set; }
        public int TotalKills { get; set; }
        public List<string> DamageBreakdown { get; set; } = new List<string>();
        public string EncounterMessage { get; set; } = string.Empty;
        public string VictoryMessage { get; set; } = string.Empty;
        public string LootMessage { get; set; } = string.Empty;

        public string GetFullMessage()
        {
            return $"{EncounterMessage} {VictoryMessage}\n{LootMessage}";
        }
    }
}
