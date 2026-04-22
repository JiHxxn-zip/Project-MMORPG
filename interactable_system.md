# IInteractable 시스템 구현 요청

## 구현 목표

유저가 NPC 또는 Monster를 터치했을 때:

* **NPC** → 대화 시작 (`DialogueHandler.Instance.StartDialogue(ctx)`)
* **Monster** → 타겟 지정 (`TargetingSystem.Instance.SetTarget(...)`)

터치 감지 로직은 `TouchDetector` 단일 컴포넌트가 담당하고,
실제 동작은 `IInteractable`을 구현한 각 컨트롤러가 처리한다.

\---

## 전체 구조

```
IInteractable (인터페이스)
    ├── NPCController.OnInteract()      → DialogueHandler 호출
    └── MonsterController.OnInteract()  → TargetingSystem 호출

NPC GameObject
├── NPCController     (IInteractable 구현 — 기존 파일 수정)
└── TouchDetector     (신규 추가)

Monster GameObject
├── MonsterController (IInteractable 구현 — 기존 파일 수정)
└── TouchDetector     (신규 추가)
```

\---

## 구현 명세

### 1\. IInteractable 인터페이스 (신규)

```csharp
namespace MMORPG.Core
{
    public interface IInteractable
    {
        void OnInteract();
    }
}
```

\---

### 2\. TouchDetector (신규)

같은 GameObject에서 `IInteractable` 구현체를 찾아 `OnInteract()`를 호출한다.
NPC/Monster 어디에 붙어도 코드 변경 없이 동작해야 한다.

```csharp
namespace MMORPG.Game
{
    public class TouchDetector : MonoBehaviour
    {
        private IInteractable \_interactable;

        void Awake()
        {
            \_interactable = GetComponent<IInteractable>();
        }

        void OnMouseDown()
        {
            \_interactable?.OnInteract();
        }
    }
}
```

\---

### 3\. NPCController 수정 (기존 파일)

`IInteractable` 구현 추가. `GetInteractionContext()` 기존 로직은 그대로 유지한다.

```csharp
public class NPCController : MonoBehaviour, IInteractable
{
    // 기존 필드/프로퍼티 그대로 유지

    public void OnInteract()
    {
        var ctx = GetInteractionContext();
        DialogueHandler.Instance.StartDialogue(ctx);
    }

    // GetInteractionContext() 기존 코드 그대로 유지
}
```

\---

### 4\. MonsterController 수정 (기존 파일)

`IInteractable` 구현 추가. 기존 AI 로직(`Update`, 상태머신 등)은 전혀 건드리지 않는다.

```csharp
public class MonsterController : MonoBehaviour, IInteractable
{
    // 기존 코드 전부 유지

    public void OnInteract()
    {
        TargetingSystem.Instance.SetTarget(\_character);
    }
}
```

\---

## 파일 구조

```
Scripts/
├── Core/
│   └── IInteractable.cs          (신규)
└── Game/
    ├── NPC/
    │   └── NPCController.cs      (IInteractable 추가 — 기존 수정)
    ├── Monster/
    │   └── MonsterController.cs  (IInteractable 추가 — 기존 수정)
    └── Common/
        └── TouchDetector.cs      (신규)
```

\---

## 작업 범위 요약

|파일|작업|
|-|-|
|`IInteractable.cs`|신규 생성|
|`TouchDetector.cs`|신규 생성|
|`NPCController.cs`|`, IInteractable` 추가 + `OnInteract()` 메서드 추가|
|`MonsterController.cs`|`, IInteractable` 추가 + `OnInteract()` 메서드 추가|

## 주의사항

* `NPCController`의 `GetInteractionContext()` 로직은 수정하지 않는다
* `MonsterController`의 기존 상태머신 및 AI 로직은 수정하지 않는다
* `TouchDetector`는 NPC/Monster 전용이 아닌 범용 컴포넌트로 작성한다
* `DialogueHandler`, `TargetingSystem`은 이미 구현된 싱글톤이므로 새로 만들지 않는다

