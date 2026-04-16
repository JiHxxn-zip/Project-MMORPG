/// <summary>
/// 플레이어 진입점. CharacterController 래핑, FSM 소유, 중력 처리, IDamageable 구현을 담당한다.
/// </summary>
using System;
using UnityEngine;
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerSO _playerData;
        [SerializeField] private PlayerAnimator _animator;
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _cc;
        private PlayerStateMachine _stateMachine;
        private Vector3 _velocity;
        private float _currentHp;
        private NPCController    _currentNPC;
        private QuestContext     _pendingContext;
        private DialoguePanel    _activeDialoguePanel;
        private DialoguePresenter _dialoguePresenter;

        // ── 프로퍼티 ──────────────────────────────────────────────────
        public PlayerStateMachine StateMachine    => _stateMachine;
        public PlayerAnimator     Animator        => _animator;
        public PlayerSO           Data            => _playerData;
        public CharacterController CC             => _cc;
        public Transform          CameraTransform => _cameraTransform;
        public bool               IsGrounded      => _cc.isGrounded;

        // ── 이벤트 ────────────────────────────────────────────────────
        public event Action<float, float> OnHpChanged;  // (currentHp, maxHp)
        public event Action               OnDead;

        // ── IDamageable ───────────────────────────────────────────────
        public bool IsDead => _currentHp <= 0f;

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            _currentHp = Mathf.Max(0f, _currentHp - amount);
            OnHpChanged?.Invoke(_currentHp, _playerData.maxHp);

            // TODO(전투 Step): _currentHp <= 0 시 DeadState로 전환
        }

        // ── 외부 제어 ─────────────────────────────────────────────────

        /// <summary>점프 등 외부에서 Y velocity를 직접 제어할 때 사용한다.</summary>
        public void SetVelocityY(float y) => _velocity.y = y;

        public void SetCurrentNPC(NPCController npc) => _currentNPC = npc;
        public void ClearCurrentNPC() => _currentNPC = null;

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void OnEnable()  => PlayerRegistry.Register(this);
        private void OnDisable() => PlayerRegistry.Unregister(this);

        private void Awake()
        {
            _cc           = GetComponent<CharacterController>();
            _currentHp    = _playerData.maxHp;
            _stateMachine = new PlayerStateMachine(this);
            _stateMachine.ChangeState(new PlayerIdleState(_stateMachine));
        }

        private void Start()
        {
            DialogueSystem.Instance.OnDialogueEnded += OnDialogueEnded;
        }

        private void OnDestroy()
        {
            if (DialogueSystem.Instance != null)
                DialogueSystem.Instance.OnDialogueEnded -= OnDialogueEnded;
        }

        private void Update()
        {
            _stateMachine.Update();
            ApplyGravity();
            HandleInteractInput();
        }

        private void HandleInteractInput()
        {
            if (_stateMachine.CurrentState is PlayerInteractState) return;
            if (!Input.GetKeyDown(KeyCode.F)) return;
            if (_currentNPC == null) return;

            _pendingContext = _currentNPC.GetInteractionContext();
            if (_pendingContext.dialogue == null) return;

            _activeDialoguePanel = UIManager.Instance.OpenPanel<DialoguePanel>(UIPanelType.Dialogue, PanelOpenFlag.KeepPrevious);
            _dialoguePresenter   = new DialoguePresenter(_activeDialoguePanel, DialogueSystem.Instance);
            _activeDialoguePanel.SetPresenter(_dialoguePresenter);
            DialogueSystem.Instance.StartDialogue(_pendingContext.dialogue, _currentNPC.Data.portrait);
            _stateMachine.ChangeState(new PlayerInteractState(_stateMachine));
        }

        private void OnDialogueEnded()
        {
            if (_pendingContext.action == QuestAction.AcceptQuest && _activeDialoguePanel != null)
            {
                var quest = _pendingContext.quest;
                _pendingContext = default;

                _activeDialoguePanel.ShowQuestChoice(
                    description: quest.description,
                    onAccept: () =>
                    {
                        QuestManager.Instance.AcceptQuest(quest);
                        _activeDialoguePanel.HideQuestChoice();
                        if (quest.acceptDialogue != null)
                            DialogueSystem.Instance.StartDialogue(quest.acceptDialogue, _currentNPC?.Data.portrait);
                        else
                            FinishDialogue();
                    },
                    onDecline: () => FinishDialogue()
                );
            }
            else
            {
                if (_pendingContext.action == QuestAction.CompleteQuest)
                    QuestManager.Instance.CompleteQuest(_pendingContext.quest);
                else if (_pendingContext.action == QuestAction.TalkToNPC)
                    QuestManager.Instance.AddProgress(_pendingContext.quest.questId);

                _pendingContext = default;
                FinishDialogue();
            }
        }

        private void FinishDialogue()
        {
            _dialoguePresenter?.Dispose();
            _dialoguePresenter   = null;
            _activeDialoguePanel = null;
            UIManager.Instance.ClosePanel(UIPanelType.Dialogue);
            (_stateMachine.CurrentState as PlayerInteractState)?.EndInteract();
        }

        private void ApplyGravity()
        {
            if (_cc.isGrounded)
            {
                _velocity.y = -2f;  // 지면 고정 (isGrounded 판정 안정화)
            }
            else
            {
                _velocity.y += Physics.gravity.y * Time.deltaTime;
            }

            _cc.Move(_velocity * Time.deltaTime);
        }
    }
}
