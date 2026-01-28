using Console_Dungeon;
using Console_Dungeon.Enums;
using Console_Dungeon.Menus;
using Console_Dungeon.Models;

class Program
{
    static void Main(string[] args)
    {
        // Ensure console is at a usable size before starting UI
        EnsureConsoleSize(minWidth: 80, minHeight: 32);

        GameState? currentGameState = null;
        IMenu currentMenu = new MainMenu();
        MenuAction action = currentMenu.Show();

        while (true)
        {
            switch (action)
            {
                case MenuAction.Main:
                    action = currentMenu.Show();
                    break;

                case MenuAction.Play:
                    var playMenu = new PlayMenu();
                    action = playMenu.Show(); // Assume PlayMenu handles its own loop and returns to MainMenu when done
                    break;

                case MenuAction.NewGame:
                    // Initialize new game state
                    int seed = Environment.TickCount; // Use timestamp as seed
                    currentGameState = new GameState(seed);

                    // Start game loop
                    var newGameLoop = new GameLoop(currentGameState);
                    action = newGameLoop.Run();
                    break;

                case MenuAction.ContinueGame:
                    // TODO: Load saved game state (Phase 6)
                    // For now, this is handled by PlayMenu showing it as disabled
                    action = MenuAction.Play;
                    break;

                case MenuAction.Options:
                    var optionsMenu = new OptionsMenu();
                    action = optionsMenu.Show(); // Assume OptionsMenu handles its own loop and returns to MainMenu when done
                    break;

                case MenuAction.Exit:
                    return; // Exit the application

                default:
                    continue; // Stay in the current menu (MainMenu)
            }
        }
    }

    private static void EnsureConsoleSize(int minWidth = 80, int minHeight = 32)
    {
        try
        {
            // On Windows, buffer must be >= desired window size. Increase buffer first,
            // then set window size, then ensure buffer is at least window size.
            int desiredBufferWidth = Math.Max(Console.BufferWidth, minWidth);
            int desiredBufferHeight = Math.Max(Console.BufferHeight, minHeight);

            if (Console.BufferWidth < desiredBufferWidth || Console.BufferHeight < desiredBufferHeight)
            {
                Console.SetBufferSize(desiredBufferWidth, desiredBufferHeight);
            }

            int windowWidth = Math.Max(Console.WindowWidth, minWidth);
            int windowHeight = Math.Max(Console.WindowHeight, minHeight);

            Console.SetWindowSize(windowWidth, windowHeight);

            // Make sure buffer still at least equals window
            if (Console.BufferWidth < Console.WindowWidth || Console.BufferHeight < Console.WindowHeight)
            {
                Console.SetBufferSize(Math.Max(Console.BufferWidth, Console.WindowWidth), Math.Max(Console.BufferHeight, Console.WindowHeight));
            }

            // Reset position to top-left (some hosts keep previous position)
            Console.SetWindowPosition(0, 0);
        }
        catch (PlatformNotSupportedException)
        {
            // Console sizing not supported on this platform — renderer will adapt
        }
        catch (IOException)
        {
            // Host refused size changes (e.g. some terminals) — ignore and continue
        }
        catch (ArgumentOutOfRangeException)
        {
            // Requested sizes out of host bounds — ignore
        }
        catch
        {
            // Any other failure — swallow so app still runs
        }
    }
}