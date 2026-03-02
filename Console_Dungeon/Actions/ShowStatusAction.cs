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

            var p = gameState.Player;
            string weaponLine = p.EquippedWeapon != null
                ? $"{p.EquippedWeapon.Name} (+{p.EquippedWeapon.AttackBonus} ATK)"
                : "(none)";
            string armorLine = p.EquippedArmor != null
                ? $"{p.EquippedArmor.Name} (+{p.EquippedArmor.DefenseBonus} DEF)"
                : "(none)";
            string equipmentBlock = $"Equipment:\n  Weapon: {weaponLine}\n  Armor:  {armorLine}";

            string statusText = MessageManager.GetMessage("status.template",
                ("name", p.Name),
                ("level", p.Level),
                ("experience", p.Experience),
                ("health", p.Health),
                ("maxHealth", p.MaxHealth),
                ("attack", p.Attack),
                ("defense", p.Defense),
                ("kills", p.Kills),
                ("gold", p.Gold),
                ("equipment", equipmentBlock),
                ("dungeonLevel", gameState.CurrentLevel.LevelNumber),
                ("x", p.PositionX),
                ("y", p.PositionY),
                ("turns", gameState.TurnCount),
                ("sessionStart", gameState.CreatedAt.ToString("g")));

            // Add XP progress bar after experience
            string fullStatus = $"{MessageManager.GetMessage("status.title")}\n\n{statusText}";

            // Insert XP bar after the Experience | Gold line
            fullStatus = fullStatus.Replace(
                $"Experience: {p.Experience}  |  Gold: {p.Gold}",
                $"Experience: {p.Experience}  |  Gold: {p.Gold}\n{xpBar}");

            ScreenRenderer.DrawScreen(fullStatus);
            InputHandler.WaitForKey();
        }
    }
}
