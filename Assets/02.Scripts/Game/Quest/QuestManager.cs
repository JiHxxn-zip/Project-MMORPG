/// <summary>
/// 퀘스트 런타임 상태를 관리하는 싱글턴 매니저.
/// 정의 데이터(QuestSO)는 수락 시 캐싱하고, 진행 상태는 내부 Dictionary에 보관한다.
/// 진행도 카운트는 QuestConditionData.currentCount 에서 직접 읽는다.
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
        private readonly Dictionary<string, QuestSO>            _questData = new();
        private readonly Dictionary<string, QuestProgressState> _states    = new();

        public event Action<QuestSO> OnQuestAccepted;
        public event Action<QuestSO> OnQuestCompleted;

        // ── 이벤트 버스 구독 ───────────────────────────────────────────

        private void OnEnable()  => GameEventBus.OnEvent += HandleEvent;
        private void OnDisable() => GameEventBus.OnEvent -= HandleEvent;

        private void HandleEvent(GameEvent e)
        {
            foreach (var quest in GetActiveQuests())
            {
                if (quest.conditions == null || quest.conditions.Count == 0) continue;  // #1 null 가드

                foreach (var cond in quest.conditions)
                {
                    if (cond.eventType != e.Type)     continue;
                    if (cond.targetId  != e.TargetId) continue;

                    cond.currentCount += e.Value;
                    Debug.Log($"[QuestManager] 진행도: {quest.questId} | {cond.eventType}:{cond.targetId} → {cond.currentCount}/{cond.requiredCount}");

                    if (quest.IsAllMet) CompleteQuest(quest);
                }
            }
        }

        private IEnumerable<QuestSO> GetActiveQuests()
        {
            foreach (var quest in _questData.Values)
                if (GetState(quest.questId) == QuestProgressState.Active)
                    yield return quest;
        }

        // ── 상태 조회 ─────────────────────────────────────────────────

        /// <summary>퀘스트 현재 상태. 미등록은 Available로 간주한다.</summary>
        public QuestProgressState GetState(string questId)
            => _states.TryGetValue(questId, out var state) ? state : QuestProgressState.Available;

        /// <summary>
        /// 퀘스트 전체 진행도 합산 (트래커 UI용).
        /// 단일 조건 퀘스트는 그대로 사용 가능. 다중 조건은 QuestSO.conditions 를 직접 참조.
        /// </summary>
        public int GetProgress(string questId)  // #3 conditions 기반
        {
            if (!_questData.TryGetValue(questId, out var quest) || quest.conditions == null)
                return 0;
            int total = 0;
            foreach (var cond in quest.conditions)
                total += cond.currentCount;
            return total;
        }

        public bool CanComplete(QuestSO quest) => quest != null && quest.IsAllMet;

        /// <summary>이 npcId를 목표로 하는 Active 상태의 TalkToNPC 퀘스트를 반환한다.</summary>
        public QuestSO GetActiveTalkQuest(string npcId)  // #4 conditions 기반
        {
            foreach (var quest in _questData.Values)
            {
                if (GetState(quest.questId) != QuestProgressState.Active) continue;
                if (quest.conditions == null) continue;

                foreach (var cond in quest.conditions)
                {
                    if (cond.eventType == GameEventType.NpcTalked && cond.targetId == npcId)
                        return quest;
                }
            }
            return null;
        }

        // ── 상태 변경 ─────────────────────────────────────────────────

        public void AcceptQuest(QuestSO quest)
        {
            if (quest == null) return;
            if (string.IsNullOrEmpty(quest.questId)) { Debug.LogWarning($"[QuestManager] questId가 비어있는 퀘스트 수락 시도: {quest.title}"); return; }
            if (GetState(quest.questId) != QuestProgressState.Available) return;

            // #5 선행 퀘스트 체크
            if (!string.IsNullOrEmpty(quest.prerequisiteQuestId) &&
                GetState(quest.prerequisiteQuestId) != QuestProgressState.Completed)
            {
                Debug.LogWarning($"[QuestManager] 선행 퀘스트 미완료로 수락 불가: {quest.title} (requires: {quest.prerequisiteQuestId})");
                return;
            }

            _questData[quest.questId] = quest;
            _states[quest.questId]    = QuestProgressState.Active;
            Debug.Log($"[QuestManager] 수락: {quest.title}");
            OnQuestAccepted?.Invoke(quest);
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
