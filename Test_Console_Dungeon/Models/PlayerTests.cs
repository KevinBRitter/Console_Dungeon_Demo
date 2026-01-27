using Console_Dungeon.Models;

namespace Console_Dungeon.Tests.Models
{
    public class PlayerTests
    {
        [Fact]
        public void NewPlayer_InitializesWithCorrectDefaults()
        {
            // Arrange & Act
            var player = new Player("TestHero");

            // Assert
            Assert.Equal("TestHero", player.Name);
            Assert.Equal(1, player.Level);
            Assert.Equal(100, player.MaxHealth);
            Assert.Equal(100, player.Health);
            Assert.Equal(10, player.Attack);
            Assert.Equal(5, player.Defense);
            Assert.Equal(0, player.Experience);
            Assert.Equal(0, player.Gold);
            Assert.Equal(0, player.PositionX);
            Assert.Equal(0, player.PositionY);
            Assert.True(player.IsAlive);
        }

        [Theory]
        [InlineData(20, 5, 15)] // 20 damage, 5 defense = 15 actual damage
        [InlineData(10, 5, 5)]  // 10 damage, 5 defense = 5 actual damage
        [InlineData(3, 5, 1)]   // 3 damage, 5 defense = 1 minimum damage
        public void TakeDamage_ReducesHealthByDamageMinusDefense(int damage, int defense, int expectedDamage)
        {
            // Arrange
            var player = new Player("Hero");
            player.Defense = defense;
            int initialHealth = player.Health;

            // Act
            player.TakeDamage(damage);

            // Assert
            Assert.Equal(initialHealth - expectedDamage, player.Health);
        }

        [Fact]
        public void TakeDamage_NeverReducesHealthBelowZero()
        {
            // Arrange
            var player = new Player("Hero");
            player.Health = 10;

            // Act
            player.TakeDamage(200);

            // Assert
            Assert.Equal(0, player.Health);
            Assert.False(player.IsAlive);
        }

        [Fact]
        public void TakeDamage_AlwaysDealsAtLeastOneDamage()
        {
            // Arrange
            var player = new Player("Hero");
            player.Defense = 100; // Very high defense
            int initialHealth = player.Health;

            // Act
            player.TakeDamage(5); // Low damage

            // Assert
            Assert.Equal(initialHealth - 1, player.Health);
        }

        [Fact]
        public void Heal_RestoresHealth()
        {
            // Arrange
            var player = new Player("Hero");
            player.Health = 50;

            // Act
            player.Heal(30);

            // Assert
            Assert.Equal(80, player.Health);
        }

        [Fact]
        public void Heal_CannotExceedMaxHealth()
        {
            // Arrange
            var player = new Player("Hero");
            player.Health = 90;

            // Act
            player.Heal(50);

            // Assert
            Assert.Equal(player.MaxHealth, player.Health);
            Assert.Equal(100, player.Health);
        }

        [Fact]
        public void IsAlive_ReturnsTrueWhenHealthAboveZero()
        {
            // Arrange
            var player = new Player("Hero");
            player.Health = 1;

            // Assert
            Assert.True(player.IsAlive);
        }

        [Fact]
        public void IsAlive_ReturnsFalseWhenHealthIsZero()
        {
            // Arrange
            var player = new Player("Hero");
            player.Health = 0;

            // Assert
            Assert.False(player.IsAlive);
        }

        [Fact]
        public void ParameterlessConstructor_CreatesPlayerWithDefaultName()
        {
            // Act
            var player = new Player();

            // Assert
            Assert.Equal("Adventurer", player.Name);
        }
    }
}
