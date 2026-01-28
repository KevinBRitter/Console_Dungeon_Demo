using Console_Dungeon.Input;
using Console_Dungeon.Managers;
using Console_Dungeon.Models;
using Console_Dungeon.UI;

namespace Console_Dungeon.Actions
{
    public class ShowLevelUpAction : IGameAction
    {
        private readonly LevelUpInfo _levelUpInfo;

        public ShowLevelUpAction(LevelUpInfo levelUpInfo)
        {
            _levelUpInfo = levelUpInfo;
        }

        public void Execute(GameState gameState)
        {
            // Build the level-up message
            string title = MessageManager.GetMessage("levelUp.title");
            string congrats = MessageManager.GetMessage("levelUp.congratulations",
                ("level", _levelUpInfo.NewLevel));
            string statsHeader = MessageManager.GetMessage("levelUp.statsIncreased");

            // Build stat gains
            string statGains = MessageManager.GetMessage("levelUp.statGains",
                ("healthGain", _levelUpInfo.HealthGain),
                ("maxHealth", _levelUpInfo.NewMaxHealth),
                ("attackGain", _levelUpInfo.AttackGain),
                ("attack", _levelUpInfo.NewAttack),
                ("defenseGain", _levelUpInfo.DefenseGain),
                ("defense", _levelUpInfo.NewDefense));

            string message = $"{title}\n\n{congrats}\n\n{statsHeader}\n\n{statGains}";

            // Add optional stats if they increased
            if (_levelUpInfo.ManaGain > 0)
            {
                string manaGain = MessageManager.GetMessage("levelUp.manaGain",
                    ("manaGain", _levelUpInfo.ManaGain),
                    ("maxMana", _levelUpInfo.NewMaxMana));
                message += $"\n{manaGain}";
            }

            if (_levelUpInfo.StaminaGain > 0)
            {
                string staminaGain = MessageManager.GetMessage("levelUp.staminaGain",
                    ("staminaGain", _levelUpInfo.StaminaGain),
                    ("maxStamina", _levelUpInfo.NewMaxStamina));
                message += $"\n{staminaGain}";
            }

            // Add restoration message
            string restored = MessageManager.GetMessage("levelUp.restored");
            string continuePrompt = MessageManager.GetMessage("levelUp.continue");

            message += $"{restored}{continuePrompt}";

            // Display
            ScreenRenderer.DrawScreen(message);
            InputHandler.WaitForKey();
        }
    }
}
