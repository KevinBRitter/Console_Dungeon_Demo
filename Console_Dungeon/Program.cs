using Console_Dungeon;
using Console_Dungeon.Enums;
using Console_Dungeon.Menus;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

class Program
{
    static void Main(string[] args)
    {
        // Ensure console is at a usable size before starting UI.
        // EnsureConsoleSize will size the host window larger than the renderer's fixed box.
        EnsureConsoleSize();

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

                case MenuAction.CharacterCreation:
                    var charCreationMenu = new CharacterCreationMenu();
                    action = charCreationMenu.Show();

                    // If character was created, start new game with that character
                    if (action == MenuAction.NewGame)
                    {
                        int seed = Environment.TickCount;
                        currentGameState = new GameState(seed);

                        // Re-use the existing Player instance created by GameState.
                        // Preserve the starting Position set in GameState and update identity/class/stats.
                        currentGameState.Player.SetNameAndClass(
                            charCreationMenu.CreatedCharacterName,
                            charCreationMenu.CreatedCharacterClass);

                        var newGameLoop = new GameLoop(currentGameState);
                        action = newGameLoop.Run();
                    }
                    break;

                case MenuAction.NewGame:
                    // Direct new game (shouldn't happen now, but keep as fallback)
                    int fallbackSeed = Environment.TickCount;
                    currentGameState = new GameState(fallbackSeed);

                    var fallbackGameLoop = new GameLoop(currentGameState);
                    action = fallbackGameLoop.Run();
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

    // Ensure console window is larger than the fixed-size game box used by ScreenRenderer.
    // The renderer now draws a fixed-size box (ScreenRenderer.BoxWidth/BoxHeight).  Make the
    // host console at least a few columns/rows larger so the asterisk box never fills the window.
    private static void EnsureConsoleSize()
    {
        // Keep small safety margins so the window is noticeably larger than the game box.
        const int horizontalMargin = 6;
        const int verticalMargin = 4;

        int minWidth = ScreenRenderer.BoxWidth + horizontalMargin;
        int minHeight = ScreenRenderer.BoxHeight + verticalMargin;

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
            // Console sizing not supported on this platform — renderer will adapt (but may be clipped)
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
