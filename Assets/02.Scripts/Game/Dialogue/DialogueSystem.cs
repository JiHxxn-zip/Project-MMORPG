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

        /// <summary>대화를 시작한다. state가 None이면 첫 번째 None 노드부터, 아니면 state와 일치하는 섹션 헤더 노드부터 재생한다.</summary>
        public void StartDialogue(DialogueSO dialogue, Sprite portrait = null, QuestProgressState state = QuestProgressState.None)
        {
            CurrentPortrait   = portrait;
            _currentDialogue  = dialogue;

            int firstIndex = FindSectionStartIndex(state);
            if (firstIndex < 0) { EndDialogue(); return; }

            _currentNodeIndex = firstIndex;
            OnNodeChanged?.Invoke(_currentDialogue.nodes[_currentNodeIndex]);
        }

        /// <summary>다음 노드로 이동한다. requiredState != None인 노드는 섹션 경계로 보고 대화를 종료한다.</summary>
        public void Next()
        {
            if (!IsActive) return;

            int nextIndex = _currentNodeIndex + 1;
            if (nextIndex >= _currentDialogue.nodes.Count) { EndDialogue(); return; }

            var next = _currentDialogue.nodes[nextIndex];
            if (next.requiredState != QuestProgressState.None) { EndDialogue(); return; }

            _currentNodeIndex = nextIndex;
            OnNodeChanged?.Invoke(_currentDialogue.nodes[_currentNodeIndex]);
        }

        public void EndDialogue()
        {
            _currentDialogue = null;
            CurrentPortrait  = null;
            OnDialogueEnded?.Invoke();
        }

        /// <summary>
        /// state에 해당하는 섹션의 시작 인덱스를 반환한다. 없으면 -1.
        /// state=None이면 첫 번째 None 노드, 아니면 requiredState가 state와 정확히 일치하는 첫 노드를 찾는다.
        /// </summary>
        private int FindSectionStartIndex(QuestProgressState state)
        {
            if (_currentDialogue == null) return -1;
            for (int i = 0; i < _currentDialogue.nodes.Count; i++)
            {
                var node = _currentDialogue.nodes[i];
                if (state == QuestProgressState.None && node.requiredState == QuestProgressState.None)
                    return i;
                if (state != QuestProgressState.None && node.requiredState == state)
                    return i;
            }
            return -1;
        }
    }
}
