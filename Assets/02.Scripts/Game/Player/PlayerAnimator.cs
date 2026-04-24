/// <summary>
/// Animator 컴포넌트를 래핑해 파라미터 해시를 캐싱하고 타입 안전한 조작 메서드를 제공한다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private static readonly int HashSpeed         = Animator.StringToHash("Speed");
        private static readonly int HashIsInteracting = Animator.StringToHash("IsInteracting");
        private static readonly int HashAttack        = Animator.StringToHash("Attack");
        private static readonly int HashAttackIndex   = Animator.StringToHash("AttackIndex");

        public void SetSpeed(float speed)
            => _animator.SetFloat(HashSpeed, speed);

        public void SetInteracting(bool value)
            => _animator.SetBool(HashIsInteracting, value);

        /// <summary>콤보 인덱스(1~3)를 세팅한 뒤 Attack 트리거를 발동한다.</summary>
        public void TriggerAttack(int comboIndex)
        {
            _animator.SetInteger(HashAttackIndex, comboIndex);
            _animator.SetTrigger(HashAttack);
        }

        /// <summary>직접 상태 재생이 필요한 경우에 사용한다.</summary>
        public void Play(string stateName)
            => _animator.Play(stateName);
    }
}
