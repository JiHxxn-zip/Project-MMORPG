/// <summary>
/// 대화 흐름을 제어하는 Model 싱글턴.
/// DialogueSO의 노드를 순서대로 재생하고, 타이핑 상태 및 종료 이벤트를 발행한다.
/// View/Presenter를 직접 참조하지 않는다.
/// </summary>
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MMORPG.Core;
using MMORPG.Data;
using UnityEngine;

namespace MMORPG.Game
{
    public class DialogueSystem : SingletonManager<DialogueSystem>
    {
        private DialogueSO _currentDialogue;
        private int _currentNodeIndex;
        private bool _isTyping;
        private CancellationTokenSource _cts;

        /// <summary>현재 NPC의 초상화. Presenter가 HandleNodeChanged에서 읽는다.</summary>
        public Sprite CurrentPortrait { get; private set; }

        public bool IsActive => _currentDialogue != null;

        public event Action<DialogueNode> OnNodeChanged;
        public event Action OnTypingComplete;
        public event Action OnDialogueEnded;

        /// <summary>대화를 시작한다. portrait는 각 노드 표시 시 Presenter에 전달된다.</summary>
        public void StartDialogue(DialogueSO dialogue, Sprite portrait = null, QuestProgressState state = QuestProgressState.None)
        {
            CurrentPortrait = portrait;
            _currentDialogue = dialogue;

            int firstIndex = FindValidNodeIndex(0, state);
            if (firstIndex < 0) { EndDialogue(); return; }

            _currentNodeIndex = firstIndex;
            ShowCurrentNode();
        }

        /// <summary>
        /// 타이핑 중 → 스킵(전체 텍스트 표시).
        /// 타이핑 완료 → 다음 유효 노드로 이동, 없으면 대화 종료.
        /// </summary>
        public void Next()
        {
            if (!IsActive) return;

            if (_isTyping)
            {
                _isTyping = false;  // WaitUntil 해제 → OnTypingComplete 발행
                return;
            }

            int nextIndex = FindValidNodeIndex(_currentNodeIndex + 1, QuestProgressState.None);
            if (nextIndex < 0) { EndDialogue(); return; }

            _currentNodeIndex = nextIndex;
            ShowCurrentNode();
        }

        public void EndDialogue()
        {
            _cts?.Cancel();
            _isTyping = false;
            _currentDialogue = null;
            CurrentPortrait = null;
            OnDialogueEnded?.Invoke();
        }

        /// <summary>View의 타이핑 애니메이션이 자연 완료되면 Presenter가 호출한다.</summary>
        public void NotifyTypingFinished()
        {
            _isTyping = false;
        }

        private void ShowCurrentNode()
        {
            StartTypingAsync(_currentDialogue.nodes[_currentNodeIndex]).Forget();
        }

        private async UniTaskVoid StartTypingAsync(DialogueNode node)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _isTyping = true;

            OnNodeChanged?.Invoke(node);  // Presenter → View 타이핑 연출 시작

            // _isTyping이 false가 될 때까지 대기
            // → Next() 스킵 호출 또는 View 자연 완료(NotifyTypingFinished) 시 해제
            await UniTask.WaitUntil(() => !_isTyping, cancellationToken: _cts.Token)
                         .SuppressCancellationThrow();

            OnTypingComplete?.Invoke();
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
