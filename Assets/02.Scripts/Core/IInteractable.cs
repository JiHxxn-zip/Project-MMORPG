using UnityEngine;

namespace MMORPG.Core
{
    /// <summary>
    /// 플레이어가 상호작용할 수 있는 오브젝트가 구현해야 하는 인터페이스.
    /// NPC, 오브젝트, 아이템 등 상호작용 대상에 적용한다.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>상호작용을 실행한다.</summary>
        /// <param name="interactor">상호작용을 시도한 주체 GameObject</param>
        void Interact(GameObject interactor);
    }
}
