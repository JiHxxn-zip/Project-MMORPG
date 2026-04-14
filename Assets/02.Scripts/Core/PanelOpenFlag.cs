/// <summary>
/// 패널 열기 동작을 조합 가능한 비트 플래그로 제어한다.
/// </summary>
namespace MMORPG.Core
{
    [System.Flags]
    public enum PanelOpenFlag
    {
        None         = 0,
        KeepPrevious = 1 << 0,  // 이전 패널 유지 (현재 패널 위에 쌓기)
        AlwaysOnTop  = 1 << 1,  // Stack 무관하게 별도 Root에 표시 (Loading 등)
        ClearStack   = 1 << 2,  // 열기 전 Stack 전부 닫기 (Main 귀환 시)
    }
}
