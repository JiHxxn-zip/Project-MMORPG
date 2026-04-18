using System;
using MMORPG.Core;

namespace MMORPG.Data
{
    [Serializable]
    public class QuestConditionData
    {
        public GameEventType eventType;    // enum으로 타입 안전 보장
        public string        targetId;
        public int           requiredCount;
        [NonSerialized] public int currentCount;  // 런타임 진행도 — SO에 직렬화하지 않음
    }
}
