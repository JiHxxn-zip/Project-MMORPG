/// <summary>
/// 진행 중인 퀘스트 목록을 표시하는 트래커. MainPanel의 자식 오브젝트에 부착한다.
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class QuestTrackerPanel : MonoBehaviour
    {
        [SerializeField] private Transform         _container;
        [SerializeField] private QuestTrackerEntry _entryPrefab;

        private readonly Dictionary<string, QuestTrackerEntry> _entries = new();

        private void OnEnable()
        {
            QuestManager.Instance.OnQuestAccepted        += OnQuestAccepted;
            QuestManager.Instance.OnQuestCompleted       += OnQuestCompleted;
            QuestManager.Instance.OnQuestProgressUpdated += OnQuestProgressUpdated;

            foreach (var quest in QuestManager.Instance.GetActiveQuests())
                SpawnEntry(quest);
        }

        private void OnDisable()
        {
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.OnQuestAccepted        -= OnQuestAccepted;
            QuestManager.Instance.OnQuestCompleted       -= OnQuestCompleted;
            QuestManager.Instance.OnQuestProgressUpdated -= OnQuestProgressUpdated;
        }

        private void OnQuestAccepted(QuestSO quest)        => SpawnEntry(quest);
        private void OnQuestCompleted(QuestSO quest)       => RemoveEntry(quest.questId);
        private void OnQuestProgressUpdated(QuestSO quest) => RefreshEntry(quest.questId);

        private void SpawnEntry(QuestSO quest)
        {
            if (_entries.ContainsKey(quest.questId)) return;
            var entry = Instantiate(_entryPrefab, _container);
            entry.Setup(quest);
            _entries[quest.questId] = entry;
        }

        private void RemoveEntry(string questId)
        {
            if (!_entries.TryGetValue(questId, out var entry)) return;
            _entries.Remove(questId);
            Destroy(entry.gameObject);
        }

        private void RefreshEntry(string questId)
        {
            if (_entries.TryGetValue(questId, out var entry))
                entry.Refresh();
        }
    }
}
