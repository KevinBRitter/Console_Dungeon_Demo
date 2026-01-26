using Console_Dungeon.Models;
using Xunit;

namespace Console_Dungeon.Tests.Integration
{
    public class GameFlowTests
    {
        [Fact]
        public void CompleteGameFlow_NewGameToExploration()
        {
            // Arrange - Create a new game
            int seed = 12345;
            var gameState = new GameState(seed);

            // Assert initial state
            Assert.Equal(100, gameState.Player.Health);
            Assert.Equal(0, gameState.Player.Gold);
            Assert.Equal(0, gameState.CurrentLevel.RoomsExplored);
            Assert.True(gameState.Player.IsAlive);

            // Act - Simulate exploration
            gameState.CurrentLevel.RoomsExplored++;
            gameState.Player.Gold += 10; // Find treasure
            gameState.TurnCount++;

            // Assert after first room
            Assert.Equal(1, gameState.CurrentLevel.RoomsExplored);
            Assert.Equal(10, gameState.Player.Gold);
            Assert.Equal(1, gameState.TurnCount);

            // Act - Simulate combat
            gameState.Player.TakeDamage(15);
            gameState.Player.Gold += 5; // Loot enemy
            gameState.CurrentLevel.RoomsExplored++;
            gameState.TurnCount++;

            // Assert after combat
            Assert.Equal(85, gameState.Player.Health);
            Assert.Equal(15, gameState.Player.Gold);
            Assert.Equal(2, gameState.CurrentLevel.RoomsExplored);
            Assert.True(gameState.Player.IsAlive);

            // Act - Simulate healing
            gameState.Player.Heal(20);
            gameState.TurnCount++;

            // Assert after healing
            Assert.Equal(100, gameState.Player.Health); // Should cap at MaxHealth
            Assert.Equal(3, gameState.TurnCount);
        }

        [Fact]
        public void PlayerDeath_SetsIsAliveToFalse()
        {
            // Arrange
            var gameState = new GameState(123);
            gameState.Player.Health = 10;

            // Act - Take lethal damage
            gameState.Player.TakeDamage(50);

            // Assert
            Assert.Equal(0, gameState.Player.Health);
            Assert.False(gameState.Player.IsAlive);
        }

        [Fact]
        public void LevelCompletion_TriggersWhenAllRoomsExplored()
        {
            // Arrange
            var gameState = new GameState(123);

            // Act - Explore all rooms
            while (gameState.CurrentLevel.RoomsExplored < gameState.CurrentLevel.TotalRooms)
            {
                gameState.CurrentLevel.RoomsExplored++;
            }

            // Assert
            Assert.Equal(gameState.CurrentLevel.TotalRooms, gameState.CurrentLevel.RoomsExplored);
            Assert.True(gameState.CurrentLevel.IsComplete);
        }

        [Fact]
        public void MultipleGameStates_AreIndependent()
        {
            // Arrange
            var gameState1 = new GameState(111);
            var gameState2 = new GameState(222);

            // Act
            gameState1.Player.Gold = 100;
            gameState1.Player.TakeDamage(50);

            gameState2.Player.Gold = 200;
            gameState2.Player.TakeDamage(10);

            // Assert - States are independent
            Assert.Equal(100, gameState1.Player.Gold);
            Assert.Equal(200, gameState2.Player.Gold);
            Assert.Equal(50, gameState1.Player.Health);
            Assert.Equal(90, gameState2.Player.Health);
        }
    }
}
