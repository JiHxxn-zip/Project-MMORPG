/// <summary>
/// NPC 머리 위 상호작용 아이콘(F키)을 표시/숨김 처리하는 컴포넌트.
/// LateUpdate에서 빌보드 효과를 적용해 항상 카메라를 바라본다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class NPCInteractionIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject _iconRoot;

        private void Awake()
        {
            Hide();
        }

        public void Show()
        {
            _iconRoot.SetActive(true);
        }

        public void Hide()
        {
            _iconRoot.SetActive(false);
        }

        private void LateUpdate()
        {
            if (Camera.main == null) return;
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
