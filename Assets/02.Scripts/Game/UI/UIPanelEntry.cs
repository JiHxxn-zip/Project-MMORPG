/// <summary>
/// UIPanelDataSO 리스트의 각 항목. 패널 타입과 프리팹을 연결한다.
/// </summary>
using MMORPG.Core;

namespace MMORPG.Game
{
    [System.Serializable]
    public struct UIPanelEntry
    {
        public UIPanelType panelType;
        public GameUIPanel prefab;
    }
}
