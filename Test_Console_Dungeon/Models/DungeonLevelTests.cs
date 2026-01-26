using Console_Dungeon.Models;
using Xunit;

namespace Console_Dungeon.Tests.Models
{
    public class DungeonLevelTests
    {
        [Fact]
        public void NewDungeonLevel_InitializesWithCorrectDefaults()
        {
            // Arrange & Act
            var level = new DungeonLevel(5, 12345);

            // Assert
            Assert.Equal(5, level.LevelNumber);
            Assert.Equal(12345, level.Seed);
            Assert.False(level.IsExplored);
            Assert.False(level.IsBossDefeated);
            Assert.Equal(0, level.RoomsExplored);
            Assert.Equal(10, level.TotalRooms);
            Assert.False(level.IsComplete);
        }

        [Fact]
        public void IsComplete_ReturnsTrueWhenAllRoomsExplored()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);

            // Act
            level.RoomsExplored = level.TotalRooms;

            // Assert
            Assert.True(level.IsComplete);
        }

        [Fact]
        public void IsComplete_ReturnsTrueWhenBossDefeated()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);
            level.RoomsExplored = 5; // Not all rooms explored

            // Act
            level.IsBossDefeated = true;

            // Assert
            Assert.True(level.IsComplete);
        }

        [Fact]
        public void IsComplete_ReturnsFalseWhenNeitherConditionMet()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);
            level.RoomsExplored = 5;
            level.IsBossDefeated = false;

            // Assert
            Assert.False(level.IsComplete);
        }

        [Fact]
        public void ParameterlessConstructor_CreatesValidDungeonLevel()
        {
            // Act
            var level = new DungeonLevel();

            // Assert
            Assert.Equal(10, level.TotalRooms);
        }
    }
}