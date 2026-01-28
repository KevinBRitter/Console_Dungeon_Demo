using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.Managers;
using Console_Dungeon.UI;

namespace Console_Dungeon.Menus
{
    public class CharacterCreationMenu : IMenu
    {
        public string CreatedCharacterName { get; private set; } = string.Empty;
        public PlayerClass CreatedCharacterClass { get; private set; }

        public MenuAction Show()
        {
            // Ensure classes are loaded
            CharacterClassManager.LoadClasses();

            // Step 1: Get character name
            string characterName = GetCharacterName();

            if (string.IsNullOrWhiteSpace(characterName))
            {
                return MenuAction.Play;
            }

            // Step 2: Choose class
            PlayerClass? selectedClass = ChooseClass();

            if (!selectedClass.HasValue)
            {
                return MenuAction.Play;
            }

            // Step 3: Confirm character
            if (ConfirmCharacter(characterName, selectedClass.Value))
            {
                CreatedCharacterName = characterName;
                CreatedCharacterClass = selectedClass.Value;
                return MenuAction.NewGame;
            }

            return MenuAction.Play;
        }

        private string GetCharacterName()
        {
            string namePrompt =
                "=== Create Your Character ===\n\n" +
                "Enter your character's name:\n" +
                "(or press Enter to cancel)\n\n";

            ScreenRenderer.DrawScreen(namePrompt);

            string? name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            return name.Trim().Substring(0, Math.Min(name.Trim().Length, 20));
        }

        private PlayerClass? ChooseClass()
        {
            var classes = CharacterClassManager.GetClasses();

            while (true)
            {
                string classPrompt = "=== Choose Your Class ===\n\n";

                int index = 1;
                foreach (var classData in classes.Classes)
                {
                    classPrompt += $"  {index}) {classData.Name}\n";
                    classPrompt += $"     HP: {classData.StartingStats.MaxHealth} | " +
                                   $"Attack: {classData.StartingStats.Attack} | " +
                                   $"Defense: {classData.StartingStats.Defense}";

                    if (classData.StartingStats.MaxMana > 0)
                    {
                        classPrompt += $" | Mana: {classData.StartingStats.MaxMana}";
                    }

                    if (classData.StartingStats.MaxStamina > 0)
                    {
                        classPrompt += $" | Stamina: {classData.StartingStats.MaxStamina}";
                    }

                    classPrompt += $"\n     {classData.Description}\n\n";
                    index++;
                }

                classPrompt += $"  {index}) Back\n\n";

                ScreenRenderer.DrawScreen(classPrompt);

                string input = InputHandler.GetMenuChoice();

                if (int.TryParse(input, out int choice))
                {
                    if (choice == index)
                    {
                        return null; // Back
                    }

                    if (choice >= 1 && choice <= classes.Classes.Count)
                    {
                        var selectedClass = classes.Classes[choice - 1];
                        return Enum.Parse<PlayerClass>(selectedClass.Name, ignoreCase: true);
                    }
                }

                InvalidChoice();
            }
        }

        private bool ConfirmCharacter(string name, PlayerClass playerClass)
        {
            var classData = CharacterClassManager.GetClassDataByEnum(playerClass);

            if (classData == null)
            {
                return false;
            }

            string confirmText =
                "=== Confirm Your Character ===\n\n" +
                $"Name: {name}\n" +
                $"Class: {classData.Name}\n\n" +
                $"Starting Stats:\n" +
                $"  Health: {classData.StartingStats.MaxHealth}\n" +
                $"  Attack: {classData.StartingStats.Attack}\n" +
                $"  Defense: {classData.StartingStats.Defense}\n";

            if (classData.StartingStats.MaxMana > 0)
            {
                confirmText += $"  Mana: {classData.StartingStats.MaxMana}\n";
            }

            if (classData.StartingStats.MaxStamina > 0)
            {
                confirmText += $"  Stamina: {classData.StartingStats.MaxStamina}\n";
            }

            confirmText +=
                $"\n{classData.FlavorText}\n\n" +
                "  1) Begin Adventure\n" +
                "  2) Start Over\n\n";

            ScreenRenderer.DrawScreen(confirmText);

            string input = InputHandler.GetMenuChoice();
            return input == "1";
        }

        private static void InvalidChoice()
        {
            ScreenRenderer.DrawScreen("Invalid choice. Press any key to return.");
            InputHandler.WaitForKey();
        }
    }
}