using System.Collections;
using UnityEngine;

namespace MMORPG.Game
{
    /// <summary>테스트용 — 스페이스바로 무기 판정을 1회 발동한다. 완성 후 삭제할 것.</summary>
    public class WeaponTestInput : MonoBehaviour
    {
        [SerializeField] private float _hitWindow = 0.2f; // 충돌 없을 때 자동 비활성화 대기 시간

        private MeleeWeapon _weapon;
        private Coroutine   _autoDisable;

        private void Awake()
        {
            _weapon = GetComponentInChildren<MeleeWeapon>();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;

            // 타겟이 없으면 반응하지 않는다
            if (TargetingSystem.Instance.CurrentTarget == null) return;

            // 이미 판정 중이면 중복 발동 방지
            if (_autoDisable != null) return;

            _weapon.EnableHit();
            _autoDisable = StartCoroutine(AutoDisable());
        }

        // MeleeWeapon.OnTriggerEnter 에서 DisableHit()가 먼저 호출되는 경우에도
        // 코루틴이 남아있으므로 정리해준다.
        private IEnumerator AutoDisable()
        {
            yield return new WaitForSeconds(_hitWindow);
            _weapon.DisableHit();
            _autoDisable = null;
        }
    }
}
