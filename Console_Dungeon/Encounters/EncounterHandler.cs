using Console_Dungeon.Input;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon.Encounters
{
    public class EncounterHandler
    {
        private readonly GameState _gameState;

        public EncounterHandler(GameState gameState)
        {
            _gameState = gameState;
        }

        public void TriggerEncounter()
        {
            var currentRoom = _gameState.CurrentLevel.GetRoom(
                _gameState.Player.PositionX,
                _gameState.Player.PositionY);

            if (!currentRoom.CanTriggerEncounter)
            {
                ShowAlreadySearchedMessage();
                return;
            }

            currentRoom.EncounterTriggered = true;

            Random rng = new Random(_gameState.Seed + _gameState.TurnCount);
            int encounterType = rng.Next(1, 11);

            string encounterText;

            if (encounterType <= 3)
            {
                encounterText = HandleTreasure(rng);
            }
            else if (encounterType <= 8)
            {
                encounterText = HandleCombat(rng);
            }
            else
            {
                encounterText = HandleEmptyRoom(rng);
            }

            ScreenRenderer.DrawScreen(encounterText + "\n\nPress any key to continue...");
            InputHandler.WaitForKey();
        }

        private void ShowAlreadySearchedMessage()
        {
            var currentRoom = _gameState.CurrentLevel.GetRoom(
                _gameState.Player.PositionX,
                _gameState.Player.PositionY);

            Random rng = new Random(_gameState.Seed + _gameState.TurnCount);
            var encounters = EncounterManager.GetEncounters();

            string message;
            if (currentRoom.EncounterTriggered)
            {
                message = encounters.AlreadySearched[rng.Next(encounters.AlreadySearched.Count)];
            }
            else
            {
                message = encounters.NeverHadEncounter[rng.Next(encounters.NeverHadEncounter.Count)];
            }

            ScreenRenderer.DrawScreen(message + "\n\nPress any key to continue...");
            InputHandler.WaitForKey();
        }

        private string HandleTreasure(Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            int goldAmount = rng.Next(5, 21);
            _gameState.Player.Gold += goldAmount;

            string template = encounters.TreasureEncounters[rng.Next(encounters.TreasureEncounters.Count)];
            return EncounterManager.FormatMessage(template, ("gold", goldAmount));
        }

        private string HandleCombat(Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            int currentLevel = _gameState.CurrentLevel.LevelNumber;

            // Get valid encounters for current level
            var validEncounters = encounters.CombatEncounters
                .Where(e => e.MinLevel <= currentLevel && e.MaxLevel >= currentLevel)
                .ToList();

            if (validEncounters.Count == 0)
            {
                return "An unknown creature attacks, but flees before combat begins!";
            }

            // Weighted random selection
            var selectedEncounter = SelectWeightedEncounter(validEncounters, rng);

            // Resolve combat
            var result = ResolveCombat(selectedEncounter, rng);

            // Apply effects to player
            _gameState.Player.TakeDamage(result.TotalDamage);
            _gameState.Player.Gold += result.TotalGold;
            _gameState.Player.Kills += result.TotalKills;

            return result.GetFullMessage();
        }

        private CombatEncounter SelectWeightedEncounter(List<CombatEncounter> encounters, Random rng)
        {
            int totalWeight = encounters.Sum(e => e.Weight);
            int roll = rng.Next(totalWeight);
            int cumulative = 0;

            foreach (var encounter in encounters)
            {
                cumulative += encounter.Weight;
                if (roll < cumulative)
                {
                    return encounter;
                }
            }

            return encounters[0]; // Fallback
        }

        private CombatResult ResolveCombat(CombatEncounter encounter, Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            var result = new CombatResult();

            // Calculate damage and gold from each enemy
            foreach (var enemyGroup in encounter.Enemies)
            {
                if (!encounters.EnemyTypes.ContainsKey(enemyGroup.Type))
                {
                    continue;
                }

                var enemyType = encounters.EnemyTypes[enemyGroup.Type];

                for (int i = 0; i < enemyGroup.Count; i++)
                {
                    int damage = rng.Next(enemyType.DamageMin, enemyType.DamageMax);
                    int gold = rng.Next(enemyType.GoldMin, enemyType.GoldMax);

                    result.TotalDamage += damage;
                    result.TotalGold += gold;
                    result.TotalKills++;

                    result.DamageBreakdown.Add($"{enemyType.Name}: {damage} damage");
                }
            }

            // Select random messages
            result.EncounterMessage = encounter.EncounterMessages[rng.Next(encounter.EncounterMessages.Count)];

            string victoryTemplate = encounter.VictoryMessages[rng.Next(encounter.VictoryMessages.Count)];
            result.VictoryMessage = EncounterManager.FormatMessage(victoryTemplate,
                ("totalDamage", result.TotalDamage));

            string lootTemplate = encounter.LootMessages[rng.Next(encounter.LootMessages.Count)];
            result.LootMessage = EncounterManager.FormatMessage(lootTemplate,
                ("totalGold", result.TotalGold));

            return result;
        }

        private string HandleEmptyRoom(Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            return encounters.EmptyRooms[rng.Next(encounters.EmptyRooms.Count)];
        }
    }
}
