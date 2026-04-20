using UnityEngine;
using MMORPG.Data;

namespace MMORPG.Game
{
    [RequireComponent(typeof(StatHandler))]
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterStatSO _stat;

        public StatHandler StatHandler { get; private set; }

        // 런타임 실드량 (버프 등으로 외부에서 조작)
        public float ShieldAmount { get; set; } = 0f;

        // 전투 스탯 프로퍼티
        public float Defense     => _stat.defense;
        public float MagicResist => _stat.magicResist;
        public float CritRate    => _stat.luck / (_stat.luck + 100f);   // Diminishing returns
        public float CritDamage  => _stat.critMultiplier;
        public float AttackPower => _stat.attackPower;

        private void Awake()
        {
            StatHandler = GetComponent<StatHandler>();
        }
    }
}
