using UnityEngine;

namespace MMORPG.Game
{
    public class MonsterKnockbackState : MonsterStateBase
    {
        private readonly Vector3 _direction;
        private readonly float   _force;
        private readonly float   _duration;
        private float            _elapsed;

        public MonsterKnockbackState(MonsterStateMachine sm, Vector3 direction, float force, float duration = 0.25f)
            : base(sm)
        {
            _direction = direction.normalized;
            _force     = force;
            _duration  = duration;
        }

        public override void Enter()
        {
            _elapsed = 0f;
        }

        public override void Update()
        {
            _elapsed += Time.deltaTime;

            // 넉백 감쇠: 시간이 지날수록 힘이 줄어든다
            float t = 1f - (_elapsed / _duration);
            Owner.transform.position += _direction * _force * t * Time.deltaTime;

            if (_elapsed >= _duration)
                StateMachine.ChangeState(new MonsterIdleState(StateMachine));
        }

        public override void Exit() { }
    }
}
