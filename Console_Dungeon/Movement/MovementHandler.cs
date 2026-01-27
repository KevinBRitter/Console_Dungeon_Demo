using Console_Dungeon.Encounters;
using Console_Dungeon.Input;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon.Movement
{
    public class MovementHandler
    {
        private readonly GameState _gameState;
        private readonly EncounterHandler _encounterHandler;

        public MovementHandler(GameState gameState, EncounterHandler encounterHandler)
        {
            _gameState = gameState;
            _encounterHandler = encounterHandler;
        }

        public bool TryMove(int dx, int dy)
        {
            int newX = _gameState.Player.PositionX + dx;
            int newY = _gameState.Player.PositionY + dy;

            // Check bounds
            if (newX < 0 || newX >= _gameState.CurrentLevel.Width ||
                newY < 0 || newY >= _gameState.CurrentLevel.Height)
            {
                ScreenRenderer.DrawScreen(MessageManager.GetMessage("movement.outOfBounds"));
                InputHandler.WaitForKey();
                return false;
            }

            var targetRoom = _gameState.CurrentLevel.GetRoom(newX, newY);

            // Check if blocked
            if (targetRoom.IsBlocked)
            {
                ScreenRenderer.DrawScreen(MessageManager.GetMessage("movement.blocked"));
                InputHandler.WaitForKey();
                return false;
            }

            // Move player
            _gameState.Player.PositionX = newX;
            _gameState.Player.PositionY = newY;

            bool wasVisited = targetRoom.Visited;
            string desc = wasVisited ? MessageManager.GetMessage("movement.returning") : targetRoom.Description;

            if (!wasVisited)
            {
                targetRoom.Visited = true;
                _gameState.CurrentLevel.RoomsExplored++;
                _gameState.TurnCount++;

                // Show room description
                ScreenRenderer.DrawScreen($"{desc}\n\nPress any key to continue...");
                InputHandler.WaitForKey();

                // Trigger encounter
                _encounterHandler.TriggerEncounter();
                return true;
            }
            else
            {
                // Already visited
                ScreenRenderer.DrawScreen($"{desc}\n\nPress any key to continue...");
                InputHandler.WaitForKey();
                return false;
            }
        }
    }
}
