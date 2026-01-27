using Console_Dungeon.Models;

namespace Console_Dungeon.Tests.Models
{
    public class GameStateTests
    {
        [Fact]
        public void NewGameState_InitializesWithCorrectDefaults()
        {
            // Arrange
            int seed = 12345;

            // Act
            var gameState = new GameState(seed);

            // Assert
            Assert.NotNull(gameState.Player);
            Assert.NotNull(gameState.CurrentLevel);
            Assert.Equal(seed, gameState.Seed);
            Assert.Equal(0, gameState.TurnCount);
            Assert.NotNull(gameState.Flags);
            Assert.Empty(gameState.Flags);
            Assert.True(gameState.CreatedAt <= DateTime.Now);
            Assert.True(gameState.LastPlayedAt <= DateTime.Now);
        }

        [Fact]
        public void UpdateLastPlayed_UpdatesTimestamp()
        {
            // Arrange
            var gameState = new GameState(123);
            var originalTime = gameState.LastPlayedAt;
            System.Threading.Thread.Sleep(10); // Ensure time passes

            // Act
            gameState.UpdateLastPlayed();

            // Assert
            Assert.True(gameState.LastPlayedAt > originalTime);
        }

        [Fact]
        public void GameState_FlagsCanBeSetAndRetrieved()
        {
            // Arrange
            var gameState = new GameState(123);

            // Act
            gameState.Flags["boss_defeated"] = true;
            gameState.Flags["secret_found"] = false;

            // Assert
            Assert.True(gameState.Flags["boss_defeated"]);
            Assert.False(gameState.Flags["secret_found"]);
            Assert.Equal(2, gameState.Flags.Count);
        }

        [Fact]
        public void ParameterlessConstructor_CreatesValidGameState()
        {
            // Act
            var gameState = new GameState();

            // Assert
            Assert.NotNull(gameState.Flags);
            Assert.NotNull(gameState.Player);
            Assert.NotNull(gameState.CurrentLevel);
        }
    }
}