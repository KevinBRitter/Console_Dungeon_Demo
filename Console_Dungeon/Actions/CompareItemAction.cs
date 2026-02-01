using Console_Dungeon.Enums;
using Console_Dungeon.Input;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon.Actions
{
    public class CompareItemAction : IGameAction
    {
        private readonly Item _newItem;

        public CompareItemAction(Item newItem)
        {
            _newItem = newItem;
        }

        public void Execute(GameState gameState)
        {
            var player = gameState.Player;

            // Get the currently equipped item in this slot (if any)
            Item? currentItem = _newItem.Slot switch
            {
                EquipmentSlot.Weapon => player.EquippedWeapon,
                EquipmentSlot.Armor => player.EquippedArmor,
                EquipmentSlot.Jewelry => player.EquippedJewelry,
                _ => null
            };

            // Build comparison screen
            string slotName = _newItem.Slot.ToString();
            string message = $"=== {slotName} Found ===\n\n";
            message += $"You found: {_newItem.Name}\n";
            message += $"{_newItem.Description}\n\n";

            // Show new item stats
            message += "--- NEW ITEM ---\n";
            message += FormatItemStats(_newItem);
            message += "\n";

            // Show current item stats (if equipped)
            if (currentItem != null)
            {
                message += "--- CURRENTLY EQUIPPED ---\n";
                message += FormatItemStats(currentItem);
                message += "\n";
            }
            else
            {
                message += $"--- CURRENTLY EQUIPPED ---\n";
                message += $"(No {slotName.ToLower()} equipped)\n\n";
            }

            // Show choices
            message += "\nWhat would you like to do?\n";
            message += "1. Equip new item" + (currentItem != null ? " (replaces current)" : "") + "\n";

            bool inventoryFull = player.Inventory.Count >= Player.MaxInventorySlots;
            if (!inventoryFull)
            {
                message += "2. Add to inventory\n";
                message += "3. Leave it behind\n";
            }
            else
            {
                message += "2. Leave it behind (inventory full)\n";
            }

            ScreenRenderer.DrawScreen(message);
            string choice = InputHandler.GetMenuChoice();

            // Handle player choice
            switch (choice)
            {
                case "1":
                    // Equip new item
                    EquipNewItem(player, _newItem, currentItem);
                    ShowEquipMessage(_newItem, currentItem);
                    break;

                case "2":
                    if (!inventoryFull)
                    {
                        // Add to inventory
                        player.Inventory.Add(_newItem);
                        ScreenRenderer.DrawScreen($"You added {_newItem.Name} to your inventory.");
                        InputHandler.WaitForKey();
                    }
                    else
                    {
                        // Leave it behind
                        ScreenRenderer.DrawScreen($"You leave the {_newItem.Name} behind.");
                        InputHandler.WaitForKey();
                    }
                    break;

                case "3":
                    if (!inventoryFull)
                    {
                        // Leave it behind
                        ScreenRenderer.DrawScreen($"You leave the {_newItem.Name} behind.");
                        InputHandler.WaitForKey();
                    }
                    break;

                default:
                    // Invalid choice, leave it behind
                    ScreenRenderer.DrawScreen($"You leave the {_newItem.Name} behind.");
                    InputHandler.WaitForKey();
                    break;
            }
        }

        private void EquipNewItem(Player player, Item newItem, Item? oldItem)
        {
            switch (newItem.Slot)
            {
                case EquipmentSlot.Weapon:
                    player.EquippedWeapon = newItem;
                    if (oldItem != null && player.Inventory.Count < Player.MaxInventorySlots)
                    {
                        player.Inventory.Add(oldItem);
                    }
                    break;

                case EquipmentSlot.Armor:
                    player.EquippedArmor = newItem;
                    if (oldItem != null && player.Inventory.Count < Player.MaxInventorySlots)
                    {
                        player.Inventory.Add(oldItem);
                    }
                    break;

                case EquipmentSlot.Jewelry:
                    player.EquippedJewelry = newItem;
                    if (oldItem != null && player.Inventory.Count < Player.MaxInventorySlots)
                    {
                        player.Inventory.Add(oldItem);
                    }
                    break;
            }
        }

        private void ShowEquipMessage(Item newItem, Item? oldItem)
        {
            string message = $"You equipped {newItem.Name}";

            if (oldItem != null)
            {
                message += $" and moved {oldItem.Name} to your inventory";
            }

            message += ".\n\nPress any key to continue...";

            ScreenRenderer.DrawScreen(message);
            InputHandler.WaitForKey();
        }

        private string FormatItemStats(Item item)
        {
            string stats = $"{item.Name}\n";

            if (item.AttackBonus != 0)
            {
                string sign = item.AttackBonus > 0 ? "+" : "";
                stats += $"  Attack: {sign}{item.AttackBonus}\n";
            }

            if (item.DefenseBonus != 0)
            {
                string sign = item.DefenseBonus > 0 ? "+" : "";
                stats += $"  Defense: {sign}{item.DefenseBonus}\n";
            }

            if (item.HealAmount != 0)
            {
                stats += $"  Healing: +{item.HealAmount}\n";
            }

            if (item.IsMeta)
            {
                stats += $"  [META ITEM - Unlocked Forever]\n";
            }

            return stats;
        }
    }
}
