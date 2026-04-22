/// <summary>
/// 게임 시작 시 로고/타이틀을 표시하는 패널.
/// GameStart 버튼을 누르면 MainPanel로 전환한다.
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using MMORPG.Core;

namespace MMORPG.Game
{
    public class LogoPanel : GameUIPanel
    {
        [SerializeField] private Button _gameStartButton;

        public override void OnOpen()
        {
            _gameStartButton.onClick.AddListener(OnGameStart);
        }

        public override void OnClose()
        {
            _gameStartButton.onClick.RemoveListener(OnGameStart);
        }

        private void OnGameStart()
        {
            UIManager.Instance.OpenPanel<MainPanel>(UIPanelType.Main);
            UIManager.Instance.ClosePanel(UIPanelType.Logo);
        }
    }
}
