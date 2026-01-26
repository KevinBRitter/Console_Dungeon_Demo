using Console_Dungeon;
using Console_Dungeon.Enums;
using Console_Dungeon.Menus;
using Console_Dungeon.Models;

class Program
{
    static void Main(string[] args)
    {
        // Set console window size
        try
        {
            Console.SetWindowSize(80, 32);  // Width: 80, Height: 30
            Console.SetBufferSize(80, 32);  // Match buffer to window size
        }
        catch (PlatformNotSupportedException)
        {
            // Some platforms (like Linux/Mac terminals) don't support SetWindowSize
            // The game will still work, user just needs to resize manually
        }
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
}