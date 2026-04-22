/// <summary>
/// 퀘스트 트래커의 단일 항목. 퀘스트 제목과 조건 진행도를 표시한다.
/// QuestTrackerPanel이 Instantiate 후 Setup()을 호출한다.
/// </summary>
using System.Text;
using TMPro;
using UnityEngine;
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class QuestTrackerEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _conditionsText;

        private QuestSO _quest;

        public string QuestId => _quest != null ? _quest.questId : string.Empty;

        public void Setup(QuestSO quest)
        {
            _quest = quest;
            _titleText.text = quest.title;
            RefreshConditions();
        }

        public void Refresh()
        {
            RefreshConditions();
        }

        private void RefreshConditions()
        {
            if (_quest?.conditions == null) return;

            var sb = new StringBuilder();
            foreach (var cond in _quest.conditions)
            {
                string label = string.IsNullOrEmpty(cond.label) ? cond.targetId : cond.label;

                if (cond.eventType == GameEventType.NpcTalked)
                {
                    // 대화 퀘스트: 카운트 없이 레이블만 표시
                    sb.AppendLine($"- {label}");
                }
                else
                {
                    // 킬·수집 퀘스트: 진행도 표시
                    sb.AppendLine($"- {label} {cond.currentCount}/{cond.requiredCount}");
                }
            }

            _conditionsText.text = sb.ToString().TrimEnd();
        }
    }
}
