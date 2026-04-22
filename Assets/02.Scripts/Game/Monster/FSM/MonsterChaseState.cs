using UnityEngine;

namespace MMORPG.Game
{
    public class MonsterChaseState : MonsterStateBase
    {
        public MonsterChaseState(MonsterStateMachine sm) : base(sm) { }

        public override void Enter() { }

        public override void Update()
        {
            if (Owner.PlayerTransform == null) { StateMachine.ChangeState(new MonsterIdleState(StateMachine)); return; }

            float dist = Owner.DistToPlayer();

            if (dist > Owner.Data.detectionRange) { StateMachine.ChangeState(new MonsterIdleState(StateMachine));  return; }
            if (dist <= Owner.Data.attackRange)   { StateMachine.ChangeState(new MonsterAttackState(StateMachine)); return; }

            Vector3 dir = (Owner.PlayerTransform.position - Owner.transform.position).normalized;
            Owner.transform.position += dir * Owner.Data.stat.moveSpeed * Time.deltaTime;
            Owner.transform.LookAt(Owner.PlayerTransform);
        }

        public override void Exit() { }
    }
}
