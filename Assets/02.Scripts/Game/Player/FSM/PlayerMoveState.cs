/// <summary>
/// 이동 상태. 카메라 기준 방향으로 CharacterController를 이동하고 회전을 Slerp 처리한다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class PlayerMoveState : PlayerStateBase
    {
        public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() { }

        public override void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (h == 0f && v == 0f)
            {
                StateMachine.ChangeState(new PlayerIdleState(StateMachine));
                return;
            }

            // 카메라 기준 이동 방향 계산
            Vector3 camForward = Owner.CameraTransform.forward;
            Vector3 camRight   = Owner.CameraTransform.right;
            camForward.y = 0f; camForward.Normalize();
            camRight.y   = 0f; camRight.Normalize();
            Vector3 moveDir = (camForward * v + camRight * h).normalized;

            // 이동
            Owner.CC.Move(moveDir * Owner.Data.moveSpeed * Time.deltaTime);

            // 이동 방향으로 회전
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                Owner.transform.rotation = Quaternion.Slerp(
                    Owner.transform.rotation,
                    targetRot,
                    Owner.Data.rotationSpeed * Time.deltaTime);
            }

            // 애니메이터 속도 갱신
            float speed = new Vector2(h, v).magnitude;
            Owner.Animator.SetSpeed(speed);
        }

        public override void Exit()
        {
            Owner.Animator.SetSpeed(0f);
        }
    }
}
