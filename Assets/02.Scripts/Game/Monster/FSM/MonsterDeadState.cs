namespace MMORPG.Game
{
    public class MonsterDeadState : MonsterStateBase
    {
        public MonsterDeadState(MonsterStateMachine sm) : base(sm) { }

        public override void Enter()
        {
            Owner.Animator.TriggerDie();    // Destroy는 MonsterController.OnDead()에서 처리
        }
        public override void Update() { }
        public override void Exit()   { }
    }
}
