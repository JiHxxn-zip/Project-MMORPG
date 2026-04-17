using System.Collections.Generic;
using UnityEngine;

namespace MMORPG.Data
{
    /// <summary>
    /// NPC 한 명의 식별 정보, 기본 대화, 제공 퀘스트 목록을 정의하는 불변 데이터 컨테이너.
    /// </summary>
    [CreateAssetMenu(fileName = "NPCSO", menuName = "MMORPG/Data/NPCSO")]
    public class NPCSO : ScriptableObject
    {
        /// <summary>NPC 고유 ID. Addressable 키 "npc/{npcId}" 와 일치해야 한다.</summary>
        public string npcId;

        /// <summary>NPC 표시 이름.</summary>
        public string npcName;

        /// <summary>NPC 초상화 이미지.</summary>
        public Sprite portrait;

        /// <summary>퀘스트 조건과 무관하게 항상 재생되는 기본 대화.</summary>
        public DialogueSO defaultDialogue;

        /// <summary>이 NPC가 제공하는 퀘스트 목록.</summary>
        public List<QuestSO> availableQuests;
    }
}
