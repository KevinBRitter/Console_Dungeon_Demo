using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Console_Dungeon.Tests.Managers
{
    public class EncounterManagerTests
    {
        private const string TestDataFolder = "TestData";
        private const string TestEncountersFile = "TestData/TestEncounters.json";

        public EncounterManagerTests()
        {
            // Setup: Create test data directory and file
            if (!Directory.Exists(TestDataFolder))
            {
                Directory.CreateDirectory(TestDataFolder);
            }

            CreateTestEncountersFile();
        }

        private void CreateTestEncountersFile()
        {
            var testData = new EncounterData
            {
                TreasureEncounters = new List<string>
                {
                    "You find {gold} gold!",
                    "A chest contains {gold} gold!"
                },
                EmptyRooms = new List<string>
                {
                    "The room is empty.",
                    "Nothing here."
                },
                AlreadySearched = new List<string>
                {
                    "You've already searched this room."
                },
                NeverHadEncounter = new List<string>
                {
                    "This room has nothing of value."
                },
                EnemyTypes = new Dictionary<string, EnemyType>
                {
                    ["testGoblin"] = new EnemyType
                    {
                        Name = "Test Goblin",
                        DamageMin = 5,
                        DamageMax = 10,
                        GoldMin = 1,
                        GoldMax = 5
                    }
                },
                CombatEncounters = new List<CombatEncounter>
                {
                    new CombatEncounter
                    {
                        Id = "test_single_goblin",
                        Weight = 10,
                        MinLevel = 1,
                        MaxLevel = 5,
                        Enemies = new List<EnemyGroup>
                        {
                            new EnemyGroup { Type = "testGoblin", Count = 1 }
                        },
                        EncounterMessages = new List<string> { "A test goblin attacks!" },
                        VictoryMessages = new List<string> { "You defeat it but take {totalDamage} damage." },
                        LootMessages = new List<string> { "You loot {totalGold} gold." }
                    }
                }
            };

            string json = JsonSerializer.Serialize(testData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(TestEncountersFile, json);
        }

        [Fact]
        public void LoadEncounters_LoadsDataFromFile()
        {
            // Act
            EncounterManager.LoadEncounters(TestEncountersFile);
            var encounters = EncounterManager.GetEncounters();

            // Assert
            Assert.NotNull(encounters);
            Assert.Equal(2, encounters.TreasureEncounters.Count);
            Assert.Equal(2, encounters.EmptyRooms.Count);
            Assert.Single(encounters.AlreadySearched);
            Assert.Single(encounters.NeverHadEncounter);
            Assert.Single(encounters.EnemyTypes);
            Assert.Single(encounters.CombatEncounters);
            Assert.True(encounters.EnemyTypes.ContainsKey("testGoblin"));
        }

        [Fact]
        public void LoadEncounters_InvalidFilePath_UsesDefaultData()
        {
            // Act
            EncounterManager.LoadEncounters("NonExistentFile.json");
            var encounters = EncounterManager.GetEncounters();

            // Assert
            Assert.NotNull(encounters);
            Assert.NotEmpty(encounters.TreasureEncounters);
            Assert.NotEmpty(encounters.EmptyRooms);
            Assert.NotEmpty(encounters.EnemyTypes);
            Assert.NotEmpty(encounters.CombatEncounters);
        }

        [Fact]
        public void GetEncounters_LoadsDataIfNotAlreadyLoaded()
        {
            // Act
            var encounters = EncounterManager.GetEncounters();

            // Assert
            Assert.NotNull(encounters);
        }

        [Theory]
        [InlineData("You find {gold} gold!", "gold", 100, "You find 100 gold!")]
        [InlineData("You take {damage} damage and find {gold} gold!", "damage", 15, "You take 15 damage and find {gold} gold!")]
        public void FormatMessage_ReplacesPlaceholders(string template, string key, object value, string expected)
        {
            // Act
            var result = EncounterManager.FormatMessage(template, (key, value));

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatMessage_HandlesMultiplePlaceholders()
        {
            // Arrange
            string template = "You take {damage} damage and find {gold} gold!";

            // Act
            var result = EncounterManager.FormatMessage(template,
                ("damage", 15),
                ("gold", 50));

            // Assert
            Assert.Equal("You take 15 damage and find 50 gold!", result);
        }

        [Fact]
        public void EnemyType_HasCorrectProperties()
        {
            // Arrange
            EncounterManager.LoadEncounters(TestEncountersFile);
            var encounters = EncounterManager.GetEncounters();

            // Act
            var goblin = encounters.EnemyTypes["testGoblin"];

            // Assert
            Assert.Equal("Test Goblin", goblin.Name);
            Assert.Equal(5, goblin.DamageMin);
            Assert.Equal(10, goblin.DamageMax);
            Assert.Equal(1, goblin.GoldMin);
            Assert.Equal(5, goblin.GoldMax);
        }

        [Fact]
        public void CombatEncounter_HasCorrectStructure()
        {
            // Arrange
            EncounterManager.LoadEncounters(TestEncountersFile);
            var encounters = EncounterManager.GetEncounters();

            // Act
            var combatEncounter = encounters.CombatEncounters[0];

            // Assert
            Assert.Equal("test_single_goblin", combatEncounter.Id);
            Assert.Equal(10, combatEncounter.Weight);
            Assert.Equal(1, combatEncounter.MinLevel);
            Assert.Equal(5, combatEncounter.MaxLevel);
            Assert.Single(combatEncounter.Enemies);
            Assert.Equal("testGoblin", combatEncounter.Enemies[0].Type);
            Assert.Equal(1, combatEncounter.Enemies[0].Count);
            Assert.Single(combatEncounter.EncounterMessages);
            Assert.Single(combatEncounter.VictoryMessages);
            Assert.Single(combatEncounter.LootMessages);
        }

        [Fact]
        public void EnemyGroup_HasCorrectProperties()
        {
            // Arrange
            EncounterManager.LoadEncounters(TestEncountersFile);
            var encounters = EncounterManager.GetEncounters();

            // Act
            var enemyGroup = encounters.CombatEncounters[0].Enemies[0];

            // Assert
            Assert.Equal("testGoblin", enemyGroup.Type);
            Assert.Equal(1, enemyGroup.Count);
        }

        [Fact]
        public void AlreadySearched_HasMessages()
        {
            // Arrange
            EncounterManager.LoadEncounters(TestEncountersFile);
            var encounters = EncounterManager.GetEncounters();

            // Assert
            Assert.NotEmpty(encounters.AlreadySearched);
            Assert.Contains("You've already searched this room.", encounters.AlreadySearched);
        }

        [Fact]
        public void NeverHadEncounter_HasMessages()
        {
            // Arrange
            EncounterManager.LoadEncounters(TestEncountersFile);
            var encounters = EncounterManager.GetEncounters();

            // Assert
            Assert.NotEmpty(encounters.NeverHadEncounter);
            Assert.Contains("This room has nothing of value.", encounters.NeverHadEncounter);
        }
        
        [Fact]
        public void Encounters_LoadSuccessfully()
        {
            // Arrange
            EncounterManager.LoadEncounters();
            var encounters = EncounterManager.GetEncounters();

            // Assert
            Assert.NotEmpty(encounters.CombatEncounters);

            var regularEncounters = encounters.CombatEncounters.Where(e => !e.IsBoss).ToList();
            var bossEncounters = encounters.CombatEncounters.Where(e => e.IsBoss).ToList();

            Debug.WriteLine($"Regular encounters: {regularEncounters.Count}");
            Debug.WriteLine($"Boss encounters: {bossEncounters.Count}");

            Assert.NotEmpty(regularEncounters);
            Assert.NotEmpty(bossEncounters);
        }
    }
}
