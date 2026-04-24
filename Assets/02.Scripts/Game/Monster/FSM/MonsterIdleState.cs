namespace MMORPG.Game
{
    public class MonsterIdleState : MonsterStateBase
    {
        public MonsterIdleState(MonsterStateMachine sm) : base(sm) { }

        public override void Enter()
        {
            Owner.Animator.SetSpeed(0f);
        }

        public override void Update()
        {
            if (Owner.PlayerTransform == null) return;

            if (Owner.DistToPlayer() <= Owner.Data.detectionRange)
                StateMachine.ChangeState(new MonsterChaseState(StateMachine));
        }

        public override void Exit() { }
    }
}
