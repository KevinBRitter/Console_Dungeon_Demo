using Console_Dungeon.Menus;

class Program
{
    static void Main()
    {
        bool running = true;
        var mainMenu = new MainMenu();

        while (running)
        {
            running = mainMenu.Show();
        }
    }
}