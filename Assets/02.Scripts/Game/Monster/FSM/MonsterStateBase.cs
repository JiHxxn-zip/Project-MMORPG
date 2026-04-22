namespace MMORPG.Game
{
    public abstract class MonsterStateBase
    {
        protected MonsterStateMachine StateMachine { get; }
        protected MonsterController Owner => StateMachine.Owner;

        protected MonsterStateBase(MonsterStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}
