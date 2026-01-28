using System.Diagnostics;
using System.Text.Json;

namespace Console_Dungeon.Managers
{
    public class RoomDescriptionManager
    {
        private static RoomDescriptions? _descriptions;

        public static void LoadDescriptions(string filePath = "Data/RoomDescriptions.json")
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                _descriptions = JsonSerializer.Deserialize<RoomDescriptions>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_descriptions == null)
                {
                    throw new Exception("Failed to deserialize room descriptions.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading room descriptions: {ex.Message}");
                _descriptions = CreateDefaultDescriptions();
            }
        }

        public static RoomDescriptions GetDescriptions()
        {
            if (_descriptions == null)
            {
                LoadDescriptions();
            }
            return _descriptions ?? CreateDefaultDescriptions();
        }

        public static string GetRandomStandardRoom(Random rng)
        {
            var descriptions = GetDescriptions();
            return descriptions.StandardRooms[rng.Next(descriptions.StandardRooms.Count)];
        }

        public static string GetRandomBossRoom(Random rng)
        {
            var descriptions = GetDescriptions();
            return descriptions.BossRooms[rng.Next(descriptions.BossRooms.Count)];
        }

        public static string GetRandomBlockedRoom(Random rng, int x, int y)
        {
            var descriptions = GetDescriptions();
            string template = descriptions.BlockedRooms[rng.Next(descriptions.BlockedRooms.Count)];
            return template.Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
        }

        public static string GetRandomSpecialRoom(string roomType, Random rng)
        {
            var descriptions = GetDescriptions();

            if (descriptions.SpecialRooms.ContainsKey(roomType))
            {
                var roomList = descriptions.SpecialRooms[roomType];
                return roomList[rng.Next(roomList.Count)];
            }

            return GetRandomStandardRoom(rng);
        }

        private static RoomDescriptions CreateDefaultDescriptions()
        {
            return new RoomDescriptions
            {
                StandardRooms = new List<string>
                {
                    "A cold stone chamber.",
                    "A dark corridor opens before you.",
                    "An ancient hall, worn by time."
                },
                BossRooms = new List<string>
                {
                    "A massive chamber looms before you. This is clearly the lair of something powerful..."
                },
                BlockedRooms = new List<string>
                {
                    "Stone wall ({x},{y})."
                },
                SpecialRooms = new Dictionary<string, List<string>>()
            };
        }
    }

    [Serializable]
    public class RoomDescriptions
    {
        public List<string> StandardRooms { get; set; } = new List<string>();
        public List<string> BossRooms { get; set; } = new List<string>();
        public List<string> BlockedRooms { get; set; } = new List<string>();
        public Dictionary<string, List<string>> SpecialRooms { get; set; } = new Dictionary<string, List<string>>();
    }
}
