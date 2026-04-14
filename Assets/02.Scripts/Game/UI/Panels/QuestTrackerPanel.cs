/// <summary>
/// 진행 중인 퀘스트 목록을 화면에 표시하는 트래커 패널. Step 7에서 QuestManager와 연동 예정.
/// </summary>
using UnityEngine;

namespace MMORPG.Game
{
    public class QuestTrackerPanel : GameUIPanel
    {
        // TODO(Step 7): QuestManager.OnQuestUpdated 이벤트 구독 후 트래커 항목 갱신
        [SerializeField] private Transform _trackerContainer;

        public override void OnOpen() { }
        public override void OnClose() { }
    }
}
