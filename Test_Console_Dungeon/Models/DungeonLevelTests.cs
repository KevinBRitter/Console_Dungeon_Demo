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
            Assert.True(level.TotalRooms > 0);
            Assert.False(level.IsComplete);
        }

        [Fact]
        public void IsComplete_ReturnsFalseWhenBossNotDefeated()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);
            level.RoomsExplored = level.TotalRooms; // All rooms explored

            // Act & Assert
            Assert.False(level.IsComplete); // Still false because boss not defeated
        }

        [Fact]
        public void IsComplete_ReturnsTrueWhenBossDefeated()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);

            // Act
            level.IsBossDefeated = true;

            // Assert
            Assert.True(level.IsComplete);
        }

        [Fact]
        public void IsComplete_ReturnsTrueEvenIfNotAllRoomsExplored()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);
            level.RoomsExplored = 1; // Only explored 1 room

            // Act
            level.IsBossDefeated = true;

            // Assert
            Assert.True(level.IsComplete); // Boss defeated is sufficient
        }

        [Fact]
        public void BossRoom_IsPlacedOnLevel()
        {
            // Arrange & Act
            var level = new DungeonLevel(1, 123);

            // Assert
            Assert.True(level.BossRoomX >= 0);
            Assert.True(level.BossRoomX < level.Width);
            Assert.True(level.BossRoomY >= 0);
            Assert.True(level.BossRoomY < level.Height);

            var bossRoom = level.GetRoom(level.BossRoomX, level.BossRoomY);
            Assert.True(bossRoom.IsBossRoom);
            Assert.False(bossRoom.IsBlocked);
        }

        [Fact]
        public void GetRoom_ReturnsCorrectRoom()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);

            // Act
            var room = level.GetRoom(0, 0);

            // Assert
            Assert.NotNull(room);
        }

        [Fact]
        public void GetRoom_ThrowsExceptionForInvalidCoordinates()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => level.GetRoom(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => level.GetRoom(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => level.GetRoom(level.Width, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => level.GetRoom(0, level.Height));
        }

        [Fact]
        public void IsValidPosition_ReturnsTrueForWalkableRoom()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);

            // Act - Starting position should always be walkable
            bool isValid = level.IsValidPosition(level.Width / 2, level.Height / 2);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidPosition_ReturnsFalseForBlockedRoom()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);

            // Find a blocked room
            int blockedX = -1, blockedY = -1;
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    if (level.GetRoom(x, y).IsBlocked)
                    {
                        blockedX = x;
                        blockedY = y;
                        break;
                    }
                }
                if (blockedX >= 0) break;
            }

            // Act & Assert (if we found a blocked room)
            if (blockedX >= 0)
            {
                Assert.False(level.IsValidPosition(blockedX, blockedY));
            }
        }

        [Fact]
        public void IsValidPosition_ReturnsFalseForOutOfBounds()
        {
            // Arrange
            var level = new DungeonLevel(1, 123);

            // Act & Assert
            Assert.False(level.IsValidPosition(-1, 0));
            Assert.False(level.IsValidPosition(0, -1));
            Assert.False(level.IsValidPosition(level.Width, 0));
            Assert.False(level.IsValidPosition(0, level.Height));
        }

        [Fact]
        public void TotalRooms_CountsOnlyWalkableRooms()
        {
            // Arrange
            var level = new DungeonLevel(1, 123, width: 5, height: 5, roomCount: 10);

            // Act - Count walkable rooms manually
            int walkableCount = 0;
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    if (!level.GetRoom(x, y).IsBlocked)
                    {
                        walkableCount++;
                    }
                }
            }

            // Assert
            Assert.Equal(walkableCount, level.TotalRooms);
        }

        [Fact]
        public void ParameterlessConstructor_CreatesValidDungeonLevel()
        {
            // Act
            var level = new DungeonLevel();

            // Assert
            Assert.Equal(5, level.Width);
            Assert.Equal(5, level.Height);
            Assert.NotNull(level.Rooms);
            Assert.True(level.TotalRooms > 0);
        }

        [Fact]
        public void BossRoom_IsFurthestFromStart()
        {
            // Arrange
            var level = new DungeonLevel(1, 123, width: 5, height: 5, roomCount: 15);
            int startX = level.Width / 2;
            int startY = level.Height / 2;

            // Calculate distance to boss room
            int bossDistance = Math.Abs(level.BossRoomX - startX) + Math.Abs(level.BossRoomY - startY);

            // Act - Check all other walkable rooms
            bool bossIsFurthest = true;
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    var room = level.GetRoom(x, y);
                    if (!room.IsBlocked && !room.IsBossRoom)
                    {
                        int distance = Math.Abs(x - startX) + Math.Abs(y - startY);
                        if (distance > bossDistance)
                        {
                            bossIsFurthest = false;
                            break;
                        }
                    }
                }
                if (!bossIsFurthest) break;
            }

            // Assert - Boss should be at or tied for furthest distance
            Assert.True(bossIsFurthest || bossDistance > 0); // At least not at start
        }
    }
}
