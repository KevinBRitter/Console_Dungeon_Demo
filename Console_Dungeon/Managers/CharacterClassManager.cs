using Console_Dungeon.Enums;
using Console_Dungeon.Models;
using System.Text.Json;

namespace Console_Dungeon.Managers
{
    public class CharacterClassManager
    {
        private static CharacterClassCollection? _classes;

        public static void LoadClasses(string filePath = "Data/CharacterClasses.json")
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                _classes = JsonSerializer.Deserialize<CharacterClassCollection>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_classes == null)
                {
                    throw new Exception("Failed to deserialize character classes.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading character classes: {ex.Message}");
                _classes = CreateDefaultClasses();
            }
        }

        public static CharacterClassCollection GetClasses()
        {
            if (_classes == null)
            {
                LoadClasses();
            }
            return _classes ?? CreateDefaultClasses();
        }

        public static CharacterClassData? GetClassData(string classId)
        {
            var classes = GetClasses();
            return classes.Classes.FirstOrDefault(c =>
                c.Id.Equals(classId, StringComparison.OrdinalIgnoreCase));
        }

        public static CharacterClassData? GetClassDataByEnum(PlayerClass playerClass)
        {
            string classId = playerClass.ToString().ToLower();
            return GetClassData(classId);
        }

        private static CharacterClassCollection CreateDefaultClasses()
        {
            return new CharacterClassCollection
            {
                Classes = new List<CharacterClassData>
                {
                    new CharacterClassData
                    {
                        Id = "warrior",
                        Name = "Warrior",
                        Description = "A sturdy fighter with balanced stats.",
                        FlavorText = "Warriors are masters of combat.",
                        StartingStats = new ClassStats
                        {
                            MaxHealth = 120,
                            Attack = 12,
                            Defense = 8,
                            MaxStamina = 100,
                            MaxMana = 0
                        },
                        LevelUpGains = new ClassStats
                        {
                            MaxHealth = 15,
                            Attack = 2,
                            Defense = 2,
                            MaxStamina = 10,
                            MaxMana = 0
                        }
                    }
                }
            };
        }
    }
}
