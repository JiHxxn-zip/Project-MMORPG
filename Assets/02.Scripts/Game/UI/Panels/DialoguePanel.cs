/// <summary>
/// NPC 대화를 표시하는 View 패널. IDialogueView를 구현하며 타이핑 연출을 담당한다.
/// Presenter를 통해 DialogueSystem(Model)과 통신한다.
/// </summary>
using System;
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
        [Header("Dialogue")]
        [SerializeField] private GameObject      _dialogueContentRoot;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Image           _portraitImage;
        [SerializeField] private float           _typingSpeed = 0.03f;

        [Header("Quest Choice")]
        [SerializeField] private GameObject _questChoiceRoot;
        [SerializeField] private Button     _acceptButton;
        [SerializeField] private Button     _declineButton;

        private DialoguePresenter        _presenter;
        private CancellationTokenSource  _typingCts;
        private string                   _fullText;

        // ── IDialogueView ─────────────────────────────────────────────

        public bool IsTyping { get; private set; }

        public void ShowDialogue(string speakerName, string text, Sprite portrait)
        {
            _speakerNameText.text = speakerName;
            _portraitImage.sprite = portrait;
            _portraitImage.gameObject.SetActive(portrait != null);
            StartTypingAsync(text).Forget();
        }

        public void HideDialogue()
        {
            _typingCts?.Cancel();
            IsTyping = false;
        }

        public void SkipTyping()
        {
            _typingCts?.Cancel();
            _dialogueText.text = _fullText;
            IsTyping = false;
        }

        // ── Quest Choice ──────────────────────────────────────────────

        /// <summary>대화 종료 후 퀘스트 수락/거절 버튼을 표시한다. PlayerController가 호출한다.</summary>
        public void HideQuestChoice() => _questChoiceRoot.SetActive(false);

        public void ShowQuestChoice(string description, Action onAccept, Action onDecline)
        {
            _dialogueText.text = description;
            _questChoiceRoot.SetActive(true);

            _acceptButton.onClick.RemoveAllListeners();
            _declineButton.onClick.RemoveAllListeners();
            _acceptButton.onClick.AddListener(() => onAccept?.Invoke());
            _declineButton.onClick.AddListener(() => onDecline?.Invoke());
        }


        // ── Unity 생명주기 ────────────────────────────────────────────

        public void SetPresenter(DialoguePresenter presenter)
        {
            _presenter = presenter;
        }

        public override void OnOpen()
        {
            _questChoiceRoot.SetActive(false);
            _dialogueContentRoot.SetActive(true);
        }

        public override void OnClose()
        {
            _typingCts?.Cancel();
            _typingCts = null;
            IsTyping   = false;
            _questChoiceRoot.SetActive(false);
            _presenter = null;
        }

        private void Update()
        {
            if (_presenter == null) return;
            if (_questChoiceRoot.activeSelf) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                _presenter.OnNextRequested();
        }

        // ── 타이핑 연출 ───────────────────────────────────────────────

        private async UniTaskVoid StartTypingAsync(string fullText)
        {
            _fullText = fullText;
            IsTyping  = true;
            _dialogueText.text = string.Empty;

            _typingCts?.Cancel();
            _typingCts = new CancellationTokenSource();

            foreach (char c in fullText)
            {
                if (_typingCts.IsCancellationRequested) break;
                _dialogueText.text += c;
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_typingSpeed),
                    cancellationToken: _typingCts.Token)
                    .SuppressCancellationThrow();
            }

            // 자연 완료 또는 스킵 완료 — 어느 경우든 전체 텍스트 보장
            if (!_typingCts.IsCancellationRequested)
            {
                _dialogueText.text = _fullText;
                IsTyping = false;
            }
        }
    }
}
