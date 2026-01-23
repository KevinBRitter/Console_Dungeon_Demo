using Console_Dungeon.Input;
using Console_Dungeon.UI;

namespace Console_Dungeon.Menus
{
    internal class PlayMenu
    {
        public bool Show()
        {
            string menuText =
                "Play\n\n" +
                "  1) New Adventure\n" +
                "  2) Continue\n" +
                "  3) Return\n\n";

            // use defaults from ScreenRenderer
            ScreenRenderer.DrawScreen(menuText);

            string input = InputHandler.GetMenuChoice();

            switch (input)
            {
                case "1":
                    NewGame();
                    return true;

                case "2":
                    ContinueGame();
                    return true;

                case "3":
                    ReturnToMain();
                    return false;

                default:
                    InvalidChoice();
                    return true;
            }
        }

        private static void NewGame()
        {
            ScreenRenderer.DrawScreen("New game selected. Press any key to return.");
            InputHandler.WaitForKey();
        }

        private static void ContinueGame()
        {
            ScreenRenderer.DrawScreen("Continue game selected. Press any key to return.");
            InputHandler.WaitForKey();
        }

        private static void ReturnToMain()
        {
            ScreenRenderer.DrawScreen("Return to main menu selected.  Press any key to return.");
            InputHandler.WaitForKey();
        }

        private static void InvalidChoice()
        {
            ScreenRenderer.DrawScreen("Invalid choice. Press any key to return.");
            InputHandler.WaitForKey();
        }
    }
}
