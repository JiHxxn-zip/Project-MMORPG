# 퀘스트 이벤트 버스 구조 초기 구현

## 목표
기존 코드를 최대한 건드리지 않고 이벤트 버스 구조를 단계적으로 적용.
시트 연동은 이후 작업. 우선 돌아가는 것이 목표.

---

## Phase 1 — 신규 파일 4개 추가 (기존 코드 무수정)

### GameEventType.cs
```csharp
public enum GameEventType
{
    MonsterKilled,
    ItemCollected,
    NpcTalked,
    ItemUsed,
    QuestCompleted
}
```

### GameEvent.cs
```csharp
public class GameEvent
{
    public GameEventType Type;
    public string        TargetId;
    public int           Value;
}
```

### GameEventBus.cs
```csharp
public static class GameEventBus
{
    public static event Action<GameEvent> OnEvent;

    public static void Publish(GameEvent e)
    {
#if UNITY_EDITOR
        Debug.Log($"[EventBus] {e.Type} | {e.TargetId} | {e.Value}");
#endif
        OnEvent?.Invoke(e);
    }
}
```

### GameEventPublisher.cs
```csharp
// 발행은 반드시 이 클래스를 통해서만.
// 발행처 추적 및 디버그 로그 일원화.
public static class GameEventPublisher
{
    public static void MonsterKilled(string monsterId, int count = 1)
        => GameEventBus.Publish(new GameEvent
           { Type = GameEventType.MonsterKilled, TargetId = monsterId, Value = count });

    public static void ItemCollected(string itemId, int count)
        => GameEventBus.Publish(new GameEvent
           { Type = GameEventType.ItemCollected, TargetId = itemId, Value = count });

    public static void NpcTalked(string npcId)
        => GameEventBus.Publish(new GameEvent
           { Type = GameEventType.NpcTalked, TargetId = npcId, Value = 1 });

    public static void ItemUsed(string itemId)
        => GameEventBus.Publish(new GameEvent
           { Type = GameEventType.ItemUsed, TargetId = itemId, Value = 1 });

    public static void QuestCompleted(string questId)
        => GameEventBus.Publish(new GameEvent
           { Type = GameEventType.QuestCompleted, TargetId = questId, Value = 1 });
}
```

---

## Phase 2 — QuestConditionData 신규 + QuestSO 수정

### QuestConditionData.cs (신규)
```csharp
[Serializable]
public class QuestConditionData
{
    public GameEventType eventType;     // enum으로 타입 안전 보장
    public string        targetId;
    public int           requiredCount;
    public int           currentCount;  // 런타임 진행도 누적
}
```

### QuestSO.cs (수정)
```csharp
// 기존 필드 유지, 아래만 추가
public List<QuestConditionData> conditions;
public bool IsAllMet => conditions != null
                     && conditions.Count > 0
                     && conditions.All(c => c.currentCount >= c.requiredCount);
```

---

## Phase 3 — QuestEventListener 신규

### QuestEventListener.cs (신규)
```csharp
// 발행 책임을 이 클래스 하나에 집중.
// 도메인 시스템(Combat, Inventory, Dialogue)은 Quest를 전혀 모름.
// 도메인 시스템의 네이티브 이벤트를 구독 → GameEventPublisher 호출.
public class QuestEventListener : MonoBehaviour
{
    private void OnEnable()
    {
        CombatSystem.OnMonsterDead   += HandleMonsterDead;
        InventorySystem.OnItemAdded  += HandleItemAdded;
        DialogueSystem.OnDialogueEnd += HandleDialogueEnd;
    }

    private void OnDisable()
    {
        CombatSystem.OnMonsterDead   -= HandleMonsterDead;
        InventorySystem.OnItemAdded  -= HandleItemAdded;
        DialogueSystem.OnDialogueEnd -= HandleDialogueEnd;
    }

    private void HandleMonsterDead(Monster m)
        => GameEventPublisher.MonsterKilled(m.id, 1);

    private void HandleItemAdded(string itemId, int count)
        => GameEventPublisher.ItemCollected(itemId, count);

    private void HandleDialogueEnd(string npcId)
        => GameEventPublisher.NpcTalked(npcId);
}
```

> **주의**: 각 도메인 시스템에 네이티브 이벤트가 없으면
> 이 단계 전에 해당 시스템에 이벤트 추가 필요.
> 확인 후 없는 것만 먼저 추가하고 진행.

---

## Phase 4 — QuestManager 수정 + PlayerInteractionHandler 1줄 교체

### QuestManager.cs (수정)
```csharp
private void OnEnable()  => GameEventBus.OnEvent += HandleEvent;
private void OnDisable() => GameEventBus.OnEvent -= HandleEvent;  // 구독 해제 필수

private void HandleEvent(GameEvent e)
{
    foreach (var quest in GetActiveQuests())
    foreach (var cond in quest.conditions)
    {
        if (cond.eventType != e.Type)     continue;
        if (cond.targetId  != e.TargetId) continue;

        cond.currentCount += e.Value;
        if (quest.IsAllMet) CompleteQuest(quest);
    }
}

// CanComplete — switch 제거, 조건 테이블로 통합
public bool CanComplete(QuestSO quest) => quest.IsAllMet;
```

### PlayerInteractionHandler.cs (1줄 수정)
```csharp
// 기존
QuestManager.Instance.AddProgress(quest.questId);

// 변경
GameEventPublisher.NpcTalked(_currentNPC.Data.npcId);
```

---

## Phase 5 — 정리

### IQuestTarget.cs
```csharp
// 현재 미사용 → 바로 삭제하지 말고 Obsolete 마킹 후 안정화 확인 뒤 삭제
[Obsolete("EventBus로 대체됨. 삭제 예정")]
public interface IQuestTarget { }
```

---

## 작업 순서 요약

| Phase | 작업 | 기존 코드 수정 여부 |
|-------|------|-------------------|
| 1 | GameEventType / GameEvent / GameEventBus / GameEventPublisher 신규 | 없음 |
| 2 | QuestConditionData 신규 + QuestSO conditions 필드 추가 | QuestSO만 |
| 3 | QuestEventListener 신규 (도메인 이벤트 없으면 선행 추가) | 도메인 시스템 |
| 4 | QuestManager 구독 전환 + PlayerInteractionHandler 1줄 교체 | 2개 파일 |
| 5 | IQuestTarget Obsolete 마킹 | 1개 파일 |

---

## 영향 범위

| 파일 | 상태 | 작업 내용 |
|------|------|-----------|
| GameEventType.cs | 신규 | enum 정의 |
| GameEvent.cs | 신규 | 이벤트 데이터 클래스 |
| GameEventBus.cs | 신규 | static 이벤트 버스 |
| GameEventPublisher.cs | 신규 | 단일 발행 창구 |
| QuestConditionData.cs | 신규 | 조건 데이터 클래스 |
| QuestEventListener.cs | 신규 | 발행 책임 집중 |
| QuestSO.cs | 수정 | conditions 필드 추가 |
| QuestManager.cs | 수정 | 구독 전환, CanComplete 통합 |
| PlayerInteractionHandler.cs | 수정 | AddProgress → NpcTalked (1줄) |
| IQuestTarget.cs | 수정 | Obsolete 마킹 |
| NPCController.cs | 유지 | GetState/CanComplete/GetActiveTalkQuest 그대로 |
| DialogueSystem.cs | 유지 | 퀘스트와 무관 |

---

## 주의사항

- `OnEnable` / `OnDisable` 구독 해제 쌍 반드시 작성
- `GameEventBus.Publish()` 직접 호출 금지 — 반드시 `GameEventPublisher`를 통해서만
- 에디터 디버그 로그는 `#if UNITY_EDITOR` 로 감싸기
- Phase 3에서 도메인 시스템 네이티브 이벤트 유무 먼저 확인 후 진행
- 기존 동작 유지하면서 Phase 단위로 검증 후 다음 단계 진행
- `IQuestTarget` 삭제는 전체 안정화 확인 후 별도 커밋으로 처리
