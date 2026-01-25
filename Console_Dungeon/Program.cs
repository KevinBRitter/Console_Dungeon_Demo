using Console_Dungeon.Enums;
using Console_Dungeon.Menus;

class Program
{
    static void Main(string[] args)
    {
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