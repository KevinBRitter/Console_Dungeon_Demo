using System.Text;
using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon.Actions
{
    public class ShowInventoryAction : IGameAction
    {
        public void Execute(GameState gameState)
        {
            while (true)
            {
                string screen = BuildInventoryScreen(gameState.Player);
                ScreenRenderer.DrawScreen(screen);
                string input = InputHandler.GetMenuChoice();

                if (input == "0")
                    return;

                if (int.TryParse(input, out int idx) &&
                    idx >= 1 && idx <= gameState.Player.Inventory.Count)
                {
                    Item item = gameState.Player.Inventory[idx - 1];
                    ShowItemMenu(gameState, item);
                }
                else
                {
                    ScreenRenderer.DrawScreen("Invalid choice.\n\nPress any key to continue...");
                    InputHandler.WaitForKey();
                }
            }
        }

        // ── List screen ────────────────────────────────────────────────────────

        private string BuildInventoryScreen(Player player)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Inventory ===");
            sb.AppendLine();

            // Equipped items header
            sb.AppendLine("Equipped:");
            string weaponLine = player.EquippedWeapon != null
                ? $"{player.EquippedWeapon.Name} (+{player.EquippedWeapon.AttackBonus} ATK)"
                : "(none)";
            string armorLine = player.EquippedArmor != null
                ? $"{player.EquippedArmor.Name} (+{player.EquippedArmor.DefenseBonus} DEF)"
                : "(none)";
            sb.AppendLine($"  Weapon: {weaponLine}");
            sb.AppendLine($"  Armor:  {armorLine}");
            sb.AppendLine();

            // Item list
            if (player.Inventory.Count == 0)
            {
                sb.AppendLine("Your inventory is empty.");
                sb.AppendLine();
                sb.Append("[0] Go back.");
            }
            else
            {
                sb.AppendLine($"Items ({player.Inventory.Count}/{Player.MaxInventorySlots}):");
                for (int i = 0; i < player.Inventory.Count; i++)
                {
                    Item item = player.Inventory[i];
                    string summary = FormatStatSummary(item);
                    sb.AppendLine($"  [{i + 1}] {item.Name,-26} {summary}");
                }
                sb.AppendLine();
                sb.Append($"Select an item [1-{player.Inventory.Count}] or [0] to go back.");
            }

            return sb.ToString();
        }

        // ── Item detail / inspect screen ───────────────────────────────────────

        private void ShowItemMenu(GameState gameState, Item item)
        {
            while (true)
            {
                string screen = BuildItemScreen(gameState.Player, item);
                ScreenRenderer.DrawScreen(screen);
                string input = InputHandler.GetMenuChoice();

                if (item.Type == ItemType.Consumable)
                {
                    switch (input)
                    {
                        case "1":
                            HandleUse(gameState, item);
                            return;
                        case "2":
                            HandleDrop(gameState, item);
                            return;
                        case "3":
                            return;
                        default:
                            ScreenRenderer.DrawScreen("Invalid choice.\n\nPress any key to continue...");
                            InputHandler.WaitForKey();
                            break;
                    }
                }
                else
                {
                    switch (input)
                    {
                        case "1":
                            HandleEquip(gameState, item);
                            return;
                        case "2":
                            HandleDrop(gameState, item);
                            return;
                        case "3":
                            return;
                        default:
                            ScreenRenderer.DrawScreen("Invalid choice.\n\nPress any key to continue...");
                            InputHandler.WaitForKey();
                            break;
                    }
                }
            }
        }

        private string BuildItemScreen(Player player, Item item)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {item.Name} ===");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                sb.AppendLine(item.Description);
                sb.AppendLine();
            }

            sb.AppendLine($"Type: {item.Type}");

            if (item.AttackBonus != 0)
                sb.AppendLine($"Attack Bonus: +{item.AttackBonus}");
            if (item.DefenseBonus != 0)
                sb.AppendLine($"Defense Bonus: +{item.DefenseBonus}");
            if (item.HealAmount != 0)
                sb.AppendLine($"Heals: {item.HealAmount} HP");
            if (item.IsMeta)
                sb.AppendLine("[META ITEM - Unlocked Forever]");

            if (item.Type == ItemType.Equipment)
            {
                Item? equipped = item.Slot switch
                {
                    EquipmentSlot.Weapon => player.EquippedWeapon,
                    EquipmentSlot.Armor => player.EquippedArmor,
                    EquipmentSlot.Jewelry => player.EquippedJewelry,
                    _ => null
                };

                sb.AppendLine();
                if (equipped != null)
                    sb.AppendLine($"Currently Equipped: {equipped.Name} — will move to inventory");
                else
                    sb.AppendLine($"Slot: {item.Slot} (empty)");

                sb.AppendLine();
                string equipLabel = equipped != null
                    ? $"[1] Equip (replaces {equipped.Name})"
                    : "[1] Equip";
                sb.AppendLine($"  {equipLabel}");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("  [1] Use");
            }

            sb.AppendLine("  [2] Drop");
            sb.Append("  [3] Back");

            return sb.ToString();
        }

        // ── Actions ────────────────────────────────────────────────────────────

        private void HandleUse(GameState gameState, Item item)
        {
            int healAmount = item.HealAmount;
            bool used = gameState.Player.UseConsumable(item.Id);
            if (used)
            {
                string msg = $"You use the {item.Name}.\n" +
                             $"Restored {healAmount} HP.\n" +
                             $"Current HP: {gameState.Player.Health}/{gameState.Player.MaxHealth}\n\n" +
                             $"Press any key to continue...";
                ScreenRenderer.DrawScreen(msg);
                InputHandler.WaitForKey();
            }
        }

        private void HandleDrop(GameState gameState, Item item)
        {
            string confirmMsg = $"Drop {item.Name}?\n" +
                                $"This cannot be undone.\n\n" +
                                $"  [1] Yes, drop it\n" +
                                $"  [2] No, go back\n\n";
            ScreenRenderer.DrawScreen(confirmMsg);
            string input = InputHandler.GetMenuChoice();

            if (input == "1")
            {
                gameState.Player.Inventory.Remove(item);
                ScreenRenderer.DrawScreen($"You dropped the {item.Name}.\n\nPress any key to continue...");
                InputHandler.WaitForKey();
            }
        }

        private void HandleEquip(GameState gameState, Item item)
        {
            var player = gameState.Player;

            Item? currentItem = item.Slot switch
            {
                EquipmentSlot.Weapon => player.EquippedWeapon,
                EquipmentSlot.Armor => player.EquippedArmor,
                EquipmentSlot.Jewelry => player.EquippedJewelry,
                _ => null
            };

            string confirmMsg = currentItem != null
                ? $"Equip {item.Name}?\n\n" +
                  $"{currentItem.Name} will be moved to your inventory.\n\n" +
                  $"  [1] Yes, equip it\n" +
                  $"  [2] No, go back\n\n"
                : $"Equip {item.Name}?\n\n" +
                  $"  [1] Yes, equip it\n" +
                  $"  [2] No, go back\n\n";

            ScreenRenderer.DrawScreen(confirmMsg);
            string input = InputHandler.GetMenuChoice();

            if (input == "1")
            {
                bool equipped = player.EquipFromInventory(item.Id);
                if (equipped)
                {
                    string resultMsg = currentItem != null
                        ? $"You equipped {item.Name} and moved {currentItem.Name} to your inventory.\n\nPress any key to continue..."
                        : $"You equipped {item.Name}.\n\nPress any key to continue...";
                    ScreenRenderer.DrawScreen(resultMsg);
                    InputHandler.WaitForKey();
                }
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private string FormatStatSummary(Item item)
        {
            if (item.AttackBonus != 0) return $"+{item.AttackBonus} ATK";
            if (item.DefenseBonus != 0) return $"+{item.DefenseBonus} DEF";
            if (item.HealAmount != 0) return $"Heals {item.HealAmount} HP";
            return string.Empty;
        }
    }
}
