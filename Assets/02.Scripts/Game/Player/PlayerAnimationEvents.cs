/// <summary>
/// 플레이어 Animator와 동일한 오브젝트에 붙인다.
/// Animation Event → 이 컴포넌트 메서드 → MeleeWeapon으로 중계한다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        [SerializeField] private MeleeWeapon _meleeWeapon;

        // Animation Event에서 호출
        public void EnableHit()  => _meleeWeapon.EnableHit();
        public void DisableHit() => _meleeWeapon.DisableHit();
    }
}
