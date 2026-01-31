using System.Text.Json;

namespace Console_Dungeon.Managers
{
    public static class MetaUnlockManager
    {
        private static HashSet<string> _unlocks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static string _filePath = "Data/MetaUnlocks.json";

        static MetaUnlockManager()
        {
            Load();
        }

        public static void Load(string? filePath = null)
        {
            if (filePath != null) _filePath = filePath;

            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var list = JsonSerializer.Deserialize<List<string>>(json);
                    _unlocks = list != null ? new HashSet<string>(list, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    _unlocks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch
            {
                _unlocks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public static bool IsUnlocked(string itemId) => _unlocks.Contains(itemId);

        public static void Unlock(string itemId)
        {
            if (_unlocks.Add(itemId))
            {
                Save();
            }
        }

        private static void Save()
        {
            try
            {
                var list = _unlocks.ToList();
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // swallow; non-critical
            }
        }
    }
}