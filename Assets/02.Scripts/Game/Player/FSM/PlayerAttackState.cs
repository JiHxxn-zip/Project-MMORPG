/// <summary>
/// 근접 공격 상태. 최대 3타 콤보를 지원한다.
/// 애니메이션의 EnableHit / DisableHit 이벤트로 MeleeWeapon 판정 구간을 제어한다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class PlayerAttackState : PlayerStateBase
    {
        private const int   MAX_COMBO        = 3;
        private const float COMBO_WINDOW     = 0.8f;   // 다음 타 입력을 받는 시간(초)

        private int   _comboIndex;
        private bool  _nextAttackQueued;
        private float _comboTimer;

        public PlayerAttackState(PlayerStateMachine stateMachine, int comboIndex = 1)
            : base(stateMachine)
        {
            _comboIndex = comboIndex;
        }

        public override void Enter()
        {
            _nextAttackQueued = false;
            _comboTimer       = COMBO_WINDOW;
            Owner.Animator.TriggerAttack(_comboIndex);
            if (Owner.MeleeWeapon != null)
                Owner.MeleeWeapon.CurrentComboIndex = _comboIndex;
        }

        public override void Update()
        {
            // 콤보 입력 감지
            if (Input.GetKeyDown(KeyCode.Space) && _comboIndex < MAX_COMBO)
                _nextAttackQueued = true;

            _comboTimer -= Time.deltaTime;

            if (_comboTimer <= 0f)
            {
                if (_nextAttackQueued)
                    StateMachine.ChangeState(new PlayerAttackState(StateMachine, _comboIndex + 1));
                else
                    StateMachine.ChangeState(new PlayerIdleState(StateMachine));
            }
        }

        public override void Exit()
        {
            Owner.Animator.SetSpeed(0f);
        }
    }
}
