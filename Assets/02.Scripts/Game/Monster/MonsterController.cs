using UnityEngine;
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(StatHandler))]
    public class MonsterController : MonoBehaviour, IInteractable
    {
        [SerializeField] private MonsterSO _data;

        private Character           _character;
        private StatHandler         _statHandler;
        private DamagePipeline      _pipeline;
        private MonsterStateMachine _stateMachine;

        // ── FSM이 읽는 프로퍼티 ───────────────────────────────────────
        public MonsterSO  Data            => _data;
        public Transform  PlayerTransform { get; private set; }

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Awake()
        {
            _character    = GetComponent<Character>();
            _statHandler  = GetComponent<StatHandler>();
            _pipeline     = new DamagePipeline();
            _stateMachine = new MonsterStateMachine(this);

            _statHandler.OnDead += OnDead;
            _statHandler.OnHit  += OnHit;
        }

        private void Start()
        {
            _stateMachine.ChangeState(new MonsterIdleState(_stateMachine));
        }

        private void Update()
        {
            PlayerTransform = PlayerRegistry.Player != null
                ? PlayerRegistry.Player.transform
                : null;

            _stateMachine.Update();
        }

        private void OnDestroy()
        {
            _statHandler.OnDead -= OnDead;
            _statHandler.OnHit  -= OnHit;
        }

        // ── 공격 (AttackState에서 호출) ───────────────────────────────

        public void PerformAttack()
        {
            if (PlayerTransform == null) return;
            var playerCharacter = PlayerTransform.GetComponent<Character>();
            if (playerCharacter == null) return;

            _pipeline.Execute(_character, playerCharacter, _data.stat.attackPower, DamageType.Physical);
        }

        // ── 유틸 ─────────────────────────────────────────────────────

        public float DistToPlayer() =>
            PlayerTransform == null
                ? float.MaxValue
                : Vector3.Distance(transform.position, PlayerTransform.position);

        // ── 이벤트 핸들러 ─────────────────────────────────────────────

        private void OnDead()
        {
            _statHandler.OnDead -= OnDead;
            _statHandler.OnHit  -= OnHit;

            GameEventBus.Publish(new GameEvent
            {
                Type     = GameEventType.MonsterKilled,
                TargetId = _data.monsterId,
                Value    = 1
            });

            _stateMachine.ChangeState(new MonsterDeadState(_stateMachine));
            Destroy(gameObject, 2f);
        }

        private void OnHit()
        {
            // 이미 넉백/사망 중이면 무시
            if (_stateMachine.CurrentState is MonsterKnockbackState) return;
            if (_stateMachine.CurrentState is MonsterDeadState)       return;

            Vector3 knockDir = PlayerTransform != null
                ? (transform.position - PlayerTransform.position).normalized
                : Vector3.back;

            _stateMachine.ChangeState(new MonsterKnockbackState(_stateMachine, knockDir, force: 3f));
        }

        // ── IInteractable ─────────────────────────────────────────────

        public void OnInteract()
        {
            if (_stateMachine.CurrentState is MonsterDeadState) return;

            TargetingSystem.Instance.SetTarget(_character);
        }
    }
}
