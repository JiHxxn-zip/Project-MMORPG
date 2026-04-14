using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Data
{
    /// <summary>
    /// 스킬 하나의 수치와 이펙트 참조를 정의하는 불변 데이터 컨테이너.
    /// 이펙트 Prefab은 Addressable 키 문자열로 간접 참조한다.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillSO", menuName = "MMORPG/Data/SkillSO")]
    public class SkillSO : ScriptableObject
    {
        /// <summary>스킬 고유 ID. Addressable 키 "skill/{skillId}" 와 일치해야 한다.</summary>
        public string skillId;

        /// <summary>스킬 표시 이름.</summary>
        public string skillName;

        /// <summary>스킬 공격 방식.</summary>
        public SkillType skillType;

        /// <summary>스킬 데미지.</summary>
        public float damage;

        /// <summary>재사용 대기시간 (초).</summary>
        public float cooldown;

        /// <summary>스킬 사거리.</summary>
        public float range;

        /// <summary>이펙트 Prefab의 Addressable 키 ("fx/{effectId}" 형식).</summary>
        public string effectAddressableKey;

        /// <summary>스킬 아이콘 이미지.</summary>
        public Sprite icon;
    }
}
