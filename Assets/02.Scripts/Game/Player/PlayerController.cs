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
        private NPCController _currentNPC;

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

            var dialogue = _currentNPC.GetDialogueForCurrentState(QuestProgressState.None);
            if (dialogue == null) return;

            UIManager.Instance.OpenPanel<DialoguePanel>(UIPanelType.Dialogue, PanelOpenFlag.KeepPrevious);
            DialogueSystem.Instance.StartDialogue(dialogue, _currentNPC.Data.portrait);
            _stateMachine.ChangeState(new PlayerInteractState(_stateMachine));
        }

        private void OnDialogueEnded()
        {
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
