using Console_Dungeon.Input;
using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon.Actions
{
    public class RestAction : IGameAction
    {
        public void Execute(GameState gameState)
        {
            int healAmount = MessageManager.GetValue<int>("rest.healAmount");
            gameState.Player.Heal(healAmount);

            string message = MessageManager.GetMessage("rest.message",
                ("amount", healAmount),
                ("current", gameState.Player.Health),
                ("max", gameState.Player.MaxHealth));

            ScreenRenderer.DrawScreen(message);
            InputHandler.WaitForKey();
        }
    }
}
