using System;

namespace MMORPG.Data
{
    /// <summary>
    /// 퀘스트 완료 시 지급되는 보상 데이터를 담는 값 타입.
    /// QuestSO.reward 필드에서 인라인으로 사용된다.
    /// </summary>
    [Serializable]
    public struct QuestReward
    {
        /// <summary>지급 경험치량.</summary>
        public int exp;

        /// <summary>지급 골드량.</summary>
        public int gold;

        /// <summary>지급 아이템 ID (Addressable 키 또는 테이블 ID).</summary>
        public string itemId;
    }
}
