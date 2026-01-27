using System;
using System.IO;

namespace Console_Dungeon
{
    public static class DebugLogger
    {
        private static readonly string LogFilePath = "debug.log";
        private static readonly object LockObject = new object();

        static DebugLogger()
        {
            // Clear log file on startup
            try
            {
                File.WriteAllText(LogFilePath, $"=== Debug Log Started: {DateTime.Now} ===\n\n");
            }
            catch { }
        }

        public static void Log(string message)
        {
            lock (LockObject)
            {
                try
                {
                    File.AppendAllText(LogFilePath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
                    // Also write to console for immediate feedback
                    Console.WriteLine($"[DEBUG] {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOG ERROR] {ex.Message}");
                }
            }
        }
    }
}
