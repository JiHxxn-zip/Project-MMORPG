/// <summary>
/// 모든 UI 패널의 베이스 클래스. 열기/닫기 훅과 외부 닫기 요청을 제공한다.
/// </summary>
using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Game
{
    public abstract class GameUIPanel : MonoBehaviour
    {
        [SerializeField] private UIPanelType _panelType;
        public UIPanelType PanelType => _panelType;

        /// <summary>패널이 열릴 때 UIManager가 호출한다.</summary>
        public virtual void OnOpen() { }

        /// <summary>패널이 닫힐 때 UIManager가 호출한다.</summary>
        public virtual void OnClose() { }

        /// <summary>닫기 버튼 등 외부에서 호출. UIManager에 위임한다.</summary>
        public void Close()
        {
            UIManager.Instance.ClosePanel(PanelType);
        }
    }
}
