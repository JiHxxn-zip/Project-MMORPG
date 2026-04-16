/// <summary>
/// NPC 데이터(NPCSO)를 보유하는 컴포넌트.
/// 범위 감지는 NPCRangeTrigger가 담당한다.
/// </summary>
using UnityEngine;
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class NPCController : MonoBehaviour
    {
        [SerializeField] private NPCSO _npcData;
        [SerializeField] private NPCInteractionIndicator _indicator;

        public NPCSO Data => _npcData;
        public NPCInteractionIndicator Indicator => _indicator;

        /// <summary>
        /// 현재 퀘스트 상태에 맞는 DialogueSO를 반환한다.
        /// QuestManager 연동 전까지는 항상 defaultDialogue를 반환한다.
        /// </summary>
        public DialogueSO GetDialogueForCurrentState(QuestProgressState state)
        {
            return _npcData.defaultDialogue;
        }
    }
}
