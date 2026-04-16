/// <summary>
/// 플레이어 컨트롤러를 정적으로 등록/해제하는 레지스트리.
/// PlayerController가 OnEnable/OnDisable에서 자동으로 등록·해제한다.
/// </summary>
namespace MMORPG.Game
{
    public static class PlayerRegistry
    {
        public static PlayerController Player { get; private set; }

        public static void Register(PlayerController player)   => Player = player;
        public static void Unregister(PlayerController player) { if (Player == player) Player = null; }
    }
}
