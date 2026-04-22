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

        private enum State { Idle, Chase, Attack, Dead }

        private Character      _character;
        private StatHandler    _statHandler;
        private DamagePipeline _pipeline;

        private State     _state = State.Idle;
        private float     _attackTimer;
        private Transform _playerTransform;

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Awake()
        {
            _character   = GetComponent<Character>();
            _statHandler = GetComponent<StatHandler>();
            _pipeline    = new DamagePipeline();

            _statHandler.OnDead += OnDead;
        }

        private void Update()
        {
            if (_state == State.Dead) return;

            _playerTransform = GetPlayerTransform();

            switch (_state)
            {
                case State.Idle:   UpdateIdle();   break;
                case State.Chase:  UpdateChase();  break;
                case State.Attack: UpdateAttack(); break;
            }
        }

        // ── 상태별 업데이트 ───────────────────────────────────────────

        private void UpdateIdle()
        {
            if (_playerTransform == null) return;

            if (DistToPlayer() <= _data.detectionRange)
                ChangeState(State.Chase);
        }

        private void UpdateChase()
        {
            if (_playerTransform == null) { ChangeState(State.Idle); return; }

            float dist = DistToPlayer();

            if (dist > _data.detectionRange) { ChangeState(State.Idle);   return; }
            if (dist <= _data.attackRange)   { ChangeState(State.Attack); return; }

            Vector3 dir = (_playerTransform.position - transform.position).normalized;
            transform.position += dir * _data.stat.moveSpeed * Time.deltaTime;
            transform.LookAt(_playerTransform);
        }

        private void UpdateAttack()
        {
            if (_playerTransform == null) { ChangeState(State.Idle); return; }

            if (DistToPlayer() > _data.attackRange) { ChangeState(State.Chase); return; }

            transform.LookAt(_playerTransform);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f) return;

            _attackTimer = _data.attackCooldown;
            PerformAttack();
        }

        // ── 공격 ──────────────────────────────────────────────────────

        private void PerformAttack()
        {
            var playerCharacter = _playerTransform.GetComponent<Character>();
            if (playerCharacter == null) return;

            _pipeline.Execute(_character, playerCharacter,
                              _data.stat.attackPower, DamageType.Physical);
        }

        // ── 사망 ──────────────────────────────────────────────────────

        private void OnDead()
        {
            ChangeState(State.Dead);

            GameEventBus.Publish(new GameEvent
            {
                Type     = GameEventType.MonsterKilled,
                TargetId = _data.monsterId,
                Value    = 1
            });

            Destroy(gameObject, 2f);
        }

        // ── IInteractable ─────────────────────────────────────────────

        public void OnInteract()
        {
            if (_state == State.Dead) return;

            TargetingSystem.Instance.SetTarget(_character);
        }

        // ── 유틸 ──────────────────────────────────────────────────────

        private void ChangeState(State next)
        {
            if (_state == next) return;
            _state = next;

            if (_state == State.Attack)
                _attackTimer = 0f;
        }

        private float DistToPlayer() =>
            _playerTransform == null ? float.MaxValue :
            Vector3.Distance(transform.position, _playerTransform.position);

        private Transform GetPlayerTransform() =>
            PlayerRegistry.Player != null ? PlayerRegistry.Player.transform : null;
    }
}
