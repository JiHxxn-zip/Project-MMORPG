using UnityEngine;

namespace MMORPG.Data
{
    [CreateAssetMenu(fileName = "CharacterStatSO", menuName = "MMORPG/Data/CharacterStat")]
    public class CharacterStatSO : ScriptableObject
    {
        [Header("전투")]
        public float maxHp;
        public float attackPower;
        public float defense;

        [Header("마법 저항 (0~100 %)")]
        [Range(0f, 100f)]
        public float magicResist = 0f;

        [Header("크리티컬")]
        [Range(0f, 500f)]
        public float luck;
        public float critMultiplier = 1.5f;

        [Header("이동")]
        public float moveSpeed;

        [Header("경험치")]
        public int expReward;
    }
}
