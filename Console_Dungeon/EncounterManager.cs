using Console_Dungeon.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Console_Dungeon
{
    public class EncounterManager
    {
        private static EncounterData? _encounterData;

        // Load encounter data from JSON file
        public static void LoadEncounters(string filePath = "Data/Encounters.json")
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                _encounterData = JsonSerializer.Deserialize<EncounterData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (_encounterData == null)
                {
                    throw new Exception("Failed to deserialize encounter data.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading encounters: {ex.Message}");
                // Fall back to default data
                _encounterData = CreateDefaultEncounters();
            }
        }

        // Get encounter data (ensure it's loaded first)
        public static EncounterData GetEncounters()
        {
            if (_encounterData == null)
            {
                LoadEncounters();
            }
            return _encounterData ?? CreateDefaultEncounters();
        }

        // Fallback default data if JSON fails to load
        private static EncounterData CreateDefaultEncounters()
        {
            return new EncounterData
            {
                TreasureEncounters = new List<string> { "You find {gold} gold coins!" },
                EmptyRooms = new List<string> { "The room is empty." },
                AlreadySearched = new List<string> { "You've already searched this room." },
                NeverHadEncounter = new List<string> { "This room has nothing of value." },
                EnemyTypes = new Dictionary<string, EnemyType>
                {
                    ["goblin"] = new EnemyType
                    {
                        Name = "Goblin",
                        DamageMin = 8,
                        DamageMax = 16,
                        GoldMin = 3,
                        GoldMax = 8
                    }
                },
                CombatEncounters = new List<CombatEncounter>
                {
                    new CombatEncounter
                    {
                        Id = "single_goblin",
                        Weight = 10,
                        MinLevel = 1,
                        MaxLevel = 99,
                        Enemies = new List<EnemyGroup>
                        {
                            new EnemyGroup { Type = "goblin", Count = 1 }
                        },
                        EncounterMessages = new List<string> { "A goblin attacks!" },
                        VictoryMessages = new List<string> { "You defeat it but take {totalDamage} damage." },
                        LootMessages = new List<string> { "You find {totalGold} gold." }
                    }
                }
            };
        }

        // Helper method to format messages with placeholders
        public static string FormatMessage(string template, params (string key, object value)[] replacements)
        {
            string result = template;
            foreach (var (key, value) in replacements)
            {
                result = result.Replace($"{{{key}}}", value.ToString());
            }
            return result;
        }
    }
}
