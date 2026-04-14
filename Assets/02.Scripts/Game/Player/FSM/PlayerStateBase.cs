/// <summary>
/// 모든 플레이어 상태의 추상 베이스 클래스. StateMachine과 Owner 접근을 제공한다.
/// </summary>
namespace MMORPG.Game
{
    public abstract class PlayerStateBase
    {
        protected PlayerStateMachine StateMachine { get; }
        protected PlayerController Owner => StateMachine.Owner;

        protected PlayerStateBase(PlayerStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}
