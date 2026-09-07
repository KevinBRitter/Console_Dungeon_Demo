using Console_Dungeon.Managers;
using Console_Dungeon.Models;

namespace Console_Dungeon.Managers
{
    public static class LootManager
    {
        // Meta items (divine gifts) are deliberately excluded from every random pool.
        // They are earned, not found - see MetaUnlockManager.
        private static List<Item> ConsumablePool()
        {
            return ItemManager.GetItems().Items
                .Where(it => !it.IsMeta && it.Type == Enums.ItemType.Consumable)
                .ToList();
        }

        private static List<Item> EquipmentPool()
        {
            return ItemManager.GetItems().Items
                .Where(it => !it.IsMeta && it.Type == Enums.ItemType.Equipment)
                .ToList();
        }

        // Pick a random entry from a pool, or null when the pool is empty.
        private static Item? PickRandom(List<Item> pool, Random rng)
        {
            if (pool.Count == 0) return null;
            return pool[rng.Next(pool.Count)].Clone();
        }

        // Generate loot for combat. difficultyFactor scales number/quality of drops.
        // ensureNonEmpty guarantees at least some gold if nothing else generated.
        public static (int gold, List<Item> items, List<string> messages) GenerateCombatLoot(Random rng, int difficultyFactor = 1, bool ensureNonEmpty = true)
        {
            var items = new List<Item>();
            var messages = new List<string>();
            var consumables = ConsumablePool();
            var equipment = EquipmentPool();

            // Gold: base 5-20 scaled by difficulty
            int baseGold = rng.Next(5, 21);
            int gold = baseGold * Math.Max(1, difficultyFactor);

            // Chance to drop 0..n potions: up to difficultyFactor
            int potionsToTry = rng.Next(0, difficultyFactor + 1);
            for (int i = 0; i < potionsToTry; i++)
            {
                if (rng.NextDouble() >= 0.75) continue;

                var pot = PickRandom(consumables, rng);
                if (pot != null) items.Add(pot);
            }

            // Chance to drop weapons/armor (0..difficultyFactor)
            int equipAttempts = rng.Next(0, difficultyFactor + 1);
            for (int i = 0; i < equipAttempts; i++)
            {
                var pick = PickRandom(equipment, rng);
                if (pick == null) break;
                items.Add(pick);
            }

            // Ensure not empty after combat
            if (ensureNonEmpty && items.Count == 0 && gold <= 0)
            {
                gold = Math.Max(1, gold) + 5;
            }

            // Build messages
            if (gold > 0) messages.Add($"You find {gold} gold.");
            foreach (var it in items)
            {
                messages.Add($"You found: {it.Name} - {it.Description}");
            }

            return (gold, items, messages);
        }

        // Treasure room generation - more generous but may still be variable
        public static (int gold, List<Item> items, List<string> messages) GenerateTreasure(Random rng, bool ensureNonEmpty = true)
        {
            // Slightly larger gold and better chance at items
            int gold = rng.Next(10, 41);

            var items = new List<Item>();
            var messages = new List<string>();

            if (rng.NextDouble() < 0.8)
            {
                // 80% chance to include at least one consumable
                var pot = PickRandom(ConsumablePool(), rng);
                if (pot != null) items.Add(pot);
            }

            if (rng.NextDouble() < 0.5)
            {
                var equip = PickRandom(EquipmentPool(), rng);
                if (equip != null) items.Add(equip);
            }

            if (ensureNonEmpty && gold == 0 && items.Count == 0)
            {
                gold = 5;
            }

            if (gold > 0) messages.Add($"You find {gold} gold.");
            foreach (var it in items)
            {
                messages.Add($"You found: {it.Name} - {it.Description}");
            }

            return (gold, items, messages);
        }

        // Empty room may return nothing or small chance for a consumable
        public static (int gold, List<Item> items, List<string> messages) GenerateEmptyRoomLoot(Random rng)
        {
            var items = new List<Item>();
            var messages = new List<string>();
            int gold = 0;

            if (rng.NextDouble() < 0.15)
            {
                // Small chance to find copper
                gold = rng.Next(1, 6);
                messages.Add($"You find {gold} small coins hidden in the dust.");
            }
            if (rng.NextDouble() < 0.05)
            {
                var pot = PickRandom(ConsumablePool(), rng);
                if (pot != null)
                {
                    items.Add(pot);
                    messages.Add($"You discover a {pot.Name} tucked under rubble.");
                }
            }

            return (gold, items, messages);
        }
    }
}
