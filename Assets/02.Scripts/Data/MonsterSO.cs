using UnityEngine;

namespace MMORPG.Data
{
    [CreateAssetMenu(fileName = "MonsterSO", menuName = "MMORPG/Data/MonsterSO")]
    public class MonsterSO : ScriptableObject
    {
        public string          monsterId;
        public string          monsterName;
        public CharacterStatSO stat;

        [Header("AI")]
        public float detectionRange = 8f;
        public float attackRange    = 2f;
        public float attackCooldown = 1.5f;
    }
}
