using Console_Dungeon.Enums;
using Console_Dungeon.Managers;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class Player
    {
        // Identity
        public string Name { get; set; }
        public PlayerClass Class { get; set; }

        // Core Stats
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        // Optional Stats
        public int Stamina { get; set; }
        public int MaxStamina { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }

        // Position
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        // Progression
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        public int Gold { get; set; }
        public int Kills { get; set; }

        // Constructor with class
        public Player(string name, PlayerClass playerClass)
        {
            Name = name;
            Class = playerClass;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 100;
            Gold = 0;
            Kills = 0;
            PositionX = 0;
            PositionY = 0;

            // Load stats from JSON
            ApplyClassStats(playerClass);
        }

        // Backwards compatibility constructor
        public Player(string name) : this(name, PlayerClass.Warrior)
        {
        }

        // Deserialization constructor
        public Player()
        {
            Name = "Adventurer";
            Class = PlayerClass.Warrior;
            ApplyClassStats(PlayerClass.Warrior);
        }

        private void ApplyClassStats(PlayerClass playerClass)
        {
            var classData = CharacterClassManager.GetClassDataByEnum(playerClass);

            if (classData != null)
            {
                MaxHealth = classData.StartingStats.MaxHealth;
                Health = MaxHealth;
                Attack = classData.StartingStats.Attack;
                Defense = classData.StartingStats.Defense;
                MaxStamina = classData.StartingStats.MaxStamina;
                Stamina = MaxStamina;
                MaxMana = classData.StartingStats.MaxMana;
                Mana = MaxMana;
            }
            else
            {
                // Fallback if class not found
                MaxHealth = 100;
                Health = MaxHealth;
                Attack = 10;
                Defense = 5;
                MaxStamina = 100;
                Stamina = MaxStamina;
                MaxMana = 0;
                Mana = 0;
            }
        }

        // Helper methods
        public bool IsAlive => Health > 0;

        public void TakeDamage(int damage)
        {
            int actualDamage = Math.Max(1, damage - Defense);
            Health = Math.Max(0, Health - actualDamage);
        }

        public void Heal(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public void GainExperience(int amount)
        {
            Experience += amount;

            while (Experience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            var classData = CharacterClassManager.GetClassDataByEnum(Class);

            if (classData != null)
            {
                Level++;
                Experience -= ExperienceToNextLevel;
                ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.5f);

                // Apply level up gains from JSON
                MaxHealth += classData.LevelUpGains.MaxHealth;
                Attack += classData.LevelUpGains.Attack;
                Defense += classData.LevelUpGains.Defense;
                MaxStamina += classData.LevelUpGains.MaxStamina;
                MaxMana += classData.LevelUpGains.MaxMana;

                // Restore to full on level up
                Health = MaxHealth;
                Stamina = MaxStamina;
                Mana = MaxMana;
            }
        }
    }        
}
