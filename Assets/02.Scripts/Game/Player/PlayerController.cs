/// <summary>
/// 플레이어 진입점. CharacterController 래핑, FSM 소유, 중력 처리, IDamageable 구현을 담당한다.
/// 대화·퀘스트 인터랙션 로직은 PlayerInteractionHandler에 위임한다.
/// </summary>
using System;
using UnityEngine;
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    [RequireComponent(typeof(PlayerInteractionHandler))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerSO      _playerData;
        [SerializeField] private PlayerAnimator _animator;
        [SerializeField] private Transform     _cameraTransform;

        private CharacterController      _cc;
        private PlayerStateMachine       _stateMachine;
        private PlayerInteractionHandler _interactionHandler;
        private Vector3 _velocity;
        private float   _currentHp;

        // ── 프로퍼티 ──────────────────────────────────────────────────
        public PlayerStateMachine  StateMachine    => _stateMachine;
        public PlayerAnimator      Animator        => _animator;
        public PlayerSO            Data            => _playerData;
        public CharacterController CC              => _cc;
        public Transform           CameraTransform => _cameraTransform;
        public bool                IsGrounded      => _cc.isGrounded;

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

        /// <summary>NPCRangeTrigger가 PlayerRegistry를 통해 호출한다. 실제 처리는 InteractionHandler에 위임.</summary>
        public void SetCurrentNPC(NPCController npc) => _interactionHandler.SetCurrentNPC(npc);
        public void ClearCurrentNPC()                => _interactionHandler.ClearCurrentNPC();

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void OnEnable()  => PlayerRegistry.Register(this);
        private void OnDisable() => PlayerRegistry.Unregister(this);

        private void Awake()
        {
            _cc                 = GetComponent<CharacterController>();
            _interactionHandler = GetComponent<PlayerInteractionHandler>();
            _currentHp          = _playerData.maxHp;
            _stateMachine       = new PlayerStateMachine(this);
            _stateMachine.ChangeState(new PlayerIdleState(_stateMachine));
        }

        private void Update()
        {
            _stateMachine.Update();
            ApplyGravity();
        }

        // ── 중력 ──────────────────────────────────────────────────────

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
