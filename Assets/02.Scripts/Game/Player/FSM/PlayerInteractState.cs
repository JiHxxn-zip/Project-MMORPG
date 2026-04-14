/// <summary>
/// NPC 대화 등 인터랙션 중 상태. 이동 입력을 차단하며, 종료는 외부(DialogueSystem)가 EndInteract()로 호출한다.
/// </summary>
namespace MMORPG.Game
{
    public class PlayerInteractState : PlayerStateBase
    {
        public PlayerInteractState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            Owner.Animator.SetInteracting(true);
            Owner.Animator.SetSpeed(0f);
        }

        public override void Update()
        {
            // 이동 입력 무시. 상태 전환은 EndInteract()를 통해서만 이루어진다.
        }

        public override void Exit()
        {
            Owner.Animator.SetInteracting(false);
        }

        /// <summary>
        /// DialogueSystem이 대화를 종료할 때 호출한다.
        /// </summary>
        public void EndInteract()
        {
            StateMachine.ChangeState(new PlayerIdleState(StateMachine));
        }
    }
}
