using System;
using System.IO;

namespace Console_Dungeon
{
    public static class DebugLogger
    {
        private static readonly string LogFilePath = "debug.log";
        private static readonly object LockObject = new object();

        // Echoing to the console corrupts the fixed-size box that ScreenRenderer draws,
        // so it stays off by default. Flip it on when you need live feedback in a terminal.
        public static bool EchoToConsole { get; set; } = false;

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

                    if (EchoToConsole)
                    {
                        Console.WriteLine($"[DEBUG] {message}");
                    }
                }
                catch
                {
                    // Logging is best-effort; never let it disturb the rendered screen.
                }
            }
        }
    }
}
