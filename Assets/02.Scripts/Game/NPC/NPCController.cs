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
        /// 상호작용 시 재생할 대화와 퀘스트 액션을 반환한다.
        /// 우선순위: 퀘스트 완료 > 퀘스트 진행 중 > TalkToNPC 목표 > 퀘스트 수락 > 기본 대화
        /// </summary>
        public QuestContext GetInteractionContext()
        {
            if (QuestManager.Instance != null)
            {
                // 이 NPC가 퀘스트 제공자인 경우
                if (_npcData.availableQuests != null)
                {
                    foreach (var quest in _npcData.availableQuests)
                    {
                        var state = QuestManager.Instance.GetState(quest.questId);

                        // TalkToNPC 퀘스트는 타겟 NPC에서만 완료 처리한다 (아래 블록에서 담당)
                        if (state == QuestProgressState.Active &&
                            quest.questType != QuestType.TalkToNPC &&
                            QuestManager.Instance.CanComplete(quest))
                            return new QuestContext { dialogue = quest.dialogue, quest = quest, action = QuestAction.CompleteQuest, dialogueState = QuestProgressState.Completed };

                        // 진행 중이지만 아직 완료 불가 → Active 섹션 대사 (TalkToNPC 포함, 제공 NPC 방문 시)
                        if (state == QuestProgressState.Active)
                            return new QuestContext { dialogue = quest.dialogue, quest = quest, action = QuestAction.None, dialogueState = QuestProgressState.Active };

                        if (state == QuestProgressState.Available)
                            return new QuestContext { dialogue = _npcData.defaultDialogue, quest = quest, action = QuestAction.AcceptQuest, dialogueState = QuestProgressState.None };
                    }
                }

                // 이 NPC가 TalkToNPC 퀘스트의 목표인 경우
                // — 대화 후 AddProgress → CanComplete이면 CompleteQuest까지 PlayerController에서 처리
                var talkQuest = QuestManager.Instance.GetActiveTalkQuest(_npcData.npcId);
                if (talkQuest != null)
                    return new QuestContext { dialogue = _npcData.defaultDialogue, quest = talkQuest, action = QuestAction.TalkToNPC, dialogueState = QuestProgressState.None };
            }

            return new QuestContext { dialogue = _npcData.defaultDialogue, action = QuestAction.None, dialogueState = QuestProgressState.None };
        }
    }
}
