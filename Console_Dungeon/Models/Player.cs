using System;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class Player
    {
        // Identity
        public string Name { get; set; }

        // Stats
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        // Position in dungeon
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        // Progression
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Gold { get; set; }

        // Kill count (tracked by combat)
        public int Kills { get; set; }

        // Constructor for new player
        public Player(string name)
        {
            Name = name;
            Level = 1;
            MaxHealth = 100;
            Health = MaxHealth;
            Attack = 10;
            Defense = 5;
            Experience = 0;
            Gold = 0;
            PositionX = 0;
            PositionY = 0;
            Kills = 0;
        }

        // Parameterless constructor for deserialization
        public Player()
        {
            Name = "Adventurer";
            Kills = 0;
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
    }
}
