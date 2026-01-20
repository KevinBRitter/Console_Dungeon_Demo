using Console_Dungeon.Menus;

class Program
{
    static void Main()
    {
        bool running = true;

        while (running)
        {
            running = MainMenu.Show();
        }
    }
}