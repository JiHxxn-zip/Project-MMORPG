/// <summary>
/// NPC와의 상호작용에서 재생할 대화와 이후 실행할 퀘스트 액션을 묶는 컨텍스트.
/// NPCController.GetQuestContext()가 반환하고, PlayerController가 소비한다.
/// </summary>
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    public struct QuestContext
    {
        public DialogueSO dialogue;
        public QuestSO    quest;    // None 액션일 때는 null
        public QuestAction action;
    }
}
