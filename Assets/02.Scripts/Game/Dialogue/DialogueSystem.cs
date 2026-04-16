/// <summary>
/// 대화 흐름을 제어하는 Model 싱글턴.
/// DialogueSO의 노드를 순서대로 재생하고, 종료 이벤트를 발행한다.
/// 타이핑 연출은 View의 책임이므로 이 클래스는 타이핑 상태를 알지 못한다.
/// </summary>
using System;
using MMORPG.Core;
using MMORPG.Data;
using UnityEngine;

namespace MMORPG.Game
{
    public class DialogueSystem : SingletonManager<DialogueSystem>
    {
        private DialogueSO _currentDialogue;
        private int _currentNodeIndex;

        /// <summary>현재 NPC의 초상화. Presenter가 HandleNodeChanged에서 읽는다.</summary>
        public Sprite CurrentPortrait { get; private set; }

        public bool IsActive => _currentDialogue != null;

        public event Action<DialogueNode> OnNodeChanged;
        public event Action               OnDialogueEnded;

        /// <summary>대화를 시작한다.</summary>
        public void StartDialogue(DialogueSO dialogue, Sprite portrait = null, QuestProgressState state = QuestProgressState.None)
        {
            CurrentPortrait   = portrait;
            _currentDialogue  = dialogue;

            int firstIndex = FindValidNodeIndex(0, state);
            if (firstIndex < 0) { EndDialogue(); return; }

            _currentNodeIndex = firstIndex;
            OnNodeChanged?.Invoke(_currentDialogue.nodes[_currentNodeIndex]);
        }

        /// <summary>다음 유효 노드로 이동한다. 없으면 대화를 종료한다.</summary>
        public void Next()
        {
            if (!IsActive) return;

            int nextIndex = FindValidNodeIndex(_currentNodeIndex + 1, QuestProgressState.None);
            if (nextIndex < 0) { EndDialogue(); return; }

            _currentNodeIndex = nextIndex;
            OnNodeChanged?.Invoke(_currentDialogue.nodes[_currentNodeIndex]);
        }

        public void EndDialogue()
        {
            _currentDialogue = null;
            CurrentPortrait  = null;
            OnDialogueEnded?.Invoke();
        }

        /// <summary>startIndex부터 조건에 맞는 첫 번째 노드 인덱스를 반환한다. 없으면 -1.</summary>
        private int FindValidNodeIndex(int startIndex, QuestProgressState state)
        {
            if (_currentDialogue == null) return -1;
            for (int i = startIndex; i < _currentDialogue.nodes.Count; i++)
            {
                var node = _currentDialogue.nodes[i];
                if (node.requiredState == QuestProgressState.None || node.requiredState == state)
                    return i;
            }
            return -1;
        }
    }
}
