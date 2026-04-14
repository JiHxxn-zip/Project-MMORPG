/// <summary>
/// 정지 상태. 이동 입력이 감지되면 MoveState로 전환한다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class PlayerIdleState : PlayerStateBase
    {
        public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Owner.Animator.SetSpeed(0f);
        }

        public override void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (h != 0f || v != 0f)
                StateMachine.ChangeState(new PlayerMoveState(StateMachine));
        }

        public override void Exit() { }
    }
}
