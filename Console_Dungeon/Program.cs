using Console_Dungeon.Enums;
using Console_Dungeon.Menus;

class Program
{
    static void Main(string[] args)
    {
        IMenu currentMenu = new MainMenu();

        while (true)
        {
            MenuAction action = currentMenu.Show();
            switch (action)
            { 
                case MenuAction.Play:
                    var playMenu = new PlayMenu();
                    playMenu.Show(); // Assume PlayMenu handles its own loop and returns to MainMenu when done
                    break;
                case MenuAction.Options:
                    var optionsMenu = new OptionsMenu();
                    optionsMenu.Show(); // Assume OptionsMenu handles its own loop and returns to MainMenu when done
                    break;
                case MenuAction.Exit:
                    return; // Exit the application
                case MenuAction.Stay:
                default:
                    continue; // Stay in the current menu
            }
        }
    }
}