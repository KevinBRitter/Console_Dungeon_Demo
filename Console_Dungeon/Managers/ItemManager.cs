using System.Text.Json;
using Console_Dungeon.Models;

namespace Console_Dungeon.Managers
{
    public static class ItemManager
    {
        private static ItemCollection? _items;

        public static void LoadItems(string filePath = "Data/Items.json")
        {
            try
            {
                string json = File.ReadAllText(filePath);
                _items = JsonSerializer.Deserialize<ItemCollection>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_items == null)
                {
                    DebugLogger.Log($"ItemManager: '{filePath}' deserialized to null; falling back to built-in items.");
                    _items = CreateDefaultItems();
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"ItemManager: failed to load '{filePath}' ({ex.Message}); falling back to built-in items.");
                _items = CreateDefaultItems();
            }
        }

        public static ItemCollection GetItems()
        {
            if (_items == null)
                LoadItems();
            return _items ?? CreateDefaultItems();
        }

        public static Item? GetItemById(string id)
        {
            var coll = GetItems();
            return coll.Items.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase))?.Clone();
        }

        // Safety net for a missing or malformed Data/Items.json.
        // This is a MIRROR of that file - keep the two in sync when adding or editing items.
        private static ItemCollection CreateDefaultItems()
        {
            return new ItemCollection
            {
                Items = new List<Item>
                {
                    new Item
                    {
                        Id = "rusty_sword",
                        Name = "Rusty Sword",
                        Type = Enums.ItemType.Equipment,
                        Slot = Enums.EquipmentSlot.Weapon,
                        AttackBonus = 3,
                        Description = "An old sword; better than bare fists."
                    },
                    new Item
                    {
                        Id = "old_staff",
                        Name = "Old Staff",
                        Type = Enums.ItemType.Equipment,
                        Slot = Enums.EquipmentSlot.Weapon,
                        AttackBonus = 2,
                        Description = "A worn staff. Mages appreciate the heft."
                    },
                    new Item
                    {
                        Id = "leather_armor",
                        Name = "Leather Armor",
                        Type = Enums.ItemType.Equipment,
                        Slot = Enums.EquipmentSlot.Armor,
                        DefenseBonus = 2,
                        Description = "Light armor that offers minimal protection."
                    },
                    new Item
                    {
                        Id = "iron_ring",
                        Name = "Iron Ring",
                        Type = Enums.ItemType.Equipment,
                        Slot = Enums.EquipmentSlot.Jewelry,
                        AttackBonus = 1,
                        Description = "A plain band. The metal hums faintly when you swing."
                    },
                    new Item
                    {
                        Id = "warding_amulet",
                        Name = "Warding Amulet",
                        Type = Enums.ItemType.Equipment,
                        Slot = Enums.EquipmentSlot.Jewelry,
                        DefenseBonus = 2,
                        Description = "A carved charm that turns aside the worst of a blow."
                    },
                    new Item
                    {
                        Id = "minor_healing",
                        Name = "Minor Healing Potion",
                        Type = Enums.ItemType.Consumable,
                        HealAmount = 20,
                        Description = "Restores a small amount of HP."
                    },
                    new Item
                    {
                        Id = "major_healing",
                        Name = "Major Healing Potion",
                        Type = Enums.ItemType.Consumable,
                        HealAmount = 50,
                        Description = "Restores a significant amount of HP."
                    },
                    new Item
                    {
                        Id = "divine_blade",
                        Name = "Divine Blade",
                        Type = Enums.ItemType.Equipment,
                        Slot = Enums.EquipmentSlot.Weapon,
                        AttackBonus = 10,
                        IsMeta = true,
                        Description = "A legendary blade. Its flavor text nods to the roguelike loop - it persists across runs once earned."
                    }
                }
            };
        }
    }

    public class ItemCollection
    {
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
