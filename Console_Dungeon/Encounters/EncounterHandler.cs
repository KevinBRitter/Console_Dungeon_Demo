using Console_Dungeon.Input;
using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using Console_Dungeon.UI;
using System.Diagnostics;

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

            DebugLogger.Log($"TriggerEncounter called at ({_gameState.Player.PositionX}, {_gameState.Player.PositionY})");
            DebugLogger.Log($"Room.IsBossRoom: {currentRoom.IsBossRoom}");
            DebugLogger.Log($"Room.HasEncounter: {currentRoom.HasEncounter}");
            DebugLogger.Log($"Room.EncounterTriggered: {currentRoom.EncounterTriggered}");
            DebugLogger.Log($"Room.CanTriggerEncounter: {currentRoom.CanTriggerEncounter}");


            if (!currentRoom.CanTriggerEncounter)
            {
                ShowAlreadySearchedMessage();
                return;
            }

            currentRoom.EncounterTriggered = true;

            Random rng = new Random(_gameState.Seed + _gameState.TurnCount);
            int encounterType = rng.Next(1, 11);

            DebugLogger.Log($"Encounter type roll: {encounterType} (1-3=treasure, 4-8=combat, 9-10=empty)");

            string encounterText;

            if (encounterType <= 3)
            {
                DebugLogger.Log("Triggering treasure encounter");
                encounterText = HandleTreasure(rng);
            }
            else if (encounterType <= 8)
            {
                DebugLogger.Log("Triggering combat encounter");
                encounterText = HandleCombat(rng);
            }
            else
            {
                DebugLogger.Log("Triggering empty room encounter");
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

            DebugLogger.Log($"HandleCombat - Current Level: {currentLevel}");
            DebugLogger.Log($"HandleCombat - Total Combat Encounters: {encounters.CombatEncounters.Count}");

            var currentRoom = _gameState.CurrentLevel.GetRoom(
                _gameState.Player.PositionX,
                _gameState.Player.PositionY);

            DebugLogger.Log($"HandleCombat - Is Boss Room: {currentRoom.IsBossRoom}");

            CombatEncounter selectedEncounter;

            if (currentRoom.IsBossRoom)
            {
                selectedEncounter = GetBossEncounter(encounters, currentLevel, rng);
                DebugLogger.Log($"HandleCombat - Selected Regular Encounter: {selectedEncounter.Id}");
            }
            else
            {
                var validEncounters = encounters.CombatEncounters
                    .Where(e => !e.IsBoss && e.MinLevel <= currentLevel && e.MaxLevel >= currentLevel)
                    .ToList();

                Debug.WriteLine($"[DEBUG] Valid Regular Encounters: {validEncounters.Count}");

                if (validEncounters.Count == 0)
                {
                    Debug.WriteLine("[DEBUG] No valid encounters found!");
                    return "An unknown creature attacks, but flees before combat begins!";
                }

                selectedEncounter = SelectWeightedEncounter(validEncounters, rng);
                Debug.WriteLine($"[DEBUG] Selected Regular Encounter: {selectedEncounter.Id}");
            }

            // Resolve combat
            var result = ResolveCombat(selectedEncounter, rng);

            // Apply effects to player
            _gameState.Player.TakeDamage(result.TotalDamage);
            _gameState.Player.Gold += result.TotalGold;
            _gameState.Player.Kills += result.TotalKills;

            // Award experience based on kills and level
            int xpPerKill = 10 + (_gameState.CurrentLevel.LevelNumber * 5); // Scales with dungeon level
            int totalXP = result.TotalKills * xpPerKill;
            _gameState.Player.GainExperience(totalXP);

            // Add XP gain message
            string xpMessage = MessageManager.GetMessage("experience.gained", ("xp", totalXP));

            // If boss defeated, mark level complete
            if (currentRoom.IsBossRoom)
            {
                _gameState.CurrentLevel.IsBossDefeated = true;
            }

            return result.GetFullMessage() + $"\n\n{xpMessage}";
        }
        private CombatEncounter GetBossEncounter(EncounterData encounters, int currentLevel, Random rng)
        {
            // Get all boss encounters valid for this level
            var validBosses = encounters.CombatEncounters
                .Where(e => e.IsBoss && e.MinLevel <= currentLevel && e.MaxLevel >= currentLevel)
                .ToList();

            if (validBosses.Count == 0)
            {
                // Fallback: create a generic boss
                return new CombatEncounter
                {
                    Id = "generic_boss",
                    IsBoss = true,
                    Enemies = new List<EnemyGroup>
            {
                new EnemyGroup { Type = "skeleton", Count = 5 }
            },
                    EncounterMessages = new List<string> { "A powerful enemy appears!" },
                    VictoryMessages = new List<string> { "You defeat the boss! You took {totalDamage} damage!" },
                    LootMessages = new List<string> { "You claim {totalGold} gold!" }
                };
            }

            // Select random boss from valid options
            return validBosses[rng.Next(validBosses.Count)];
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
