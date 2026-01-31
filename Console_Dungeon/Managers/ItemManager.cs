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
                    _items = CreateDefaultItems();
            }
            catch
            {
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
                        Id = "minor_healing",
                        Name = "Minor Healing Potion",
                        Type = Enums.ItemType.Consumable,
                        HealAmount = 20,
                        Description = "Restores a small amount of HP."
                    },
                    new Item
                    {
                        Id = "divine_blade",
                        Name = "Divine Blade",
                        Type = Enums.ItemType.Equipment,
                        Slot = Enums.EquipmentSlot.Weapon,
                        AttackBonus = 10,
                        IsMeta = true,
                        Description = "A legendary blade. It whispers of the roguelike loop."
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
