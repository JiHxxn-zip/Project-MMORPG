/// <summary>
/// 플레이어 FSM. 상태 전환 시 Exit → Enter 순서를 보장한다.
/// </summary>
namespace MMORPG.Game
{
    public class PlayerStateMachine
    {
        public PlayerController Owner { get; }
        public PlayerStateBase CurrentState { get; private set; }

        public PlayerStateMachine(PlayerController owner)
        {
            Owner = owner;
        }

        public void ChangeState(PlayerStateBase newState)
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
