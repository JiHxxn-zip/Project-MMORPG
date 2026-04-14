# Step UI — UIManager 시스템 구축

CLAUDE.md의 레이어 구조와 코딩 컨벤션을 참고해서
UIManager 시스템을 구현해줘.
네임스페이스는 MMORPG (MabiLike 아님).

---

## 설계 원칙

- Find, GetComponent 반복 호출 금지 — 모든 참조는 [SerializeField] 또는 캐싱
- UIManager는 UI 생성/활성화/스택 관리만 담당 — 카메라·게임로직 직접 참조 금지
- 카메라 제어 등 외부 연동은 이벤트(Action) 구독 방식으로 분리
- 패널은 SO가 Prefab을 들고 있고, 캐시에 없으면 Instantiate, 있으면 SetActive로 재사용
- 패널 열기 옵션은 PanelOpenFlag 비트 플래그로 처리 (bool 파라미터 나열 금지)

---

## 생성 대상 및 위치

### Core 레이어 (Assets/Scripts/Core/)
- UIPanelType.cs
- PanelOpenFlag.cs

### Data 레이어 (Assets/Scripts/Data/)
- UIPanelDataSO.cs
- UIPanelEntry.cs

### Game 레이어 (Assets/Scripts/Game/UI/)
- GameUIPanel.cs
- UIManager.cs

### Game 레이어 — 패널 (Assets/Scripts/Game/UI/Panels/)
- MainPanel.cs
- LogoPanel.cs
- DialoguePanel.cs       (대화 시스템 연동용 — 내부 로직은 Step 6에서 채움)
- QuestTrackerPanel.cs   (퀘스트 트래커용 — 내부 로직은 Step 7에서 채움)
- LoadingPanel.cs

---

## 각 파일 요구사항

### UIPanelType.cs (Core)
```
namespace MMORPG.Core

public enum UIPanelType
{
    None = 0,
    Logo,
    Main,
    Dialogue,
    QuestTracker,
    Loading,
}
```

### PanelOpenFlag.cs (Core)
```
namespace MMORPG.Core

[System.Flags]
public enum PanelOpenFlag
{
    None         = 0,
    KeepPrevious = 1 << 0,  // 이전 패널 유지 (현재 패널 위에 쌓기)
    AlwaysOnTop  = 1 << 1,  // Stack 무관하게 별도 Root에 표시 (Loading 등)
    ClearStack   = 1 << 2,  // 열기 전 Stack 전부 닫기 (Main 귀환 시)
}
```

### UIPanelEntry.cs (Data)
```
namespace MMORPG.Data

[System.Serializable]
public struct UIPanelEntry
{
    public UIPanelType panelType;
    public GameUIPanel prefab;
}
```

### UIPanelDataSO.cs (Data)
```
namespace MMORPG.Data

[CreateAssetMenu(menuName = "MMORPG/Data/UIPanelData")]
public class UIPanelDataSO : ScriptableObject

필드:
- [SerializeField] List<UIPanelEntry> entries

메서드:
- public bool TryGetPrefab(UIPanelType type, out GameUIPanel prefab)
  entries를 순회해서 해당 type의 prefab 반환
  없으면 false + Debug.LogError 출력
```

### GameUIPanel.cs (Game)
```
namespace MMORPG.Game

public abstract class GameUIPanel : MonoBehaviour

필드:
- [SerializeField] private UIPanelType _panelType
- public UIPanelType PanelType => _panelType

메서드:
- public virtual void OnOpen() { }   -- 패널이 열릴 때 UIManager가 호출
- public virtual void OnClose() { }  -- 패널이 닫힐 때 UIManager가 호출
- public void Close()                -- 외부(닫기 버튼 등)에서 호출
  → UIManager.Instance.ClosePanel(PanelType) 위임
```

### UIManager.cs (Game)
```
namespace MMORPG.Game

public class UIManager : SingletonManager<UIManager>

[SerializeField] 필드:
- UIPanelDataSO _panelData
- Transform _panelRoot           -- 일반 패널 부모
- Transform _alwaysOnTopRoot     -- AlwaysOnTop 패널 부모 (Loading 등)
- CanvasScaler _canvasScaler     -- 직접 연결, GetComponent 반복 금지

private 필드:
- Dictionary<UIPanelType, GameUIPanel> _panelCache   -- 생성된 패널 캐시
- Stack<UIPanelType> _panelStack                     -- 패널 히스토리

이벤트:
- public event Action<UIPanelType> OnPanelOpened
- public event Action<UIPanelType> OnPanelClosed

public 메서드:

1. T OpenPanel<T>(UIPanelType type, PanelOpenFlag flags = PanelOpenFlag.None) where T : GameUIPanel
   동작 순서:
   a. TryGetOrCreate로 패널 인스턴스 확보
   b. ClearStack 플래그 있으면 CloseAllStacked() 호출
   c. KeepPrevious 없으면 현재 스택 최상단 패널 SetActive(false)
   d. AlwaysOnTop 있으면 _alwaysOnTopRoot로 부모 변경, 스택 미포함
   e. SetActive(true), SetAsLastSibling, OnOpen() 호출
   f. AlwaysOnTop 아니면 _panelStack.Push(type)
   g. OnPanelOpened 이벤트 발행
   h. 패널 반환

2. void ClosePanel(UIPanelType type)
   동작 순서:
   a. 캐시에서 패널 찾기
   b. OnClose() 호출, SetActive(false)
   c. RemoveFromStack(type)
   d. PeekAndSetActive(true) — 이전 패널 복원
   e. OnPanelClosed 이벤트 발행

3. void CloseCurrent()
   → 스택 최상단 패널 ClosePanel 호출

4. void GoBack()
   → CloseCurrent와 동일 (뒤로가기 버튼용 명시적 메서드)

5. bool IsPanelOpen(UIPanelType type)
   → 캐시에 있고 activeInHierarchy이면 true

6. void RefreshDisplaySize()
   해상도 대응:
   - standardRatio = 1080f / 1920f
   - currentRatio = Screen.height / Screen.width
   - currentRatio > standard → matchWidthOrHeight = 0f
   - currentRatio < standard → matchWidthOrHeight = 1f
   - 동일 → 0.5f
   - _canvasScaler.matchWidthOrHeight에 적용

private 메서드:

- bool TryGetOrCreate<T>(UIPanelType type, out T panel) where T : GameUIPanel
  캐시에 있으면 반환, 없으면 SO에서 Prefab 조회 후 Instantiate
  Instantiate 후 SetActive(false) 상태로 캐시 등록
  실패 시 Debug.LogError + return false

- void PeekAndSetActive(bool active)
  스택이 비어있으면 return
  Peek()한 타입의 캐시 패널 SetActive(active)

- void CloseAllStacked()
  스택 전부 Pop하며 OnClose() + SetActive(false)

- void RemoveFromStack(UIPanelType type)
  Stack에서 특정 타입만 제거
  (Stack을 List로 복사 → 해당 타입 제거 → 역순으로 다시 Push)

override:
- public override void Initialize()
  _panelCache.Clear(), _panelStack.Clear(), RefreshDisplaySize()

#if UNITY_EDITOR
- private void Update() => RefreshDisplaySize()
#endif
```

### 패널 클래스들 (Panels/)
```
MainPanel, LogoPanel, DialoguePanel, QuestTrackerPanel, LoadingPanel

공통 규칙:
- namespace MMORPG.Game
- GameUIPanel 상속
- [SerializeField] private UIPanelType _panelType 값은 각자 맞는 enum으로 설정
- OnOpen(), OnClose() override (현재는 빈 구현 — 로직은 이후 Step에서 채움)
- 클래스 상단 XML 주석으로 역할 한 줄 설명

DialoguePanel 추가 사항:
- 추후 DialogueSystem과 연동할 것임을 주석으로 명시
- [SerializeField] GameObject dialogueContent (자리만 잡아둠)

QuestTrackerPanel 추가 사항:
- 추후 QuestManager와 연동할 것임을 주석으로 명시
- [SerializeField] Transform trackerContainer (자리만 잡아둠)
```

---

## SingletonManager 참고

이미 프로젝트에 SingletonManager<T> 베이스 클래스가 있다고 가정한다.
없으면 아래 구조로 생성해줘:

```csharp
// Assets/Scripts/Core/SingletonManager.cs
namespace MMORPG.Core

public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    public virtual void Initialize() { }
}
```

---

## 씬 구성 안내

코드 생성 후 아래 내용을 단계별로 안내해줘.

1. UIManager GameObject 구성
   - 컴포넌트 부착 방법
   - _panelRoot, _alwaysOnTopRoot Transform 구성 (Canvas 하위 계층 구조)
   - CanvasScaler 직접 연결 방법
   - UIPanelDataSO 생성 및 연결 방법

2. UIPanelDataSO 설정
   - SO 생성 위치 (Assets/Data/UI/)
   - 각 Panel Prefab 등록 방법
   - Prefab 생성 시 PanelType 인스펙터 설정 방법

3. 동작 테스트 방법
   - 씬 시작 시 LogoPanel 자동 열기 예시 코드
   - Main으로 전환 시 ClearStack 예시
   - Loading 패널 AlwaysOnTop 동작 확인 방법

---

## 완료 조건

1. 지정된 경로에 모든 파일이 생성되어 있을 것
2. 모든 타입이 올바른 네임스페이스(MMORPG.Core / .Data / .Game)에 있을 것
3. UIManager 내부에 Find, GetComponent 반복 호출이 없을 것
4. UIManager가 카메라나 GameManager를 직접 참조하지 않을 것
5. PanelOpenFlag 비트 조합이 정상 동작할 것
6. AlwaysOnTop 패널이 _panelStack에 포함되지 않을 것
7. TryGetOrCreate에서 캐시 히트 시 Instantiate가 발생하지 않을 것
8. Unity Editor에서 컴파일 에러가 없을 것
9. 각 파일 상단에 XML 주석이 있을 것

완료 후 UIManager의 전체 public 인터페이스와
각 PanelOpenFlag 조합 사용 예시를 요약해줘.
