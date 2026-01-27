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
            // Map removed from the main status to avoid overflowing the render area.
            string status =
                $"=== {_gameState.Player.Name} ===\n\n" +
                $"Level: {_gameState.Player.Level}  |  " +
                $"HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}  |  " +
                $"Gold: {_gameState.Player.Gold}\n" +
                $"Attack: {_gameState.Player.Attack}  |  Defense: {_gameState.Player.Defense}\n\n" +
                $"--- Dungeon Level {_gameState.CurrentLevel.LevelNumber} ---\n" +
                $"Rooms Explored: {_gameState.CurrentLevel.RoomsExplored}/{_gameState.CurrentLevel.TotalRooms}\n\n" +
                // Make viewing the map a dedicated action to avoid overflowing the body
                $"What will you do?\n\n" +
                $"  1) View Map (choose direction from the map)\n" +
                $"  2) Search current room (may trigger events)\n" +
                $"  3) Rest (restore 20 HP)\n" +
                $"  4) Check status\n" +
                $"  5) Return to Main Menu\n\n";

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
                    ShowMapView();
                    return null;
                case "2":
                    ExploreRoom();
                    return null;
                case "3":
                    Rest();
                    return null;
                case "4":
                    ShowDetailedStatus();
                    return null;
                case "5":
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

        // Map-centered view: shows the map and accepts direction commands.
        // Choosing a direction performs the move immediately. Moving into an unexplored room
        // automatically triggers the room's encounter and returns to the main action screen.
        private void ShowMapView()
        {
            while (true)
            {
                string map = ScreenRenderer.RenderMap(_gameState.CurrentLevel, _gameState.Player);

                string mapText =
                    $"Map:\n{map}\n\n" +
                    $"Choose a direction to move from the map view:\n\n" +
                    $"  1) Move North\n" +
                    $"  2) Move South\n" +
                    $"  3) Move East\n" +
                    $"  4) Move West\n" +
                    $"  5) Back to menu\n\n";

                ScreenRenderer.DrawScreen(mapText);

                string input = InputHandler.GetMenuChoice();

                switch (input)
                {
                    case "1":
                        if (TryMove(0, -1)) return; // encounter triggered -> return to action screen
                        break;
                    case "2":
                        if (TryMove(0, 1)) return;
                        break;
                    case "3":
                        if (TryMove(1, 0)) return;
                        break;
                    case "4":
                        if (TryMove(-1, 0)) return;
                        break;
                    case "5":
                        return; // back to main game menu
                    default:
                        ScreenRenderer.DrawScreen("Invalid choice in map view. Press any key to continue...");
                        InputHandler.WaitForKey();
                        break;
                }

                // If TryMove returned false (no encounter), loop continues and map view is shown again.
            }
        }

        // Returns true when an encounter was triggered (move into an unexplored room),
        // which signals the caller (map view) to return to the action screen.
        private bool TryMove(int dx, int dy)
        {
            int newX = _gameState.Player.PositionX + dx;
            int newY = _gameState.Player.PositionY + dy;

            if (newX < 0 || newX >= _gameState.CurrentLevel.Width || newY < 0 || newY >= _gameState.CurrentLevel.Height)
            {
                ScreenRenderer.DrawScreen("You cannot move that way. The path is blocked.\n\nPress any key to continue...");
                InputHandler.WaitForKey();
                return false;
            }

            var targetRoom = _gameState.CurrentLevel.GetRoom(newX, newY);

            // Prevent entering blocked rooms (walls). Do not change position.
            if (targetRoom.IsBlocked)
            {
                ScreenRenderer.DrawScreen("You cannot enter that space. A solid wall blocks your path.\n\nPress any key to continue...");
                InputHandler.WaitForKey();
                return false;
            }

            // Safe to move: update position
            _gameState.Player.PositionX = newX;
            _gameState.Player.PositionY = newY;

            bool wasVisited = targetRoom.Visited;
            string desc = wasVisited ? "You return to a familiar chamber." : targetRoom.Description;

            if (!wasVisited)
            {
                targetRoom.Visited = true;
                _gameState.CurrentLevel.RoomsExplored++;

                // Count movement as a turn so turns are tracked when moving from the map view.
                _gameState.TurnCount++;

                // Show the room description briefly before triggering the encounter.
                ScreenRenderer.DrawScreen($"{desc}\n\nPress any key to continue...");
                InputHandler.WaitForKey();

                // Automatically trigger the room encounter and return to the main action screen.
                ExploreRoom();
                return true;
            }
            else
            {
                // Visited room: show description and stay in the map view.
                ScreenRenderer.DrawScreen($"{desc}\n\nPress any key to continue...");
                InputHandler.WaitForKey();
                return false;
            }
        }

        private void ExploreRoom()
        {
            // Searching the current room can trigger an encounter; reuse existing logic but do not auto-increment RoomsExplored
            // (RoomsExplored is handled when the player first visits a room).
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
            if (enemyKey == "goblinPair")
            {
                _gameState.Player.Kills += 2;
            }
            else
            {
                _gameState.Player.Kills++;
            }

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
                $"Enemies Defeated: {_gameState.Player.Kills}\n" +
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
                $"  Enemies Defeated: {_gameState.Player.Kills}\n" +
                $"  Turns Survived: {_gameState.TurnCount}\n\n" +
                $"Your adventure ends here... for now.\n\n" +
                $"Press any key to return to the main menu...";

            ScreenRenderer.DrawScreen(deathText);
            InputHandler.WaitForKey();
        }
    }
}
