using System.Collections.Generic;
using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Game
{
    /// <summary>
    /// 근접 무기 콜라이더에 붙이는 스크립트.
    /// 애니메이션 이벤트로 EnableHit() / DisableHit() 을 호출해 판정 구간을 제어한다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MeleeWeapon : MonoBehaviour
    {
        [SerializeField] private DamageType _damageType = DamageType.Physical;

        private Character          _owner;
        private DamagePipeline     _pipeline;
        private Collider           _col;
        private HashSet<Collider>  _hitBuffer;   // 한 스윙에 같은 대상 중복 처리 방지

        private void Awake()
        {
            _owner     = GetComponentInParent<Character>();
            _pipeline  = new DamagePipeline();
            _hitBuffer = new HashSet<Collider>();
            _col       = GetComponent<Collider>();
            _col.isTrigger = true;
            DisableHit();
        }

        // ── 애니메이션 이벤트에서 호출 ────────────────────────────────

        public void EnableHit()
        {
            _hitBuffer.Clear();
            _col.enabled = true;
        }

        public void DisableHit()
        {
            _col.enabled = false;
        }

        // ── 충돌 처리 ─────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (_owner == null)            return;
            if (_hitBuffer.Contains(other)) return;

            var defender = other.GetComponentInParent<Character>();
            if (defender == null || defender == _owner) return;

            _hitBuffer.Add(other);
            _pipeline.Execute(_owner, defender, _owner.AttackPower, _damageType);
        }
    }
}
