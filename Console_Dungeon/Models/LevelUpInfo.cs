namespace Console_Dungeon.Models
{
    /// <summary>
    /// Tracks stat changes during a level-up for display purposes
    /// </summary>
    public class LevelUpInfo
    {
        public int NewLevel { get; set; }
        public int HealthGain { get; set; }
        public int AttackGain { get; set; }
        public int DefenseGain { get; set; }
        public int StaminaGain { get; set; }
        public int ManaGain { get; set; }

        public int NewMaxHealth { get; set; }
        public int NewAttack { get; set; }
        public int NewDefense { get; set; }
        public int NewMaxStamina { get; set; }
        public int NewMaxMana { get; set; }
    }
}