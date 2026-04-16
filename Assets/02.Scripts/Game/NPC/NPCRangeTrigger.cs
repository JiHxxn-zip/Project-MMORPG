/// <summary>
/// NPC 상호작용 범위를 트리거 콜라이더로 감지하는 컴포넌트.
/// 플레이어 진입 시 Indicator를 표시하고 PlayerController에 현재 NPC를 등록한다.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class NPCRangeTrigger : MonoBehaviour
    {
        [SerializeField] private NPCController _npc;

        private void OnTriggerEnter(Collider other)
        {
            if (PlayerRegistry.Player == null) return;
            if (other.gameObject != PlayerRegistry.Player.gameObject) return;

            _npc.Indicator.Show();
            PlayerRegistry.Player.SetCurrentNPC(_npc);
        }

        private void OnTriggerExit(Collider other)
        {
            if (PlayerRegistry.Player == null) return;
            if (other.gameObject != PlayerRegistry.Player.gameObject) return;

            _npc.Indicator.Hide();
            PlayerRegistry.Player.ClearCurrentNPC();
        }
    }
}
