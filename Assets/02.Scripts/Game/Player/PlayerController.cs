/// <summary>
/// 플레이어 진입점. CharacterController 래핑, FSM 소유, 중력 처리를 담당한다.
/// 대화·퀘스트 인터랙션 로직은 PlayerInteractionHandler에 위임한다.
/// </summary>
using UnityEngine;
using MMORPG.Data;

namespace MMORPG.Game
{
    [RequireComponent(typeof(PlayerInteractionHandler))]
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(StatHandler))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerSO      _playerData;
        [SerializeField] private PlayerAnimator _animator;
        [SerializeField] private Transform     _cameraTransform;

        private CharacterController      _cc;
        private PlayerStateMachine       _stateMachine;
        private PlayerInteractionHandler _interactionHandler;
        private Vector3 _velocity;

        // ── 프로퍼티 ──────────────────────────────────────────────────
        public PlayerStateMachine  StateMachine    => _stateMachine;
        public PlayerAnimator      Animator        => _animator;
        public PlayerSO            Data            => _playerData;
        public CharacterController CC              => _cc;
        public Transform           CameraTransform => _cameraTransform;
        public bool                IsGrounded      => _cc.isGrounded;

        // ── 외부 제어 ─────────────────────────────────────────────────

        /// <summary>점프 등 외부에서 Y velocity를 직접 제어할 때 사용한다.</summary>
        public void SetVelocityY(float y) => _velocity.y = y;

        /// <summary>NPCRangeTrigger가 PlayerRegistry를 통해 호출한다. 실제 처리는 InteractionHandler에 위임.</summary>
        public void SetCurrentNPC(NPCController npc)          => _interactionHandler.SetCurrentNPC(npc);
        public void ClearCurrentNPC()                          => _interactionHandler.ClearCurrentNPC();
        public bool IsCurrentNPC(NPCController npc)            => _interactionHandler.IsCurrentNPC(npc);
        public void StartInteraction(NPCController npc)        => _interactionHandler.StartInteraction(npc);

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void OnEnable()
        {
            PlayerRegistry.Register(this);
            TargetingSystem.Instance.OnTargetChanged += OnTargetChanged;
        }

        private void OnDisable()
        {
            PlayerRegistry.Unregister(this);
            TargetingSystem.Instance.OnTargetChanged -= OnTargetChanged;
        }

        private void OnTargetChanged(Character target)
        {
            if (target == null) return;

            Vector3 dir = (target.transform.position - transform.position);
            dir.y = 0f;
            if (dir == Vector3.zero) return;

            if (_lookRoutine != null) StopCoroutine(_lookRoutine);
            _lookRoutine = StartCoroutine(LookAtRoutine(Quaternion.LookRotation(dir)));
        }

        private Coroutine _lookRoutine;

        private System.Collections.IEnumerator LookAtRoutine(Quaternion targetRot)
        {
            while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRot,
                    _playerData.rotationSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = targetRot;
            _lookRoutine = null;
        }

        private void Awake()
        {
            _cc                 = GetComponent<CharacterController>();
            _interactionHandler = GetComponent<PlayerInteractionHandler>();
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
