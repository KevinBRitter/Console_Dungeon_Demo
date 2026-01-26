using Console_Dungeon;
using Console_Dungeon.Models;
using Xunit;

namespace Console_Dungeon.Tests.GameLogic
{
    public class GameLoopTests
    {
        [Fact]
        public void GameLoop_InitializesWithGameState()
        {
            // Arrange
            var gameState = new GameState(12345);

            // Act
            var gameLoop = new GameLoop(gameState);

            // Assert
            Assert.NotNull(gameLoop);
            // GameLoop should load encounters on construction
            var encounters = EncounterManager.GetEncounters();
            Assert.NotNull(encounters);
        }

        [Fact]
        public void GameState_TurnCountIncrementsAsExpected()
        {
            // Arrange
            var gameState = new GameState(12345);
            int initialTurnCount = gameState.TurnCount;

            // Act
            gameState.TurnCount++;
            gameState.TurnCount++;

            // Assert
            Assert.Equal(initialTurnCount + 2, gameState.TurnCount);
        }

        [Fact]
        public void Player_CanCollectGold()
        {
            // Arrange
            var gameState = new GameState(12345);
            int initialGold = gameState.Player.Gold;

            // Act
            gameState.Player.Gold += 25;

            // Assert
            Assert.Equal(initialGold + 25, gameState.Player.Gold);
        }

        [Fact]
        public void DungeonLevel_RoomsExploredIncrementsCorrectly()
        {
            // Arrange
            var gameState = new GameState(12345);
            int initialRooms = gameState.CurrentLevel.RoomsExplored;

            // Act
            gameState.CurrentLevel.RoomsExplored++;
            gameState.CurrentLevel.RoomsExplored++;

            // Assert
            Assert.Equal(initialRooms + 2, gameState.CurrentLevel.RoomsExplored);
        }

        [Fact]
        public void GameState_UpdateLastPlayed_ModifiesTimestamp()
        {
            // Arrange
            var gameState = new GameState(12345);
            var originalTimestamp = gameState.LastPlayedAt;
            System.Threading.Thread.Sleep(10);

            // Act
            gameState.UpdateLastPlayed();

            // Assert
            Assert.True(gameState.LastPlayedAt > originalTimestamp);
        }
    }
}
