using Console_Dungeon.Input;
using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon.Actions
{
    public class ShowStatusAction : IGameAction
    {
        public void Execute(GameState gameState)
        {
            string xpBar = gameState.Player.GetXPProgressBar(30);

            string statusText = MessageManager.GetMessage("status.template",
                ("name", gameState.Player.Name),
                ("level", gameState.Player.Level),
                ("experience", gameState.Player.Experience),
                ("health", gameState.Player.Health),
                ("maxHealth", gameState.Player.MaxHealth),
                ("attack", gameState.Player.Attack),
                ("defense", gameState.Player.Defense),
                ("kills", gameState.Player.Kills),
                ("gold", gameState.Player.Gold),
                ("dungeonLevel", gameState.CurrentLevel.LevelNumber),
                ("x", gameState.Player.PositionX),
                ("y", gameState.Player.PositionY),
                ("turns", gameState.TurnCount),
                ("sessionStart", gameState.CreatedAt.ToString("g")));

            // Add XP progress bar after experience
            string fullStatus = $"{MessageManager.GetMessage("status.title")}\n\n{statusText}";

            // Insert XP bar after experience line
            fullStatus = fullStatus.Replace(
                $"Experience: {gameState.Player.Experience}",
                $"Experience: {gameState.Player.Experience}\n{xpBar}");

            ScreenRenderer.DrawScreen(fullStatus);
            InputHandler.WaitForKey();
        }
    }
}
