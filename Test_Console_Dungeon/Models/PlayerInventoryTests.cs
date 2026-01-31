using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using Xunit;

namespace Console_Dungeon.Tests.Models
{
    public class PlayerInventoryTests
    {
        [Fact]
        public void AddItem_AutoEquipsWeaponWhenEmpty()
        {
            // Arrange
            ItemManager.LoadItems("Data/Items.json");
            var player = new Player("Tester");
            var sword = ItemManager.GetItemById("rusty_sword");

            // Act
            bool accepted = player.AddItem(sword!);

            // Assert
            Assert.True(accepted);
            Assert.NotNull(player.EquippedWeapon);
            Assert.Equal("rusty_sword", player.EquippedWeapon!.Id);
        }

        [Fact]
        public void UseConsumable_HealsPlayer()
        {
            ItemManager.LoadItems("Data/Items.json");
            var player = new Player("Tester");
            var potion = ItemManager.GetItemById("minor_healing");

            // Damage player then consume
            player.TakeDamage(30);
            Assert.True(player.Health < player.MaxHealth);

            player.AddItem(potion!);
            bool used = player.UseConsumable("minor_healing");

            Assert.True(used);
            Assert.True(player.Health <= player.MaxHealth);
        }
    }
}
