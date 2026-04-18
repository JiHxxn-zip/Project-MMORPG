// 발행은 반드시 이 클래스를 통해서만.
// 발행처 추적 및 디버그 로그 일원화.
using MMORPG.Core;

namespace MMORPG.Game
{
    public static class GameEventPublisher
    {
        public static void MonsterKilled(string monsterId, int count = 1)
            => GameEventBus.Publish(new GameEvent
               { Type = GameEventType.MonsterKilled, TargetId = monsterId, Value = count });

        public static void ItemCollected(string itemId, int count)
            => GameEventBus.Publish(new GameEvent
               { Type = GameEventType.ItemCollected, TargetId = itemId, Value = count });

        public static void NpcTalked(string npcId)
            => GameEventBus.Publish(new GameEvent
               { Type = GameEventType.NpcTalked, TargetId = npcId, Value = 1 });

        public static void ItemUsed(string itemId)
            => GameEventBus.Publish(new GameEvent
               { Type = GameEventType.ItemUsed, TargetId = itemId, Value = 1 });

        public static void QuestCompleted(string questId)
            => GameEventBus.Publish(new GameEvent
               { Type = GameEventType.QuestCompleted, TargetId = questId, Value = 1 });
    }
}
