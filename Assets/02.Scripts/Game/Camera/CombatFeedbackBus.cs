using System;

namespace MMORPG.Game
{
    public static class CombatFeedbackBus
    {
        public static event Action<CombatFeedbackEvent> OnFeedback;

        public static void Publish(CombatFeedbackEvent e) => OnFeedback?.Invoke(e);
    }
}
