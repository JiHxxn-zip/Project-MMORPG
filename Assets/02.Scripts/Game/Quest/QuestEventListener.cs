/// <summary>
/// 도메인 시스템(Combat, Inventory 등)의 네이티브 이벤트를 구독하여
/// GameEventPublisher를 통해 EventBus에 발행하는 어댑터.
/// 도메인 시스템은 Quest를 전혀 모른다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class QuestEventListener : MonoBehaviour
    {
        private void OnEnable()
        {
            // TODO: CombatSystem 구현 후 연결
            // CombatSystem.OnMonsterDead += HandleMonsterDead;

            // TODO: InventorySystem 구현 후 연결
            // InventorySystem.OnItemAdded += HandleItemAdded;
        }

        private void OnDisable()
        {
            // CombatSystem.OnMonsterDead -= HandleMonsterDead;
            // InventorySystem.OnItemAdded -= HandleItemAdded;
        }

        // private void HandleMonsterDead(string monsterId, int count)
        //     => GameEventPublisher.MonsterKilled(monsterId, count);

        // private void HandleItemAdded(string itemId, int count)
        //     => GameEventPublisher.ItemCollected(itemId, count);
    }
}
