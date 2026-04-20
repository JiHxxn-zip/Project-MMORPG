using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Game
{
    public struct DamageResult
    {
        public float FinalDamage;
        public bool  IsCritical;
    }

    public class DamagePipeline
    {
        public DamageResult Execute(Character attacker, Character defender,
                                    float rawDamage, DamageType type)
        {
            // 1. 크리티컬 판정
            float damage = ApplyCritical(rawDamage, attacker, out bool isCrit);

            // 2. 방어/저항 적용
            damage = ApplyDefense(damage, defender, type);

            // 3. HP 차감 + 이벤트 발행
            ApplyAndNotify(defender, damage, isCrit);

            return new DamageResult { FinalDamage = damage, IsCritical = isCrit };
        }

        private float ApplyCritical(float raw, Character attacker, out bool isCrit)
        {
            isCrit = Random.value < attacker.CritRate;
            return isCrit ? raw * attacker.CritDamage : raw;
        }

        private float ApplyDefense(float damage, Character defender, DamageType type)
        {
            if (type == DamageType.True) return damage;

            if (type == DamageType.Magic)
                damage *= 1f - defender.MagicResist / 100f;

            float reduction = defender.Defense / (defender.Defense + 100f);
            return damage * (1f - reduction);
        }

        private void ApplyAndNotify(Character defender, float damage, bool isCrit)
        {
            defender.StatHandler.ModifyHP(-damage);

            CombatEventBus.Publish(new DamageTakenEvent
            {
                Defender    = defender,
                FinalDamage = damage,
                IsCritical  = isCrit
            });
        }
    }
}
