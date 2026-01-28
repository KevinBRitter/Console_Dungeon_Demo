using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Console_Dungeon.Managers
{
    public class MessageManager
    {
        private static Dictionary<string, object>? _messages;

        public static void LoadMessages(string filePath = "Data/Messages.json")
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                _messages = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (_messages == null)
                {
                    throw new Exception("Failed to deserialize messages.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading messages: {ex.Message}");
                _messages = new Dictionary<string, object>();
            }
        }

        public static string GetMessage(string path, params (string key, object value)[] replacements)
        {
            if (_messages == null)
            {
                LoadMessages();
            }

            string message = GetNestedValue(path);

            foreach (var (key, value) in replacements)
            {
                message = message.Replace($"{{{key}}}", value.ToString());
            }

            return message;
        }

        public static T GetValue<T>(string path)
        {
            if (_messages == null)
            {
                LoadMessages();
            }

            var value = GetNestedValue(path);

            if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(value);
            }

            return (T)(object)value;
        }

        private static string GetNestedValue(string path)
        {
            var parts = path.Split('.');
            object? current = _messages;

            foreach (var part in parts)
            {
                if (current is Dictionary<string, object> dict && dict.ContainsKey(part))
                {
                    current = dict[part];
                }
                else if (current is JsonElement element)
                {
                    current = element.GetProperty(part);
                }
                else
                {
                    return path; // Return path as fallback
                }
            }

            if (current is JsonElement jsonElement)
            {
                return jsonElement.ToString();
            }

            return current?.ToString() ?? path;
        }
    }
}
