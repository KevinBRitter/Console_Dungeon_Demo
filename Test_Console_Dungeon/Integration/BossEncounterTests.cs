using Console_Dungeon;
using Console_Dungeon.Encounters;
using Console_Dungeon.Enums;
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
        public void BossRoom_EncounterKind_IsCombat()
        {
            // Regression test: PlaceBossRoom must set Encounter = Combat.
            // Before the fix, Encounter was left as None if the cell was initially
            // generated without an encounter, so the encounter never fired.

            // Arrange
            var level = new DungeonLevel(1, 99999);

            // Act
            var bossRoom = level.GetRoom(level.BossRoomX, level.BossRoomY);

            // Assert
            Assert.True(bossRoom.Encounter == EncounterKind.Combat,
                "Boss room Encounter must be Combat so the encounter handler fires");
        }

        [Fact]
        public void BossRoom_CanTriggerEncounter_EvenWhenEncounterKindIsNone()
        {
            // Regression test: CanTriggerEncounter must use IsBossRoom as a fallback
            // guard so the encounter handler's IsBossRoom override can never be silently
            // bypassed by a stale Encounter == None value.

            // Arrange - manually construct a worst-case boss room
            var room = new Room("A dark throne room.", hasEncounter: false);
            room.IsBossRoom = true;
            // Encounter is None because hasEncounter was false

            // Assert
            Assert.Equal(EncounterKind.None, room.Encounter);
            Assert.True(room.CanTriggerEncounter,
                "IsBossRoom should make CanTriggerEncounter true even when Encounter == None");
        }

        [Fact]
        public void BossRoom_CannotTriggerEncounter_AfterEncounterFires()
        {
            // Regression test: once the boss encounter has been triggered,
            // CanTriggerEncounter must return false regardless of IsBossRoom.

            // Arrange
            var room = new Room("A dark throne room.", hasEncounter: true);
            room.IsBossRoom = true;
            room.EncounterTriggered = true;

            // Assert
            Assert.False(room.CanTriggerEncounter,
                "Boss room should not trigger a second encounter once EncounterTriggered is set");
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
