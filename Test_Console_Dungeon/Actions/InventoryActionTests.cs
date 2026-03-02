using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using Xunit;

namespace Console_Dungeon.Tests.Actions
{
    public class InventoryActionTests
    {
        private static Player MakePlayer()
        {
            ItemManager.LoadItems("Data/Items.json");
            return new Player("Tester");
        }

        [Fact]
        public void UsePotion_HealsForItemHealAmount()
        {
            // Verifies healing is driven by the item's HealAmount,
            // not a hardcoded constant (was 20 from rest; minor_healing is also 20
            // but major_healing is 50, proving the value comes from the item).
            var player = MakePlayer();
            var potion = ItemManager.GetItemById("major_healing")!;

            player.TakeDamage(80);
            int hpBefore = player.Health;
            player.AddItem(potion);

            player.UseConsumable("major_healing");

            int healed = player.Health - hpBefore;
            Assert.Equal(potion.HealAmount, healed);
        }

        [Fact]
        public void UsePotion_RemovesPotionFromInventory()
        {
            var player = MakePlayer();
            var potion = ItemManager.GetItemById("minor_healing")!;
            player.AddItem(potion);
            Assert.Equal(1, player.Inventory.Count);

            player.TakeDamage(10);
            player.UseConsumable("minor_healing");

            Assert.Equal(0, player.Inventory.Count);
        }

        [Fact]
        public void DropItem_RemovesItemFromInventory()
        {
            // Simulates what HandleDrop does: Inventory.Remove(item)
            var player = MakePlayer();
            var potion = ItemManager.GetItemById("minor_healing")!;
            player.AddItem(potion);
            Assert.Equal(1, player.Inventory.Count);

            player.Inventory.Remove(potion);

            Assert.Equal(0, player.Inventory.Count);
            Assert.DoesNotContain(potion, player.Inventory);
        }

        [Fact]
        public void EquipFromInventory_SwapsWeaponCorrectly()
        {
            var player = MakePlayer();
            var first = ItemManager.GetItemById("rusty_sword")!;
            var second = ItemManager.GetItemById("old_staff")!;

            // First sword auto-equips (empty slot)
            player.AddItem(first);
            Assert.Equal("rusty_sword", player.EquippedWeapon!.Id);

            // Second goes to inventory (slot occupied)
            player.AddItem(second);
            Assert.Equal(1, player.Inventory.Count);

            // Equip from inventory — old weapon should move to inventory
            bool swapped = player.EquipFromInventory("old_staff");

            Assert.True(swapped);
            Assert.Equal("old_staff", player.EquippedWeapon!.Id);
            Assert.Equal(1, player.Inventory.Count);
            Assert.Equal("rusty_sword", player.Inventory[0].Id);
        }

        [Fact]
        public void EquipFromInventory_WithEmptySlot_NoDisplacement()
        {
            // Equipping when the slot is empty should not add anything to inventory
            var player = MakePlayer();
            var armor = ItemManager.GetItemById("leather_armor")!;

            // Put armor in inventory without auto-equipping
            // (force it in directly since AddItem auto-equips empty slots)
            player.Inventory.Add(armor);
            Assert.Null(player.EquippedArmor);
            Assert.Equal(1, player.Inventory.Count);

            bool equipped = player.EquipFromInventory("leather_armor");

            Assert.True(equipped);
            Assert.NotNull(player.EquippedArmor);
            Assert.Equal(0, player.Inventory.Count); // no displacement into inventory
        }
    }
}
