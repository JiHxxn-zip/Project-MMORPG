namespace MMORPG.Game
{
    public class MonsterStateMachine
    {
        public MonsterController Owner        { get; }
        public MonsterStateBase  CurrentState { get; private set; }

        public MonsterStateMachine(MonsterController owner)
        {
            Owner = owner;
        }

        public void ChangeState(MonsterStateBase newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}
