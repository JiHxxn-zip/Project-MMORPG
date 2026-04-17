using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Data
{
    /// <summary>
    /// 퀘스트 한 건의 목표·보상·대화를 정의하는 불변 데이터 컨테이너.
    /// 진행 상태(현재 킬 카운트 등)는 JSON 런타임 데이터에 저장하며 이 SO에 저장하지 않는다.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestSO", menuName = "MMORPG/Data/QuestSO")]
    public class QuestSO : ScriptableObject
    {
        /// <summary>퀘스트 고유 ID. Addressable 키 "quest/{questId}" 와 일치해야 한다.</summary>
        public string questId;

        /// <summary>퀘스트 표시 제목.</summary>
        public string title;

        /// <summary>퀘스트 설명 텍스트.</summary>
        public string description;

        /// <summary>퀘스트 목표 종류.</summary>
        public QuestType questType;

        /// <summary>목표 대상 ID (IQuestTarget.TargetId 와 대조됨).</summary>
        public string targetId;

        /// <summary>목표 달성에 필요한 수량.</summary>
        public int targetCount;

        /// <summary>선행 퀘스트 ID. 비어 있으면 선행 조건 없음.</summary>
        public string prerequisiteQuestId;

        /// <summary>완료 시 지급될 보상.</summary>
        public QuestReward reward;

        /// <summary>
        /// 퀘스트 관련 대화 전체를 담는 단일 DialogueSO.
        /// requiredState=Available → 수락 후 재생, Completed → 완료 대화, Active → TalkToNPC 목표 대화.
        /// </summary>
        public DialogueSO dialogue;
    }
}
