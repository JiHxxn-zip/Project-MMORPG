using System;
using UnityEngine;

namespace MMORPG.Game
{
    public static class CombatEventBus
    {
        public static event Action<DamageTakenEvent> OnDamageTaken;

        public static void Publish(DamageTakenEvent e)
        {
#if UNITY_EDITOR
            string critTag = e.IsCritical ? " [CRIT]" : "";
            Debug.Log($"[CombatBus] {e.Defender?.name} 피해 {e.FinalDamage:F1}{critTag}");
#endif
            OnDamageTaken?.Invoke(e);
        }
    }
}
