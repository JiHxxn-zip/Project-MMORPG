using UnityEngine;

namespace MMORPG.Game
{
    public class MonsterAttackState : MonsterStateBase
    {
        private float _attackTimer;

        public MonsterAttackState(MonsterStateMachine sm) : base(sm) { }

        public override void Enter()
        {
            _attackTimer = 0f;  // 진입 즉시 첫 공격
            Owner.Animator.SetSpeed(0f);
        }

        public override void Update()
        {
            if (Owner.PlayerTransform == null) { StateMachine.ChangeState(new MonsterIdleState(StateMachine));  return; }
            if (Owner.DistToPlayer() > Owner.Data.attackRange) { StateMachine.ChangeState(new MonsterChaseState(StateMachine)); return; }

            Owner.transform.LookAt(Owner.PlayerTransform);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f) return;

            _attackTimer = Owner.Data.attackCooldown;
            Owner.Animator.TriggerAttack();
            Owner.PerformAttack();
        }

        public override void Exit() { }
    }
}
