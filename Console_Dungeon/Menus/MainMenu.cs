using Console_Dungeon.Input;
using Console_Dungeon.UI;

namespace Console_Dungeon.Menus
{
    public class MainMenu: IMenu
    {
        public bool Show()
        {
            string menuText =
                "Main Menu\n\n" +
                "  1) Play\n" +
                "  2) Options\n" +
                "  3) Exit\n\n";

            // use defaults from ScreenRenderer
            ScreenRenderer.DrawScreen(menuText);

            string input = InputHandler.GetMenuChoice();

            switch (input)
            {
                case "1":
                    Play();
                    return true;

                case "2":
                    Options();
                    return true;

                case "3":
                    Exit();
                    return false;

                default:
                    InvalidChoice();
                    return true;
            }
        }

        private static void Play()
        {
            var playMenu = new PlayMenu();

            // Loop while the PlayMenu wants to remain active.
            // PlayMenu.Show() returns true for "stay" (New/Continue) and false for "Return".
            bool stayInPlay = true;
            while (stayInPlay)
            {
                stayInPlay = playMenu.Show();
            }
        }

        private static void Options()
        {
            ScreenRenderer.DrawScreen("Options selected. Press any key to return.");
            InputHandler.WaitForKey();
        }

        private static void Exit()
        {
            ScreenRenderer.DrawScreen("Your time in the dungeon has ended. Goodbye.");
        }

        private static void InvalidChoice()
        {
            ScreenRenderer.DrawScreen("Invalid choice. Press any key to return.");
            InputHandler.WaitForKey();
        }
    }    
}
