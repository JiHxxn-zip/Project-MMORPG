/// <summary>
/// NPC 대화를 표시하는 View 패널. IDialogueView를 구현하며 타이핑 연출을 담당한다.
/// Presenter를 통해 DialogueSystem(Model)과 통신한다.
/// </summary>
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MMORPG.Core;

namespace MMORPG.Game
{
    public class DialoguePanel : GameUIPanel, IDialogueView
    {
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Image           _portraitImage;
        [SerializeField] private float           _typingSpeed = 0.03f;

        private DialoguePresenter _presenter;
        private CancellationTokenSource _typingCts;
        private bool _isTypingComplete;
        private string _fullText;

        public override void OnOpen()
        {
            _presenter = new DialoguePresenter(this, DialogueSystem.Instance);
        }

        public override void OnClose()
        {
            _presenter?.Dispose();
            _typingCts?.Cancel();
            _typingCts = null;
        }

        // ── IDialogueView ─────────────────────────────────────────────

        public void ShowDialogue(string speakerName, string text, Sprite portrait)
        {
            _speakerNameText.text = speakerName;
            _portraitImage.sprite = portrait;
            _portraitImage.gameObject.SetActive(portrait != null);
            StartTypingAsync(text).Forget();
        }

        public void HideDialogue()
        {
            Close();
        }

        public void SetTypingComplete()
        {
            _typingCts?.Cancel();
            _isTypingComplete = true;
            _dialogueText.text = _fullText;
        }

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Update()
        {
            if (!DialogueSystem.Instance.IsActive) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                DialogueSystem.Instance.Next();
        }

        // ── 타이핑 연출 ───────────────────────────────────────────────

        private async UniTaskVoid StartTypingAsync(string fullText)
        {
            _fullText = fullText;
            _isTypingComplete = false;
            _dialogueText.text = string.Empty;

            _typingCts?.Cancel();
            _typingCts = new CancellationTokenSource();

            foreach (char c in fullText)
            {
                if (_typingCts.IsCancellationRequested) break;
                _dialogueText.text += c;
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(_typingSpeed),
                    cancellationToken: _typingCts.Token)
                    .SuppressCancellationThrow();
                if (_typingCts.IsCancellationRequested) break;
            }

            if (!_isTypingComplete)
            {
                _dialogueText.text = _fullText;
                _isTypingComplete = true;
                _presenter?.OnViewTypingCompleted();  // Model에 타이핑 자연 완료 알림
            }
        }
    }
}
