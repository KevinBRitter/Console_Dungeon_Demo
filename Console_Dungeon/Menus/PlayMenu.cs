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
                "  2) Continue (Not yet implemented)\n" +
                "  3) Return\n\n";

            ScreenRenderer.DrawScreen(menuText);
            string input = InputHandler.GetMenuChoice();

            switch (input)
            {
                case "1":
                    return MenuAction.CharacterCreation; // Route to character creation
                case "2":
                    ContinueNotAvailable();
                    return MenuAction.Play;
                case "3":
                    ReturnToMain();
                    return MenuAction.Main;
                default:
                    InvalidChoice();
                    return MenuAction.Play;
            }
        }
        private static void ContinueNotAvailable()
        {
            ScreenRenderer.DrawScreen(
                "Continue Game\n\n" +
                "Save functionality is not yet implemented.\n" +
                "This feature will be available in Phase 6.\n\n" +
                "Press any key to return...");
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
