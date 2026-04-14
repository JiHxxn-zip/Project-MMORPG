namespace MMORPG.Core
{
    /// <summary>
    /// 퀘스트 한 건의 진행 상태를 나타내는 Enum.
    /// QuestManager와 DialogueNode의 requiredState 필드에서 공통으로 사용한다.
    /// </summary>
    public enum QuestProgressState
    {
        /// <summary>퀘스트가 존재하지 않거나 초기화 전.</summary>
        None,

        /// <summary>수락 가능한 상태 (선행 퀘스트 완료 등 조건 충족).</summary>
        Available,

        /// <summary>수락 후 진행 중.</summary>
        Active,

        /// <summary>목표 달성 후 완료 처리됨.</summary>
        Completed,

        /// <summary>실패 조건에 도달하여 실패 처리됨.</summary>
        Failed,
    }
}
