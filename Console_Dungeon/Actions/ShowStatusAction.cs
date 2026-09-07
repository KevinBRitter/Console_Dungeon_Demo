using System.Text;
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
            var p = gameState.Player;

            string statusText = MessageManager.GetMessage("status.template",
                ("name", p.Name),
                ("level", p.Level),
                ("experience", p.Experience),
                ("xpBar", p.GetXPProgressBar(30)),
                ("health", p.Health),
                ("maxHealth", p.MaxHealth),
                ("attack", FormatStat(p.Attack, p.GetEffectiveAttack())),
                ("defense", FormatStat(p.Defense, p.GetEffectiveDefense())),
                ("kills", p.Kills),
                ("gold", p.Gold),
                ("equipment", BuildEquipmentBlock(p)),
                ("dungeonLevel", gameState.CurrentLevel.LevelNumber),
                ("x", p.PositionX),
                ("y", p.PositionY),
                ("turns", gameState.TurnCount),
                ("sessionStart", gameState.CreatedAt.ToString("g")));

            string fullStatus = $"{MessageManager.GetMessage("status.title")}\n\n{statusText}";

            ScreenRenderer.DrawScreen(fullStatus);
            InputHandler.WaitForKey();
        }

        // Show what the player actually fights with, with the base value in parentheses
        // whenever equipment is contributing. Base-only stats render as a plain number.
        private static string FormatStat(int baseValue, int effectiveValue)
        {
            int bonus = effectiveValue - baseValue;
            return bonus == 0
                ? effectiveValue.ToString()
                : $"{effectiveValue} ({baseValue} {bonus:+#;-#})";
        }

        private static string BuildEquipmentBlock(Player p)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Equipment:");
            sb.AppendLine($"  Weapon:  {DescribeSlot(p.EquippedWeapon)}");
            sb.AppendLine($"  Armor:   {DescribeSlot(p.EquippedArmor)}");

            // Only costs a line when something is actually equipped - the status screen
            // is close to the renderer's 20-row practical ceiling.
            if (p.EquippedJewelry != null)
            {
                sb.Append($"  Jewelry: {DescribeSlot(p.EquippedJewelry)}");
            }

            return sb.ToString().TrimEnd();
        }

        private static string DescribeSlot(Item? item)
        {
            if (item == null) return "(none)";

            var bonuses = new List<string>();
            if (item.AttackBonus != 0) bonuses.Add($"+{item.AttackBonus} ATK");
            if (item.DefenseBonus != 0) bonuses.Add($"+{item.DefenseBonus} DEF");

            return bonuses.Count > 0
                ? $"{item.Name} ({string.Join(", ", bonuses)})"
                : item.Name;
        }
    }
}
