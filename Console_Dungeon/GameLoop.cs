using Console_Dungeon.Actions;
using Console_Dungeon.Encounters;
using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using Console_Dungeon.Movement;
using Console_Dungeon.UI;

namespace Console_Dungeon
{
    public class GameLoop
    {
        private readonly GameState _gameState;
        private readonly EncounterHandler _encounterHandler;
        private readonly MovementHandler _movementHandler;

        public GameLoop(GameState gameState)
        {
            _gameState = gameState;

            // Load configuration
            EncounterManager.LoadEncounters();
            MessageManager.LoadMessages();
            RoomDescriptionManager.LoadDescriptions();
            CharacterClassManager.LoadClasses(); // NEW

            // Initialize handlers
            _encounterHandler = new EncounterHandler(gameState);
            _movementHandler = new MovementHandler(gameState, _encounterHandler);
        }

        public MenuAction Run()
        {
            while (true)
            {
                _gameState.UpdateLastPlayed();
                _gameState.TurnCount++;

                RenderGameState();

                string action = InputHandler.GetMenuChoice();
                MenuAction? exitAction = ResolveAction(action);

                if (exitAction.HasValue)
                {
                    return exitAction.Value;
                }

                UpdateWorld();

                if (!_gameState.Player.IsAlive)
                {
                    HandleDeath();
                    return MenuAction.Main;
                }
            }
        }

        private void RenderGameState()
        {
            string xpBar = _gameState.Player.GetXPProgressBar(25);

            string status =
                $"=== {_gameState.Player.Name} ===\n\n" +
                $"Level: {_gameState.Player.Level}  |  " +
                $"HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}  |  " +
                $"Gold: {_gameState.Player.Gold}\n" +
                $"Attack: {_gameState.Player.Attack}  |  Defense: {_gameState.Player.Defense}\n" +
                $"{xpBar}\n\n" +  // NEW: XP Progress Bar
                $"--- Dungeon Level {_gameState.CurrentLevel.LevelNumber} ---\n" +
                $"Rooms Explored: {_gameState.CurrentLevel.RoomsExplored}/{_gameState.CurrentLevel.TotalRooms}\n\n" +
                MessageManager.GetMessage("mainActions.header") +
                MessageManager.GetMessage("mainActions.options");

            ScreenRenderer.DrawScreen(status);
        }

        private MenuAction? ResolveAction(string action)
        {
            switch (action)
            {
                case "1":
                    ShowMapView();
                    CheckForLevelUp(); // NEW
                    CheckLevelCompletion();
                    return null;
                case "2":
                    _encounterHandler.TriggerEncounter();
                    CheckForLevelUp(); // NEW
                    CheckLevelCompletion();
                    return null;
                case "3":
                    new RestAction().Execute(_gameState);
                    return null;
                case "4":
                    new ShowStatusAction().Execute(_gameState);
                    return null;
                case "5":
                    return ConfirmExit() ? MenuAction.Main : null;
                default:
                    ShowInvalidAction();
                    return null;
            }
        }

        private void CheckForLevelUp()
        {
            if (_gameState.Player.HasPendingLevelUp)
            {
                var levelUpInfo = _gameState.Player.GetAndClearLevelUp();
                if (levelUpInfo != null)
                {
                    var levelUpAction = new ShowLevelUpAction(levelUpInfo);
                    levelUpAction.Execute(_gameState);
                }
            }
        }
        private void CheckLevelCompletion()
        {
            if (_gameState.CurrentLevel.IsComplete)
            {
                ShowLevelCompleteScreen();
                AdvanceToNextLevel();
            }
        }

        private void ShowLevelCompleteScreen()
        {
            string message =
                $"=== Level {_gameState.CurrentLevel.LevelNumber} Complete! ===\n\n" +
                $"You have defeated the boss and conquered this level!\n\n" +
                $"Statistics:\n" +
                $"  Rooms Explored: {_gameState.CurrentLevel.RoomsExplored}\n" +
                $"  Enemies Defeated: {_gameState.Player.Kills}\n" +
                $"  Gold Collected: {_gameState.Player.Gold}\n" +
                $"  Current HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}\n\n" +
                $"Descending deeper into the dungeon...\n\n" +
                $"Press any key to continue...";

            ScreenRenderer.DrawScreen(message);
            InputHandler.WaitForKey();
        }

        private void AdvanceToNextLevel()
        {
            // Create next level
            int nextLevelNumber = _gameState.CurrentLevel.LevelNumber + 1;
            _gameState.CurrentLevel = new DungeonLevel(nextLevelNumber, _gameState.Seed + nextLevelNumber);

            // Reset player position to start
            _gameState.Player.PositionX = 0;
            _gameState.Player.PositionY = 0;

            // Optional: Heal player partially
            int healAmount = _gameState.Player.MaxHealth / 4; // 25% heal
            _gameState.Player.Heal(healAmount);

            string message =
                $"=== Dungeon Level {nextLevelNumber} ===\n\n" +
                $"You descend deeper into the darkness.\n" +
                $"The air grows colder and more oppressive.\n\n" +
                $"You recover {healAmount} HP as you catch your breath.\n" +
                $"Current HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth}\n\n" +
                $"Press any key to continue...";

            ScreenRenderer.DrawScreen(message);
            InputHandler.WaitForKey();
        }

        private void ShowMapView()
        {
            while (true)
            {
                string map = ScreenRenderer.RenderMap(_gameState.CurrentLevel, _gameState.Player);
                string mapText = $"{MessageManager.GetMessage("mapView.title")}\n{map}\n\n" +
                               MessageManager.GetMessage("mapView.actions");

                ScreenRenderer.DrawScreen(mapText);
                string input = InputHandler.GetMenuChoice();

                switch (input)
                {
                    case "1": if (_movementHandler.TryMove(0, -1)) return; break;
                    case "2": if (_movementHandler.TryMove(0, 1)) return; break;
                    case "3": if (_movementHandler.TryMove(1, 0)) return; break;
                    case "4": if (_movementHandler.TryMove(-1, 0)) return; break;
                    case "5": return;
                    default:
                        ScreenRenderer.DrawScreen(MessageManager.GetMessage("menu.invalidMapChoice"));
                        InputHandler.WaitForKey();
                        break;
                }
            }
        }

        private bool ConfirmExit()
        {
            ScreenRenderer.DrawScreen(MessageManager.GetMessage("menu.confirmExit"));
            string input = InputHandler.GetMenuChoice();
            return input == "1";
        }

        private void ShowInvalidAction()
        {
            ScreenRenderer.DrawScreen(MessageManager.GetMessage("menu.invalidAction"));
            InputHandler.WaitForKey();
        }

        private void UpdateWorld()
        {
            // Placeholder for world updates
        }

        private void HandleDeath()
        {
            string message = MessageManager.GetMessage("death.message",
                ("name", _gameState.Player.Name),
                ("level", _gameState.Player.Level),
                ("gold", _gameState.Player.Gold),
                ("rooms", _gameState.CurrentLevel.RoomsExplored),
                ("kills", _gameState.Player.Kills),
                ("turns", _gameState.TurnCount));

            string fullMessage = $"{MessageManager.GetMessage("death.title")}\n\n{message}";

            ScreenRenderer.DrawScreen(fullMessage);
            InputHandler.WaitForKey();
        }
    }
}