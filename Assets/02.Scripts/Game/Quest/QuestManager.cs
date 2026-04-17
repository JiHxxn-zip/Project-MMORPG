/// <summary>
/// 퀘스트 런타임 상태를 관리하는 싱글턴 매니저.
/// 정의 데이터(QuestSO)는 수락 시 캐싱하고, 진행 상태와 카운트는 내부 Dictionary에 보관한다.
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;
using MMORPG.Core;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class QuestManager : SingletonManager<QuestManager>
    {
        private readonly Dictionary<string, QuestSO>            _questData     = new();
        private readonly Dictionary<string, QuestProgressState> _states        = new();
        private readonly Dictionary<string, int>                _progressCount = new();

        public event Action<QuestSO> OnQuestAccepted;
        public event Action<QuestSO> OnQuestCompleted;

        // ── 상태 조회 ─────────────────────────────────────────────────

        /// <summary>퀘스트 현재 상태. 미등록은 Available로 간주한다.</summary>
        public QuestProgressState GetState(string questId)
            => _states.TryGetValue(questId, out var state) ? state : QuestProgressState.Available;

        public int GetProgress(string questId)
            => _progressCount.TryGetValue(questId, out var count) ? count : 0;

        public bool CanComplete(QuestSO quest)
        {
            if (quest == null) return false;
            return quest.questType switch
            {
                QuestType.KillMonster => quest.targetCount > 0 && GetProgress(quest.questId) >= quest.targetCount,
                QuestType.TalkToNPC   => quest.targetCount > 0 && GetProgress(quest.questId) >= quest.targetCount,
                QuestType.CollectItem => false, // TODO: Inventory 연동
                _                     => false
            };
        }

        /// <summary>이 npcId를 목표로 하는 Active 상태의 TalkToNPC 퀘스트를 반환한다.</summary>
        public QuestSO GetActiveTalkQuest(string npcId)
        {
            foreach (var quest in _questData.Values)
            {
                if (quest.questType             != QuestType.TalkToNPC)          continue;
                if (quest.targetId              != npcId)                         continue;
                if (GetState(quest.questId)     != QuestProgressState.Active)     continue;
                return quest;
            }
            return null;
        }

        // ── 상태 변경 ─────────────────────────────────────────────────

        public void AcceptQuest(QuestSO quest)
        {
            if (quest == null) return;
            if (string.IsNullOrEmpty(quest.questId)) { Debug.LogWarning($"[QuestManager] questId가 비어있는 퀘스트 수락 시도: {quest.title}"); return; }
            if (GetState(quest.questId) != QuestProgressState.Available) return;
            _questData[quest.questId]  = quest;
            _states[quest.questId]     = QuestProgressState.Active;
            Debug.Log($"[QuestManager] 수락: {quest.title}");
            OnQuestAccepted?.Invoke(quest);
        }

        public void AddProgress(string questId, int amount = 1)
        {
            if (GetState(questId) != QuestProgressState.Active) return;
            _progressCount[questId] = GetProgress(questId) + amount;
            Debug.Log($"[QuestManager] 진행도: {questId} → {GetProgress(questId)}");
        }

        public void CompleteQuest(QuestSO quest)
        {
            if (quest == null) return;
            if (string.IsNullOrEmpty(quest.questId)) { Debug.LogWarning($"[QuestManager] questId가 비어있는 퀘스트 완료 시도: {quest.title}"); return; }
            if (GetState(quest.questId) == QuestProgressState.Completed) return;
            _states[quest.questId] = QuestProgressState.Completed;
            GrantReward(quest.reward);
            Debug.Log($"[QuestManager] 완료: {quest.title}");
            OnQuestCompleted?.Invoke(quest);
        }

        // ── 보상 ──────────────────────────────────────────────────────

        private void GrantReward(QuestReward reward)
        {
            // TODO: PlayerStats.Instance.AddExp(reward.exp);
            // TODO: PlayerInventory.Instance.AddGold(reward.gold);
            Debug.Log($"[QuestManager] 보상 — EXP: {reward.exp}, Gold: {reward.gold}, Item: {reward.itemId}");
        }
    }
}
