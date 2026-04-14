# MMORPG Unity — Project Architecture Rules

## 프로젝트 개요

- **목표**: 마비노기 모바일을 레퍼런스로 한 Unity ARPG 모작
- **엔진**: Unity 2022 LTS / C# / URP
- **핵심 설계 철학**: 레이어 의존 방향을 강제하고, 정의 데이터(SO)와 런타임 상태(JSON)를 분리한다

---

## 레이어 구조 (Assembly Definition 기준)

아래 세 레이어만 존재한다. 의존 방향은 단방향이며 역방향 참조는 허용하지 않는다.

```
Core  ←  Data  ←  Game
```

### Core (MMORPG.Core.asmdef)
- **위치**: `Assets/02.Scripts/Core/`
- **역할**: 인터페이스, Enum, 이벤트 정의만 포함. 구현 코드 없음
- **외부 의존**: 없음
- **포함 대상**
  - `IDamageable` — TakeDamage(float), IsDead
  - `IInteractable` — Interact(GameObject)
  - `IQuestTarget` — string TargetId { get; }
  - `QuestProgressState` enum — None / Available / Active / Completed / Failed
  - `QuestType` enum — KillMonster / TalkToNPC / CollectItem / Reach
  - `SkillType` enum — Melee / Ranged / AoE

### Data (MMORPG.Data.asmdef)
- **위치**: `Assets/02.Scripts/Data/`
- **역할**: ScriptableObject 데이터 컨테이너. 로직 없음
- **외부 의존**: Core만 참조
- **포함 대상**
  - `PlayerSO` — moveSpeed, maxHp, attackPower, attackRange, List\<SkillSO\> skills
  - `SkillSO` — skillId, skillType, damage, cooldown, range, effectPrefab(Addressable key)
  - `QuestSO` — questId, title, description, questType, targetId, targetCount, prerequisiteQuestId, reward, acceptDialogue, completeDialogue
  - `NPCSO` — npcId, npcName, portrait, defaultDialogue, List\<QuestSO\> availableQuests
  - `DialogueSO` — dialogueId, List\<DialogueNode\> nodes
  - `DialogueNode` (struct) — speakerName, text, requiredState(QuestProgressState), nextDialogueId

### Game (MMORPG.Game.asmdef)
- **위치**: `Assets/02.Scripts/Game/`
- **역할**: 실제 게임 로직 구현
- **외부 의존**: Core + Data 참조
- **포함 대상**: QuestManager, DialogueSystem, PlayerController, CombatSystem, NPCController, QuestTrackerUI

---

## Addressables 키 네이밍 규칙

모든 Addressable 키는 아래 형식을 따른다. questId와 키를 동일하게 맞춰
QuestManager가 questId 하나로 SO / JSON / Dialogue를 동시에 로드할 수 있도록 한다.

```
"quest/{questId}"          → QuestSO
"quest/{questId}_state"    → quest 런타임 JSON
"npc/{npcId}"              → NPCSO
"dialogue/{dialogueId}"    → DialogueSO
"skill/{skillId}"          → SkillSO
"fx/{effectId}"            → 스킬 이펙트 Prefab
"preload/player"           → PlayerSO (게임 시작 시 사전 로드)
```

---

## SO vs JSON 역할 분리 원칙

| 구분 | 저장 방식 | 예시 |
|------|-----------|------|
| 불변 정의 데이터 | ScriptableObject | 퀘스트 제목, NPC 이름, 스킬 수치 |
| 런타임 상태 데이터 | JSON (Addressables) | 퀘스트 진행 상태, 현재 킬 카운트 |

SO는 에디터에서 편집, JSON은 세이브/로드 및 서버 동기화 대상이다.
절대로 SO에 런타임 상태(진행도, 현재 수량 등)를 저장하지 않는다.

---

## 코딩 컨벤션

- 클래스명: PascalCase
- 변수명: camelCase (private는 `m_` 접두사 없이 `_camelCase`)
- 인터페이스명: `I` 접두사 (IDamageable)
- SO 클래스명: 타입 + SO 접미사 (QuestSO, NPCSO)
- Enum값: PascalCase
- 이벤트/Action: `On` 접두사 (OnQuestUpdated, OnQuestCompleted)
- 상수: UPPER_SNAKE_CASE

---

## 현재 구현 완료 범위

- [ ] Assembly Definition 세팅 (진행 중)
- [ ] Core 레이어 인터페이스 / Enum 정의
- [ ] Data 레이어 SO 클래스 작성
- [ ] Addressables 그룹 구성
- [ ] PlayerController (이동 + 기본 공격 + 스킬 1~2개)
- [ ] QuestManager (JSON Data-Driven)
- [ ] NPC 대화 시스템 (DialogueSO 체인)
- [ ] 퀘스트 트래커 UI

---

## 지금 요청 — Assembly Definition 세팅

아래 세 가지 `.asmdef` 파일을 생성한다.

### 1. MMORPG.Core.asmdef
```json
{
  "name": "MMORPG.Core",
  "rootNamespace": "MMORPG.Core",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": false,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```
- **저장 위치**: `Assets/Scripts/Core/MMORPG.Core.asmdef`
- **noEngineReferences: false** — IDamageable 등에서 UnityEngine 타입을 쓸 수 있도록

### 2. MMORPG.Data.asmdef
```json
{
  "name": "MMORPG.Data",
  "rootNamespace": "MMORPG.Data",
  "references": [
    "GUID:MMORPG.Core"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": false,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```
- **저장 위치**: `Assets/Scripts/Data/MMORPG.Data.asmdef`
- Core만 참조. Game을 참조하지 않는다

### 3. MMORPG.Game.asmdef
```json
{
  "name": "MMORPG.Game",
  "rootNamespace": "MMORPG.Game",
  "references": [
    "GUID:MMORPG.Core",
    "GUID:MMORPG.Data"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": false,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```
- **저장 위치**: `Assets/02.Scripts/Game/MMORPG.Game.asmdef`
- Core + Data 참조. 역방향 참조 금지

---

## 완료 조건 (이 작업의 Done 기준)

1. 세 폴더(`Core/`, `Data/`, `Game/`)가 생성되어 있다 ✅
2. 각 폴더에 `.asmdef` 파일이 존재한다 ✅
3. Unity Editor에서 컴파일 에러가 없다
4. Data가 Game을 참조하거나 Core가 Data를 참조하는 역방향 의존이 없다 ✅
5. 각 폴더에 namespace를 맞춘 빈 placeholder 스크립트가 1개씩 존재한다 ✅

```csharp
// Assets/02.Scripts/Core/CorePlaceholder.cs
namespace MMORPG.Core { }

// Assets/02.Scripts/Data/DataPlaceholder.cs
namespace MMORPG.Data { }

// Assets/02.Scripts/Game/GamePlaceholder.cs
namespace MMORPG.Game { }
```
