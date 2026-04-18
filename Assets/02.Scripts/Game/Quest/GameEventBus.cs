using System;
using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Game
{
    public static class GameEventBus
    {
        public static event Action<GameEvent> OnEvent;

        public static void Publish(GameEvent e)
        {
#if UNITY_EDITOR
            Debug.Log($"[EventBus] {e.Type} | {e.TargetId} | {e.Value}");
#endif
            OnEvent?.Invoke(e);
        }
    }
}
