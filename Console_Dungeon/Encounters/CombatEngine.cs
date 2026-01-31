using Console_Dungeon.Managers;
using Console_Dungeon.Models;

namespace Console_Dungeon.Encounters
{
    /// <summary>
    /// Simple turn-based combat engine.
    /// Runs a combat loop between the player's current state (mutated) and the selected CombatEncounter.
    /// Returns a CombatResult describing damage taken, gold gained, and kills.
    /// </summary>
    public static class CombatEngine
    {
        private class EnemyInstance
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int DamageMin { get; set; }
            public int DamageMax { get; set; }
            public int GoldMin { get; set; }
            public int GoldMax { get; set; }
            public int Health { get; set; }
            public bool IsAlive => Health > 0;
        }

        // Runs a combat loop. This mutates gameState.Player (health, gold, kills).
        // rng should be seeded by caller for deterministic runs.
        public static CombatResult RunCombat(GameState gameState, CombatEncounter encounter, Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            var result = new CombatResult();

            // Prepare enemies list (expand groups into individual instances)
            var enemies = new List<EnemyInstance>();
            foreach (var group in encounter.Enemies)
            {
                if (!encounters.EnemyTypes.TryGetValue(group.Type, out var template))
                    continue;

                for (int i = 0; i < Math.Max(1, group.Count); i++)
                {
                    var inst = new EnemyInstance
                    {
                        Id = group.Type,
                        Name = template.Name,
                        DamageMin = template.DamageMin,
                        DamageMax = template.DamageMax,
                        GoldMin = template.GoldMin,
                        GoldMax = template.GoldMax,
                        // Simple health formula: (damageMax * 2) + 5 to give some HP to sustain turns.
                        Health = Math.Max(1, template.DamageMax * 2 + 5)
                    };
                    enemies.Add(inst);
                }
            }

            // Select and store the encounter message up front.
            if (encounter.EncounterMessages?.Count > 0)
            {
                result.EncounterMessage = encounter.EncounterMessages[rng.Next(encounter.EncounterMessages.Count)];
            }
            else
            {
                result.EncounterMessage = "Combat begins!";
            }

            var player = gameState.Player;

            // Combat loop
            while (player.IsAlive && enemies.Exists(e => e.IsAlive))
            {
                // Player turn: simple attack against the first alive enemy
                var target = enemies.Find(e => e.IsAlive);
                if (target != null)
                {
                    // Use effective attack which includes weapon bonuses
                    int effectiveAttack = player.GetEffectiveAttack();
                    int minPlayerDamage = Math.Max(1, effectiveAttack / 2);
                    int maxPlayerDamage = Math.Max(minPlayerDamage, effectiveAttack);
                    int playerDamage = rng.Next(minPlayerDamage, maxPlayerDamage + 1);

                    target.Health = Math.Max(0, target.Health - playerDamage);

                    result.DamageBreakdown.Add($"Player -> {target.Name}: {playerDamage} damage");

                    // If target died, award gold and count kill
                    if (!target.IsAlive)
                    {
                        result.TotalKills++;
                        int goldFound = rng.Next(target.GoldMin, target.GoldMax + 1);
                        result.TotalGold += goldFound;
                    }
                }

                // Enemies turn: each alive enemy attacks the player
                foreach (var enemy in enemies)
                {
                    if (!enemy.IsAlive)
                    {
                        continue;
                    }

                    int rawEnemyDamage = rng.Next(Math.Max(1, enemy.DamageMin), Math.Max(enemy.DamageMin, enemy.DamageMax) + 1);

                    // Compute actual damage after player's effective defense (armor included)
                    int actualDamage = Math.Max(1, rawEnemyDamage - player.GetEffectiveDefense());

                    // Apply damage to player using Player.TakeDamage (which now uses effective defense)
                    player.TakeDamage(rawEnemyDamage);

                    // Record actual damage (not the raw roll) for messaging/statistics
                    result.TotalDamage += actualDamage;
                    result.DamageBreakdown.Add($"{enemy.Name}: {actualDamage} damage");

                    if (!player.IsAlive)
                    {
                        break;
                    }
                }

                // loop continues until one side is dead
            }

            // Select victory/defeat message and loot text
            if (player.IsAlive)
            {
                // Victory
                if (encounter.VictoryMessages?.Count > 0)
                {
                    var victoryTemplate = encounter.VictoryMessages[rng.Next(encounter.VictoryMessages.Count)];
                    result.VictoryMessage = EncounterManager.FormatMessage(victoryTemplate, ("totalDamage", result.TotalDamage));
                }
                else
                {
                    result.VictoryMessage = $"You survived the encounter taking {result.TotalDamage} damage.";
                }

                if (encounter.LootMessages?.Count > 0)
                {
                    var lootTemplate = encounter.LootMessages[rng.Next(encounter.LootMessages.Count)];
                    result.LootMessage = EncounterManager.FormatMessage(lootTemplate, ("totalGold", result.TotalGold));
                }
                else
                {
                    result.LootMessage = $"You gather {result.TotalGold} gold.";
                }

                // Apply accumulated gold and kills to player
                player.Gold += result.TotalGold;
                player.Kills += result.TotalKills;
            }
            else
            {
                // Defeat path
                result.VictoryMessage = "You are defeated...";
                result.LootMessage = $"You lose {result.TotalGold} gold."; // still show what was gained if any before death
            }

            return result;
        }
    }
}
