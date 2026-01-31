using System.IO;
using Console_Dungeon.Managers;
using Xunit;

namespace Console_Dungeon.Tests.Managers
{
    public class ItemManagerTests
    {
        [Fact]
        public void LoadItems_CanLoadDefaultDataFile()
        {
            // Arrange - ensure items file exists (project includes Data/Items.json)
            string filePath = "Data/Items.json";
            ItemManager.LoadItems(filePath);

            // Act
            var item = ItemManager.GetItemById("rusty_sword");

            // Assert
            Assert.NotNull(item);
            Assert.Equal("rusty_sword", item.Id);
            Assert.True(item.AttackBonus >= 0);
        }
    }
}
