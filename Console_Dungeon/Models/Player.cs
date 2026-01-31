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

        // Inventory & Equipment (Phase 5)
        public const int MaxInventorySlots = 8;
        public List<Item> Inventory { get; set; } = new List<Item>();
        public Item? EquippedWeapon { get; set; }
        public Item? EquippedArmor { get; set; }

        // Track if player just leveled up (for showing level-up screen)
        [NonSerialized]
        private LevelUpInfo? _pendingLevelUp = null;

        public bool HasPendingLevelUp => _pendingLevelUp != null;

        public LevelUpInfo? GetAndClearLevelUp()
        {
            var levelUp = _pendingLevelUp;
            _pendingLevelUp = null;
            return levelUp;
        }

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
                // Fallback if class not found (preserve original defaults)
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

        // Public helper to update identity/class while preserving the existing Player instance.
        // This lets callers reuse the GameState-created Player (and its Position) instead of replacing it.
        public void SetNameAndClass(string name, PlayerClass playerClass)
        {
            Name = name;
            Class = playerClass;

            // Re-apply class stats so the existing instance gets correct starting stats.
            ApplyClassStats(playerClass);
        }

        // Helper methods
        public bool IsAlive => Health > 0;

        // TakeDamage now uses effective defense (base + equipped armor)
        public void TakeDamage(int damage)
        {
            int effectiveDefense = GetEffectiveDefense();
            int actualDamage = Math.Max(1, damage - effectiveDefense);
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
                // Track old stats
                int oldMaxHealth = MaxHealth;
                int oldAttack = Attack;
                int oldDefense = Defense;
                int oldMaxStamina = MaxStamina;
                int oldMaxMana = MaxMana;

                // Increase level
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

                // Store level-up info for display
                _pendingLevelUp = new LevelUpInfo
                {
                    NewLevel = Level,
                    HealthGain = MaxHealth - oldMaxHealth,
                    AttackGain = Attack - oldAttack,
                    DefenseGain = Defense - oldDefense,
                    StaminaGain = MaxStamina - oldMaxStamina,
                    ManaGain = MaxMana - oldMaxMana,
                    NewMaxHealth = MaxHealth,
                    NewAttack = Attack,
                    NewDefense = Defense,
                    NewMaxStamina = MaxStamina,
                    NewMaxMana = MaxMana
                };
            }
        }

        // Helper method to get XP progress as a percentage
        public float GetXPProgress()
        {
            if (ExperienceToNextLevel == 0) return 1.0f;
            return (float)Experience / ExperienceToNextLevel;
        }

        // Helper method to get XP progress bar
        public string GetXPProgressBar(int barLength = 20)
        {
            float progress = GetXPProgress();
            int filledLength = (int)(progress * barLength);

            string filled = new string('=', filledLength);
            string empty = new string(' ', barLength - filledLength);

            return $"[{filled}>{empty}] {Experience}/{ExperienceToNextLevel} XP";
        }

        // Inventory / Equipment helpers (Phase 5)

        // Effective attack includes weapon bonus
        public int GetEffectiveAttack()
        {
            return Attack + (EquippedWeapon?.AttackBonus ?? 0);
        }

        // Effective defense includes armor bonus
        public int GetEffectiveDefense()
        {
            return Defense + (EquippedArmor?.DefenseBonus ?? 0);
        }

        // Try to add an item to the player's possessions.
        // Auto-equips equipment if the corresponding slot is empty.
        // Returns true if the item was accepted (equipped or added to inventory).
        public bool AddItem(Item item)
        {
            if (item == null) return false;

            // Meta items unlock globally
            if (item.IsMeta)
            {
                MetaUnlockManager.Unlock(item.Id);
            }

            // Auto-equip equipment if slot empty
            if (item.Type == ItemType.Equipment)
            {
                if (item.Slot == EquipmentSlot.Weapon && EquippedWeapon == null)
                {
                    EquippedWeapon = item;
                    return true;
                }
                if (item.Slot == EquipmentSlot.Armor && EquippedArmor == null)
                {
                    EquippedArmor = item;
                    return true;
                }
            }

            if (Inventory.Count >= MaxInventorySlots)
            {
                // Inventory full — reject item (caller may show a message / drop)
                return false;
            }

            Inventory.Add(item);
            return true;
        }

        // Equip an item from inventory by id. Returns true if equipped.
        public bool EquipFromInventory(string itemId)
        {
            int idx = Inventory.FindIndex(i => i.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;

            var item = Inventory[idx];
            if (item.Type != Enums.ItemType.Equipment) return false;

            if (item.Slot == Enums.EquipmentSlot.Weapon)
            {
                var prev = EquippedWeapon;
                EquippedWeapon = item;
                Inventory.RemoveAt(idx);
                if (prev != null) Inventory.Add(prev);
                return true;
            }

            if (item.Slot == Enums.EquipmentSlot.Armor)
            {
                var prev = EquippedArmor;
                EquippedArmor = item;
                Inventory.RemoveAt(idx);
                if (prev != null) Inventory.Add(prev);
                return true;
            }

            return false;
        }

        // Use a consumable from inventory by id. Returns true if consumed.
        public bool UseConsumable(string itemId)
        {
            int idx = Inventory.FindIndex(i => i.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;

            var item = Inventory[idx];
            if (item.Type != Enums.ItemType.Consumable) return false;

            if (item.HealAmount > 0)
            {
                Heal(item.HealAmount);
            }

            Inventory.RemoveAt(idx);
            return true;
        }
    }
}
