using UnityEngine;

namespace MMORPG.Game
{
    public class MonsterAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private static readonly int HashSpeed      = Animator.StringToHash("Speed");
        private static readonly int HashAttack     = Animator.StringToHash("Attack");
        private static readonly int HashTakeDamage = Animator.StringToHash("TakeDamage");
        private static readonly int HashDie        = Animator.StringToHash("Die");

        public void SetSpeed(float speed)       => _animator.SetFloat(HashSpeed, speed);
        public void TriggerAttack()             => _animator.SetTrigger(HashAttack);
        public void TriggerTakeDamage()         => _animator.SetTrigger(HashTakeDamage);
        public void TriggerDie()                => _animator.SetTrigger(HashDie);
    }
}
