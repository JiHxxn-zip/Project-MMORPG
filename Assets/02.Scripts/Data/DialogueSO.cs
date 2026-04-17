using System;
using System.Collections.Generic;
using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Data
{
    /// <summary>
    /// NPC 대화 흐름 전체를 정의하는 불변 데이터 컨테이너.
    /// DialogueNode 목록을 체인 방식으로 연결해 분기 대화를 표현한다.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueSO", menuName = "MMORPG/Data/DialogueSO")]
    public class DialogueSO : ScriptableObject
    {
        /// <summary>대화 고유 ID. Addressable 키 "dialogue/{dialogueId}" 와 일치해야 한다.</summary>
        public string dialogueId;

        /// <summary>순서대로 재생되는 대화 노드 목록.</summary>
        public List<DialogueNode> nodes;
    }

    /// <summary>
    /// 대화 한 줄의 내용과 분기 조건을 담는 값 타입.
    /// requiredState 조건이 현재 퀘스트 상태와 일치할 때만 해당 노드가 표시된다.
    /// </summary>
    [Serializable]
    public struct DialogueNode
    {
        /// <summary>대사를 말하는 화자 이름.</summary>
        public string speakerName;

        /// <summary>표시될 대사 내용.</summary>
        public string text;

        /// <summary>이 노드가 표시되기 위한 퀘스트 진행 상태 조건. None이면 조건 없음.</summary>
        public QuestProgressState requiredState;
    }
}
