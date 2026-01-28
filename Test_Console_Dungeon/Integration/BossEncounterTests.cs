using Console_Dungeon;
using Console_Dungeon.Encounters;
using Console_Dungeon.Models;
using Xunit;

namespace Console_Dungeon.Tests.Integration
{
    public class BossEncounterTests
    {
        [Fact]
        public void BossRoom_HasEncounterEnabled()
        {
            // Arrange
            var gameState = new GameState(12345);

            // Act
            var bossRoom = gameState.CurrentLevel.GetRoom(
                gameState.CurrentLevel.BossRoomX,
                gameState.CurrentLevel.BossRoomY);

            // Assert
            Assert.True(bossRoom.IsBossRoom, "Boss room should be marked as IsBossRoom");
            Assert.True(bossRoom.HasEncounter, "Boss room should have HasEncounter = true");
            Assert.False(bossRoom.IsBlocked, "Boss room should not be blocked");
            Assert.False(bossRoom.EncounterTriggered, "Boss encounter should not be triggered initially");
            Assert.True(bossRoom.CanTriggerEncounter, "Boss room should be able to trigger encounter");
        }

        [Fact]
        public void MovingToBossRoom_TriggersEncounter()
        {
            // Arrange
            var gameState = new GameState(12345);
            var encounterHandler = new EncounterHandler(gameState);

            // Move player to boss room
            gameState.Player.PositionX = gameState.CurrentLevel.BossRoomX;
            gameState.Player.PositionY = gameState.CurrentLevel.BossRoomY;

            var bossRoom = gameState.CurrentLevel.GetRoom(
                gameState.CurrentLevel.BossRoomX,
                gameState.CurrentLevel.BossRoomY);

            // Assert room can trigger
            Assert.True(bossRoom.CanTriggerEncounter);

            // Note: We can't actually test TriggerEncounter here without mocking UI,
            // but we can verify the conditions are correct
        }

        [Fact]
        public void RegularRooms_HaveEncountersEnabled()
        {
            // Arrange
            var gameState = new GameState(12345);

            // Act - Find all walkable, non-boss rooms
            int roomsWithEncounters = 0;
            for (int x = 0; x < gameState.CurrentLevel.Width; x++)
            {
                for (int y = 0; y < gameState.CurrentLevel.Height; y++)
                {
                    var room = gameState.CurrentLevel.GetRoom(x, y);
                    if (!room.IsBlocked && !room.IsBossRoom && room.HasEncounter)
                    {
                        roomsWithEncounters++;
                    }
                }
            }

            // Assert - Should have multiple rooms with encounters
            Assert.True(roomsWithEncounters > 0, "Should have at least one regular room with encounters");
        }
    }
}
