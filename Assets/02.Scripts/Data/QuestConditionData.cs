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
        /// <summary>트래커 UI에 표시할 텍스트. 예: "슬라임 처치", "마을 촌장과 대화"</summary>
        public string        label;
        [NonSerialized] public int currentCount;  // 런타임 진행도 — SO에 직렬화하지 않음
    }
}
