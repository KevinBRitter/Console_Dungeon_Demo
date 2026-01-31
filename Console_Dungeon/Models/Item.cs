using System.Text.Json.Serialization;
using Console_Dungeon.Enums;

namespace Console_Dungeon.Models
{
    [Serializable]
    public class Item
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ItemType Type { get; set; } = ItemType.Consumable;
        public EquipmentSlot Slot { get; set; } = EquipmentSlot.None;
        public int AttackBonus { get; set; } = 0;
        public int DefenseBonus { get; set; } = 0;
        public int HealAmount { get; set; } = 0;
        public bool IsMeta { get; set; } = false;
        public string Description { get; set; } = string.Empty;

        // Shallow clone helper in case we need independent instances
        public Item Clone() => new Item
        {
            Id = Id,
            Name = Name,
            Type = Type,
            Slot = Slot,
            AttackBonus = AttackBonus,
            DefenseBonus = DefenseBonus,
            HealAmount = HealAmount,
            IsMeta = IsMeta,
            Description = Description
        };
    }
}
