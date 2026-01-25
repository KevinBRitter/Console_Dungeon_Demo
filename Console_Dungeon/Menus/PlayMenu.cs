using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.UI;

namespace Console_Dungeon.Menus
{
    internal class PlayMenu
    {
        public MenuAction Show()
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
                    return MenuAction.NewGame;

                case "2":
                    return MenuAction.ContinueGame;

                case "3":
                    ReturnToMain();
                    return MenuAction.Back;

                default:
                    InvalidChoice();
                    return MenuAction.Stay;
            }
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
