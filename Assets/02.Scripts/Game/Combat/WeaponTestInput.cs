using UnityEngine;

namespace MMORPG.Game
{
    /// <summary>테스트용 — 스페이스바로 무기 판정을 켜고 끈다. 완성 후 삭제할 것.</summary>
    public class WeaponTestInput : MonoBehaviour
    {
        private MeleeWeapon _weapon;
        private bool        _hitActive;

        private void Awake()
        {
            _weapon = GetComponentInChildren<MeleeWeapon>();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;

            _hitActive = !_hitActive;

            if (_hitActive) _weapon.EnableHit();
            else            _weapon.DisableHit();
        }
    }
}
