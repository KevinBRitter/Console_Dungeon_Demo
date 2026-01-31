using System;
using Console_Dungeon.Managers;
using Xunit;

namespace Console_Dungeon.Tests.Managers
{
    public class LootManagerTests
    {
        [Fact]
        public void GenerateTreasure_ReturnsSomeResults()
        {
            var rng = new Random(12345);
            var result = LootManager.GenerateTreasure(rng, ensureNonEmpty: true);

            Assert.NotNull(result.items);
            Assert.NotNull(result.messages);
            Assert.True(result.gold >= 0);
        }

        [Fact]
        public void GenerateCombatLoot_EnsureNonEmptyGuaranteesGoldOrItems()
        {
            var rng = new Random(54321);
            var result = LootManager.GenerateCombatLoot(rng, difficultyFactor: 1, ensureNonEmpty: true);

            Assert.NotNull(result.items);
            Assert.NotNull(result.messages);
            Assert.True(result.gold > 0 || result.items.Count > 0);
        }
    }
}
