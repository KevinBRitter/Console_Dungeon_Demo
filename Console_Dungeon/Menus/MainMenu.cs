using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.UI;

namespace Console_Dungeon.Menus
{
    public class MainMenu: IMenu
    {
        public MenuAction Show()
        {
            string menuText =
                "Main Menu\n\n" +
                "  1) Play\n" +
                "  2) Options\n" +
                "  3) Exit\n\n";

            // use defaults from ScreenRenderer
            ScreenRenderer.DrawScreen(menuText);

            string userInput = InputHandler.GetMenuChoice();

            switch (userInput)
            {
                case "1":
                    return MenuAction.Play;

                case "2":
                    return MenuAction.Options;

                case "3":
                    Exit();
                    return MenuAction.Exit;

                default:
                    InvalidChoice();
                    return MenuAction.Main;
            }
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
