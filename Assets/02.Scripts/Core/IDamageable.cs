using UnityEngine;

namespace MMORPG.Core
{
    /// <summary>
    /// 데미지를 받을 수 있는 모든 오브젝트가 구현해야 하는 인터페이스.
    /// 플레이어, 몬스터 등 피격 판정이 필요한 대상에 적용한다.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>지정한 양만큼 데미지를 적용한다.</summary>
        /// <param name="amount">적용할 데미지 수치</param>
        void TakeDamage(float amount);

        /// <summary>현재 사망 상태 여부.</summary>
        bool IsDead { get; }
    }
}
