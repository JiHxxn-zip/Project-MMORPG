using System.Collections.Generic;
using UnityEngine;

namespace MMORPG.Data
{
    /// <summary>
    /// 플레이어의 기본 스탯과 스킬 목록을 정의하는 불변 데이터 컨테이너.
    /// 런타임 상태(현재 HP 등)는 이 SO에 저장하지 않는다.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerSO", menuName = "MMORPG/Data/PlayerSO")]
    public class PlayerSO : ScriptableObject
    {
        /// <summary>캐릭터 표시 이름.</summary>
        public string characterName;

        /// <summary>이동 속도 (Unity units/s).</summary>
        public float moveSpeed;

        /// <summary>최대 HP.</summary>
        public float maxHp;

        /// <summary>기본 공격력.</summary>
        public float attackPower;

        /// <summary>기본 공격 사거리.</summary>
        public float attackRange;

        /// <summary>캐릭터 회전 속도 (deg/s, Slerp 계수).</summary>
        public float rotationSpeed = 10f;

        /// <summary>보유 스킬 목록.</summary>
        public List<SkillSO> skills;

        /// <summary>캐릭터 초상화 이미지.</summary>
        public Sprite portrait;
    }
}
