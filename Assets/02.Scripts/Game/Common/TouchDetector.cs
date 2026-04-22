using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Game
{
    /// <summary>
    /// 같은 GameObject의 IInteractable 구현체를 찾아 OnMouseDown 시 OnInteract를 호출한다.
    /// NPC / Monster 어디에 붙여도 코드 변경 없이 동작한다.
    /// </summary>
    public class TouchDetector : MonoBehaviour
    {
        private IInteractable _interactable;

        private void Awake()
        {
            _interactable = GetComponent<IInteractable>();
        }

        private void OnMouseDown()
        {
            _interactable?.OnInteract();
        }
    }
}
