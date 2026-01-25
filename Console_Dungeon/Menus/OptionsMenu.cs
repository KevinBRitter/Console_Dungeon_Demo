using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.UI;

namespace Console_Dungeon.Menus
{
    public class OptionsMenu : IMenu
    {
        public MenuAction Show()
        {
            string menuText =
                "Options\n\n" +
                "  1) Audio\n" +
                "  2) Controls\n" +
                "  3) Return\n\n";

            // use defaults from ScreenRenderer
            ScreenRenderer.DrawScreen(menuText);

            string input = InputHandler.GetMenuChoice();

            switch (input)
            {
                case "1":
                    AudioOptions();
                    return MenuAction.AudioOptions;

                case "2":
                    ControlOptions();
                    return MenuAction.ControlOptions;

                case "3":
                    ReturnToMain();
                    return MenuAction.Back;

                default:
                    InvalidChoice();
                    return MenuAction.Stay;
            }
        }

        private static void AudioOptions()
        {
            ScreenRenderer.DrawScreen("Audio options selected. Press any key to return.");
            InputHandler.WaitForKey();
        }

        private static void ControlOptions()
        {
            ScreenRenderer.DrawScreen("Control options selected. Press any key to return.");
            InputHandler.WaitForKey();
        }

        private static void ReturnToMain()
        {
            ScreenRenderer.DrawScreen("Returning to main menu. Press any key to continue.");
            InputHandler.WaitForKey();
        }

        private static void InvalidChoice()
        {
            ScreenRenderer.DrawScreen("Invalid choice. Press any key to return.");
            InputHandler.WaitForKey();
        }
    }
}
