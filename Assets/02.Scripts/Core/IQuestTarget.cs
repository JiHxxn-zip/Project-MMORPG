namespace MMORPG.Core
{
    /// <summary>
    /// 퀘스트 진행 목표가 될 수 있는 오브젝트가 구현해야 하는 인터페이스.
    /// QuestManager가 킬 카운트·수집 등 진행도를 추적할 때 사용한다.
    /// </summary>
    public interface IQuestTarget
    {
        /// <summary>
        /// QuestSO의 targetId와 대조되는 고유 식별자.
        /// </summary>
        string TargetId { get; }
    }
}
