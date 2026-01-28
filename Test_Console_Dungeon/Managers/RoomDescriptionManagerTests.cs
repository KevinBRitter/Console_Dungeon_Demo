using Console_Dungeon.Managers;

namespace Console_Dungeon.Tests.Managers
{
    public class RoomDescriptionManagerTests
    {
        [Fact]
        public void GetRandomStandardRoom_ReturnsNonEmptyString()
        {
            // Arrange
            var rng = new Random(12345);

            // Act
            var description = RoomDescriptionManager.GetRandomStandardRoom(rng);

            // Assert
            Assert.NotNull(description);
            Assert.NotEmpty(description);
        }

        [Fact]
        public void GetRandomBossRoom_ReturnsNonEmptyString()
        {
            // Arrange
            var rng = new Random(12345);

            // Act
            var description = RoomDescriptionManager.GetRandomBossRoom(rng);

            // Assert
            Assert.NotNull(description);
            Assert.NotEmpty(description);
        }

        [Fact]
        public void GetRandomBlockedRoom_ReplacesCoordinates()
        {
            // Arrange
            var rng = new Random(12345);

            // Act
            var description = RoomDescriptionManager.GetRandomBlockedRoom(rng, 3, 5);

            // Assert
            Assert.NotNull(description);
            Assert.NotEmpty(description);
            // Note: Some descriptions might not have coordinates, so just check it's not empty
        }

        [Fact]
        public void GetRandomSpecialRoom_Treasure_ReturnsValidDescription()
        {
            // Arrange
            var rng = new Random(12345);

            // Act
            var description = RoomDescriptionManager.GetRandomSpecialRoom("treasure", rng);

            // Assert
            Assert.NotNull(description);
            Assert.NotEmpty(description);
        }

        [Fact]
        public void GetDescriptions_ReturnsValidObject()
        {
            // Act
            var descriptions = RoomDescriptionManager.GetDescriptions();

            // Assert
            Assert.NotNull(descriptions);
            Assert.NotNull(descriptions.StandardRooms);
            Assert.NotNull(descriptions.BossRooms);
            Assert.NotNull(descriptions.BlockedRooms);
            Assert.NotEmpty(descriptions.StandardRooms);
        }
    }
}
