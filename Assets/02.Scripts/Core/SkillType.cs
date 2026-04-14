namespace MMORPG.Core
{
    /// <summary>
    /// 스킬의 공격 방식을 나타내는 Enum.
    /// SkillSO.skillType 필드에서 사용하며, CombatSystem이 히트 판정 방식을 선택하는 데 쓴다.
    /// </summary>
    public enum SkillType
    {
        /// <summary>근접 단일 대상 공격.</summary>
        Melee,

        /// <summary>원거리 투사체 공격.</summary>
        Ranged,

        /// <summary>범위 내 다수 대상 공격.</summary>
        AoE,
    }
}
