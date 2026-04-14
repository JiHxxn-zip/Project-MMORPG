namespace MMORPG.Core
{
    /// <summary>
    /// 퀘스트 목표의 종류를 나타내는 Enum.
    /// QuestSO.questType 필드에서 사용하며, QuestManager가 진행도 집계 방식을 결정하는 데 쓴다.
    /// </summary>
    public enum QuestType
    {
        /// <summary>특정 몬스터를 지정 수만큼 처치.</summary>
        KillMonster,

        /// <summary>특정 NPC에게 대화.</summary>
        TalkToNPC,

        /// <summary>특정 아이템을 지정 수량만큼 수집.</summary>
        CollectItem,

        /// <summary>특정 위치에 도달.</summary>
        Reach,
    }
}
