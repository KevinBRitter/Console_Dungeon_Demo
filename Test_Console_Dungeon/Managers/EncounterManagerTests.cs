using Console_Dungeon.Models;
using System.Text.Json;

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
                Enemies = new Dictionary<string, EnemyData>
                {
                    ["testGoblin"] = new EnemyData
                    {
                        Name = "Test Goblin",
                        DamageMin = 5,
                        DamageMax = 10,
                        GoldMin = 1,
                        GoldMax = 5,
                        EncounterMessages = new List<string> { "A test goblin attacks! {damage} damage." },
                        LootMessages = new List<string> { "You loot {gold} gold." }
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
            Assert.Single(encounters.Enemies);
            Assert.True(encounters.Enemies.ContainsKey("testGoblin"));
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
            Assert.NotEmpty(encounters.Enemies);
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
        public void EnemyData_HasCorrectProperties()
        {
            // Arrange
            EncounterManager.LoadEncounters(TestEncountersFile);
            var encounters = EncounterManager.GetEncounters();

            // Act
            var goblin = encounters.Enemies["testGoblin"];

            // Assert
            Assert.Equal("Test Goblin", goblin.Name);
            Assert.Equal(5, goblin.DamageMin);
            Assert.Equal(10, goblin.DamageMax);
            Assert.Equal(1, goblin.GoldMin);
            Assert.Equal(5, goblin.GoldMax);
            Assert.Single(goblin.EncounterMessages);
            Assert.Single(goblin.LootMessages);
        }
    }
}
