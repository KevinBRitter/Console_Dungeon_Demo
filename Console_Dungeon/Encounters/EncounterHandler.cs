using Console_Dungeon.Input;
using Console_Dungeon.Managers;
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

            DebugLogger.Log($"TriggerEncounter called at ({_gameState.Player.PositionX}, {_gameState.Player.PositionY})");
            DebugLogger.Log($"Room.IsBossRoom: {currentRoom.IsBossRoom}");
            DebugLogger.Log($"Room.Encounter: {currentRoom.Encounter}");
            DebugLogger.Log($"Room.EncounterTriggered: {currentRoom.EncounterTriggered}");
            DebugLogger.Log($"Room.CanTriggerEncounter: {currentRoom.CanTriggerEncounter}");

            if (!currentRoom.CanTriggerEncounter)
            {
                ShowAlreadySearchedMessage();
                return;
            }

            // Mark triggered so repeated searches do not re-trigger unless player flees
            currentRoom.EncounterTriggered = true;

            // If this is a boss room, force combat regardless of assigned kind
            var effectiveKind = currentRoom.IsBossRoom ? Enums.EncounterKind.Combat : currentRoom.Encounter;

            string encounterText;

            switch (effectiveKind)
            {
                case Enums.EncounterKind.Treasure:
                    DebugLogger.Log("Triggering treasure encounter (pre-determined)");
                    encounterText = HandleTreasure(new Random(_gameState.Seed + _gameState.TurnCount));
                    ScreenRenderer.DrawScreen(encounterText + "\n\nPress any key to continue...");
                    InputHandler.WaitForKey();
                    break;

                case Enums.EncounterKind.Combat:
                    DebugLogger.Log("Triggering combat encounter (pre-determined)");

                    var encounters = EncounterManager.GetEncounters();
                    int currentLevel = _gameState.CurrentLevel.LevelNumber;

                    CombatEncounter selectedEncounter;
                    if (currentRoom.IsBossRoom)
                    {
                        selectedEncounter = GetBossEncounter(encounters, currentLevel, new Random(_gameState.Seed + _gameState.TurnCount));
                    }
                    else
                    {
                        var validEncounters = encounters.CombatEncounters
                            .Where(e => !e.IsBoss && e.MinLevel <= currentLevel && e.MaxLevel >= currentLevel)
                            .ToList();

                        if (validEncounters.Count == 0)
                        {
                            encounterText = "An unknown creature attacks, but flees before combat begins!";
                            ScreenRenderer.DrawScreen(encounterText + "\n\nPress any key to continue...");
                            InputHandler.WaitForKey();
                            return;
                        }

                        selectedEncounter = SelectWeightedEncounter(validEncounters, new Random(_gameState.Seed + _gameState.TurnCount));
                    }

                    // Interactive combat loop: player chooses Attack or Flee each turn
                    var encRng = new Random(_gameState.Seed + _gameState.TurnCount);
                    var encounterData = EncounterManager.GetEncounters();

                    // Build enemy instances list (mutable tuples)
                    var enemyInstances = new List<(string Id, string Name, int DamageMin, int DamageMax, int GoldMin, int GoldMax, int Health)>();
                    foreach (var group in selectedEncounter.Enemies)
                    {
                        if (!encounterData.EnemyTypes.TryGetValue(group.Type, out var template))
                            continue;

                        for (int i = 0; i < Math.Max(1, group.Count); i++)
                        {
                            int hp = Math.Max(1, template.DamageMax * 2 + 5);
                            enemyInstances.Add((group.Type, template.Name, template.DamageMin, template.DamageMax, template.GoldMin, template.GoldMax, hp));
                        }
                    }

                    var combatResult = new CombatResult();
                    combatResult.EncounterMessage = selectedEncounter.EncounterMessages?.Count > 0
                        ? selectedEncounter.EncounterMessages[encRng.Next(selectedEncounter.EncounterMessages.Count)]
                        : "Combat begins!";

                    bool fled = false;

                    while (_gameState.Player.IsAlive && enemyInstances.Any(e => e.Health > 0))
                    {
                        // Show status
                        var statusSb = new System.Text.StringBuilder();
                        statusSb.AppendLine(combatResult.EncounterMessage);
                        statusSb.AppendLine();
                        statusSb.AppendLine($"Player HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}");
                        statusSb.AppendLine();
                        statusSb.AppendLine("Enemies:");
                        for (int i = 0; i < enemyInstances.Count; i++)
                        {
                            var e = enemyInstances[i];
                            string life = e.Health > 0 ? $"{e.Health} HP" : "Dead";
                            statusSb.AppendLine($" {i + 1}. {e.Name} - {life}");
                        }
                        statusSb.AppendLine();
                        statusSb.AppendLine("Choose an action:");
                        statusSb.AppendLine("1) Attack");
                        statusSb.AppendLine("2) Flee");

                        ScreenRenderer.DrawScreen(statusSb.ToString());
                        string input = InputHandler.GetMenuChoice().Trim();

                        if (input == "2")
                        {
                            // Flee: room returns to unexplored and encounter is unset so it can be retriggered.
                            fled = true;
                            currentRoom.EncounterTriggered = false;
                            if (currentRoom.Visited)
                            {
                                currentRoom.Visited = false;
                                if (_gameState.CurrentLevel.RoomsExplored > 0)
                                    _gameState.CurrentLevel.RoomsExplored--;
                            }

                            ScreenRenderer.DrawScreen("You flee from the encounter!\n\nPress any key to continue...");
                            InputHandler.WaitForKey();
                            break;
                        }

                        // Default: Attack
                        var targetIdx = enemyInstances.FindIndex(e => e.Health > 0);
                        if (targetIdx < 0)
                            break;

                        var target = enemyInstances[targetIdx];

                        // Player damage roll (use effective attack including weapon)
                        int effectiveAttack = _gameState.Player.GetEffectiveAttack();
                        int minPlayerDamage = Math.Max(1, effectiveAttack / 2);
                        int maxPlayerDamage = Math.Max(minPlayerDamage, effectiveAttack);
                        int playerDamage = encRng.Next(minPlayerDamage, maxPlayerDamage + 1);

                        // Apply to target
                        target.Health = Math.Max(0, target.Health - playerDamage);
                        enemyInstances[targetIdx] = target;

                        var turnSb = new System.Text.StringBuilder();
                        turnSb.AppendLine($"You attack the {target.Name} for {playerDamage} damage.");
                        combatResult.DamageBreakdown.Add($"Player -> {target.Name}: {playerDamage} damage");

                        if (target.Health == 0)
                        {
                            combatResult.TotalKills++;
                            int goldFound = encRng.Next(target.GoldMin, target.GoldMax + 1);
                            combatResult.TotalGold += goldFound;
                            turnSb.AppendLine($"You slay the {target.Name} and find {goldFound} gold.");
                        }

                        // Enemies retaliate (each alive enemy)
                        foreach (var enemy in enemyInstances.Where(e => e.Health > 0).ToList())
                        {
                            int rawEnemyDamage = encRng.Next(Math.Max(1, enemy.DamageMin), Math.Max(enemy.DamageMin, enemy.DamageMax) + 1);

                            // Compute actual damage after defense (includes equipped armor)
                            int actualDamage = Math.Max(1, rawEnemyDamage - _gameState.Player.GetEffectiveDefense());

                            // Apply damage using Player.TakeDamage (now considers equipped armor)
                            _gameState.Player.TakeDamage(rawEnemyDamage);

                            combatResult.TotalDamage += actualDamage;
                            combatResult.DamageBreakdown.Add($"{enemy.Name}: {actualDamage} damage");

                            turnSb.AppendLine($"{enemy.Name} hits you for {actualDamage} damage.");

                            if (!_gameState.Player.IsAlive)
                            {
                                turnSb.AppendLine("You have been defeated...");
                                break;
                            }
                        }

                        // Count this as a player turn
                        _gameState.TurnCount++;

                        // Show turn results
                        ScreenRenderer.DrawScreen(turnSb.ToString() + "\n\nPress any key to continue...");
                        InputHandler.WaitForKey();
                    }

                    if (fled)
                    {
                        // Player fled: do not award XP or loot, leave room untriggered/unvisited for re-entry
                        return;
                    }

                    if (_gameState.Player.IsAlive)
                    {
                        // Victory: finalize messages and award gold/kills
                        if (selectedEncounter.VictoryMessages?.Count > 0)
                        {
                            var victoryTemplate = selectedEncounter.VictoryMessages[encRng.Next(selectedEncounter.VictoryMessages.Count)];
                            combatResult.VictoryMessage = EncounterManager.FormatMessage(victoryTemplate, ("totalDamage", combatResult.TotalDamage));
                        }
                        else
                        {
                            combatResult.VictoryMessage = $"You survived the encounter taking {combatResult.TotalDamage} damage.";
                        }

                        if (selectedEncounter.LootMessages?.Count > 0)
                        {
                            var lootTemplate = selectedEncounter.LootMessages[encRng.Next(selectedEncounter.LootMessages.Count)];
                            combatResult.LootMessage = EncounterManager.FormatMessage(lootTemplate, ("totalGold", combatResult.TotalGold));
                        }
                        else
                        {
                            combatResult.LootMessage = $"You gather {combatResult.TotalGold} gold.";
                        }

                        // Apply gold and kills to player (base enemy loot)
                        _gameState.Player.Gold += combatResult.TotalGold;
                        _gameState.Player.Kills += combatResult.TotalKills;

                        // Award experience based on kills and level
                        int xpPerKill = 10 + (_gameState.CurrentLevel.LevelNumber * 5);
                        int totalXP = combatResult.TotalKills * xpPerKill;
                        _gameState.Player.GainExperience(totalXP);

                        // If boss defeated, mark level complete
                        if (currentRoom.IsBossRoom)
                        {
                            _gameState.CurrentLevel.IsBossDefeated = true;
                        }

                        // Generate additional loot/items using LootManager.
                        // Difficulty factor can be scaled by level or totalDamage; keep simple for now.
                        int difficultyFactor = Math.Max(1, _gameState.CurrentLevel.LevelNumber);
                        var loot = LootManager.GenerateCombatLoot(encRng, difficultyFactor: difficultyFactor, ensureNonEmpty: true);

                        // Apply generated loot
                        if (loot.gold > 0)
                        {
                            _gameState.Player.Gold += loot.gold;
                        }

                        var lootMessagesSb = new System.Text.StringBuilder();
                        foreach (var msg in loot.messages)
                        {
                            lootMessagesSb.AppendLine(msg);
                        }

                        foreach (var item in loot.items)
                        {
                            bool accepted = _gameState.Player.AddItem(item);
                            if (!accepted)
                            {
                                lootMessagesSb.AppendLine($"You couldn't carry the {item.Name} and left it behind.");
                            }
                            else
                            {
                                lootMessagesSb.AppendLine($"Added {item.Name} to your possessions.");
                            }
                        }

                        string xpMessage = MessageManager.GetMessage("experience.gained", ("xp", totalXP));
                        encounterText = combatResult.GetFullMessage() + $"\n\n{xpMessage}\n\n" + lootMessagesSb.ToString().TrimEnd();

                        ScreenRenderer.DrawScreen(encounterText + "\n\nPress any key to continue...");
                        InputHandler.WaitForKey();
                    }
                    else
                    {
                        // Defeat
                        encounterText = "You were defeated in combat...";
                        ScreenRenderer.DrawScreen(encounterText + "\n\nPress any key to continue...");
                        InputHandler.WaitForKey();
                    }

                    break;

                case Enums.EncounterKind.None:
                default:
                    DebugLogger.Log("Triggering empty room encounter (pre-determined)");
                    encounterText = HandleEmptyRoom(new Random(_gameState.Seed + _gameState.TurnCount));
                    ScreenRenderer.DrawScreen(encounterText + "\n\nPress any key to continue...");
                    InputHandler.WaitForKey();
                    break;
            }
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
            // Use LootManager to generate treasure
            var treasure = LootManager.GenerateTreasure(rng, ensureNonEmpty: true);

            // Apply treasure
            if (treasure.gold > 0)
            {
                _gameState.Player.Gold += treasure.gold;
            }

            var sb = new System.Text.StringBuilder();
            string template = encounters.TreasureEncounters[rng.Next(encounters.TreasureEncounters.Count)];
            sb.AppendLine(EncounterManager.FormatMessage(template, ("gold", treasure.gold)));

            foreach (var item in treasure.items)
            {
                bool accepted = _gameState.Player.AddItem(item);
                if (!accepted)
                {
                    sb.AppendLine($"You cannot carry the {item.Name}; it remains on the ground.");
                }
                else
                {
                    sb.AppendLine($"You pocket: {item.Name} - {item.Description}");
                }
            }

            return sb.ToString().TrimEnd();
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

        private string HandleEmptyRoom(Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            var loot = LootManager.GenerateEmptyRoomLoot(rng);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(encounters.EmptyRooms[rng.Next(encounters.EmptyRooms.Count)]);
            foreach (var msg in loot.messages)
            {
                sb.AppendLine(msg);
            }

            // Apply small finds
            if (loot.gold > 0) _gameState.Player.Gold += loot.gold;
            foreach (var item in loot.items)
            {
                bool accepted = _gameState.Player.AddItem(item);
                if (accepted) sb.AppendLine($"You pick up {item.Name}.");
                else sb.AppendLine($"You found {item.Name} but could not carry it.");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
