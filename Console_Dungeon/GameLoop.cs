using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon
{
    public class GameLoop
    {
        private GameState _gameState;

        public GameLoop(GameState gameState)
        {
            _gameState = gameState;

            // Ensure encounters are loaded
            EncounterManager.LoadEncounters();
        }

        // Main game loop - returns MenuAction to indicate how to exit
        public MenuAction Run()
        {
            while (true)
            {
                // Update last played time
                _gameState.UpdateLastPlayed();
                _gameState.TurnCount++;

                // Render current state
                RenderGameState();

                // Present player actions
                string action = GetPlayerAction();

                // Resolve action
                MenuAction? exitAction = ResolveAction(action);
                if (exitAction.HasValue)
                {
                    return exitAction.Value;
                }

                // Update world (enemies move, events trigger, etc.)
                UpdateWorld();

                // Check exit/death conditions
                if (!_gameState.Player.IsAlive)
                {
                    HandleDeath();
                    return MenuAction.Main;
                }
            }
        }

        private void RenderGameState()
        {
            string status =
                $"=== {_gameState.Player.Name} ===\n\n" +
                $"Level: {_gameState.Player.Level}  |  " +
                $"HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}  |  " +
                $"Gold: {_gameState.Player.Gold}\n" +
                $"Attack: {_gameState.Player.Attack}  |  Defense: {_gameState.Player.Defense}\n\n" +
                $"--- Dungeon Level {_gameState.CurrentLevel.LevelNumber} ---\n" +
                $"Rooms Explored: {_gameState.CurrentLevel.RoomsExplored}/{_gameState.CurrentLevel.TotalRooms}\n\n" +
                $"You stand in a dimly lit chamber. Shadows dance on the stone walls.\n\n" +
                $"What will you do?\n\n" +
                $"  1) Explore deeper\n" +
                $"  2) Rest (restore 20 HP)\n" +
                $"  3) Check status\n" +
                $"  4) Return to Main Menu\n\n";

            ScreenRenderer.DrawScreen(status);
        }

        private string GetPlayerAction()
        {
            return InputHandler.GetMenuChoice();
        }

        private MenuAction? ResolveAction(string action)
        {
            switch (action)
            {
                case "1":
                    ExploreRoom();
                    return null;
                case "2":
                    Rest();
                    return null;
                case "3":
                    ShowDetailedStatus();
                    return null;
                case "4":
                    if (ConfirmExit())
                    {
                        return MenuAction.Main;
                    }
                    return null;
                default:
                    InvalidAction();
                    return null;
            }
        }

        private void ExploreRoom()
        {
            _gameState.CurrentLevel.RoomsExplored++;

            // Simple random encounter (placeholder)
            Random rng = new Random(_gameState.Seed + _gameState.TurnCount);
            int encounterType = rng.Next(1, 11); // 1-10 for weighted distribution

            string encounterText;

            // Weighted encounters: 30% treasure, 50% combat, 20% empty
            if (encounterType <= 3)
            {
                // Treasure encounters (30%)
                encounterText = TreasureEncounter(rng);
            }
            else if (encounterType <= 8)
            {
                // Combat encounters (50%)
                encounterText = CombatEncounter(rng);
            }
            else
            {
                // Empty room encounters (20%)
                encounterText = EmptyRoomEncounter(rng);
            }

            ScreenRenderer.DrawScreen(encounterText + "\n\nPress any key to continue...");
            InputHandler.WaitForKey();
        }

        private string TreasureEncounter(Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            int goldAmount = rng.Next(5, 21); // 5-20 gold
            _gameState.Player.Gold += goldAmount;

            string template = encounters.TreasureEncounters[rng.Next(encounters.TreasureEncounters.Count)];
            return EncounterManager.FormatMessage(template, ("gold", goldAmount));
        }

        private string CombatEncounter(Random rng)
        {
            var encounters = EncounterManager.GetEncounters();

            // Choose random enemy type
            string[] enemyKeys = new[] { "goblin", "skeleton", "slime", "goblinPair" };
            string enemyKey = enemyKeys[rng.Next(enemyKeys.Length)];

            if (!encounters.Enemies.ContainsKey(enemyKey))
            {
                return "An unknown creature attacks!";
            }

            var enemy = encounters.Enemies[enemyKey];

            // Calculate damage and gold
            int damage;
            string combatMessage;

            if (enemyKey == "goblinPair")
            {
                // Special handling for two goblins
                int damage1 = rng.Next(8, 16);
                int damage2 = rng.Next(8, 16);
                damage = damage1 + damage2;

                string template = enemy.EncounterMessages[rng.Next(enemy.EncounterMessages.Count)];
                combatMessage = EncounterManager.FormatMessage(template,
                    ("damage1", damage1),
                    ("damage2", damage2),
                    ("damage", damage));
            }
            else
            {
                damage = rng.Next(enemy.DamageMin, enemy.DamageMax);
                string template = enemy.EncounterMessages[rng.Next(enemy.EncounterMessages.Count)];
                combatMessage = EncounterManager.FormatMessage(template, ("damage", damage));
            }

            int goldDropped = rng.Next(enemy.GoldMin, enemy.GoldMax);

            // Apply effects
            _gameState.Player.TakeDamage(damage);
            _gameState.Player.Gold += goldDropped;

            // Format loot message
            string lootTemplate = enemy.LootMessages[rng.Next(enemy.LootMessages.Count)];
            string lootMessage = EncounterManager.FormatMessage(lootTemplate, ("gold", goldDropped));

            return $"{combatMessage}\n{lootMessage}";
        }

        private string EmptyRoomEncounter(Random rng)
        {
            var encounters = EncounterManager.GetEncounters();
            return encounters.EmptyRooms[rng.Next(encounters.EmptyRooms.Count)];
        }

        private void Rest()
        {
            _gameState.Player.Heal(20);
            ScreenRenderer.DrawScreen(
                $"You rest for a moment, bandaging your wounds.\n\n" +
                $"Restored 20 HP. Current HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}\n\n" +
                $"Press any key to continue...");
            InputHandler.WaitForKey();
        }

        private void ShowDetailedStatus()
        {
            string statusText =
                $"=== Character Status ===\n\n" +
                $"Name: {_gameState.Player.Name}\n" +
                $"Level: {_gameState.Player.Level}\n" +
                $"Experience: {_gameState.Player.Experience}\n\n" +
                $"Health: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}\n" +
                $"Attack: {_gameState.Player.Attack}\n" +
                $"Defense: {_gameState.Player.Defense}\n" +
                $"Gold: {_gameState.Player.Gold}\n\n" +
                $"Current Location: Dungeon Level {_gameState.CurrentLevel.LevelNumber}\n" +
                $"Position: ({_gameState.Player.PositionX}, {_gameState.Player.PositionY})\n\n" +
                $"Turns Taken: {_gameState.TurnCount}\n" +
                $"Session Started: {_gameState.CreatedAt:g}\n\n" +
                $"Press any key to continue...";

            ScreenRenderer.DrawScreen(statusText);
            InputHandler.WaitForKey();
        }

        private bool ConfirmExit()
        {
            ScreenRenderer.DrawScreen(
                "Are you sure you want to return to the main menu?\n" +
                "Your progress will be lost until save functionality is implemented.\n\n" +
                "  1) Yes, return to menu\n" +
                "  2) No, continue playing\n\n");

            string input = InputHandler.GetMenuChoice();
            return input == "1";
        }

        private void InvalidAction()
        {
            ScreenRenderer.DrawScreen("Invalid action. Press any key to continue...");
            InputHandler.WaitForKey();
        }

        private void UpdateWorld()
        {
            // Placeholder for world updates (enemy turns, events, etc.)
            // This is where you'd update non-player entities in the future
        }

        private void HandleDeath()
        {
            string deathText =
                $"=== You Have Fallen ===\n\n" +
                $"{_gameState.Player.Name} has been defeated in the depths of the dungeon.\n\n" +
                $"Final Statistics:\n" +
                $"  Level: {_gameState.Player.Level}\n" +
                $"  Gold Collected: {_gameState.Player.Gold}\n" +
                $"  Rooms Explored: {_gameState.CurrentLevel.RoomsExplored}\n" +
                $"  Turns Survived: {_gameState.TurnCount}\n\n" +
                $"Your adventure ends here... for now.\n\n" +
                $"Press any key to return to the main menu...";

            ScreenRenderer.DrawScreen(deathText);
            InputHandler.WaitForKey();
        }
    }
}
