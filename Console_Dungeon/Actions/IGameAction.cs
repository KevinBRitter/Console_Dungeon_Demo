using Console_Dungeon.Models;

namespace Console_Dungeon.Actions
{
    public interface IGameAction
    {
        void Execute(GameState gameState);
    }
}
