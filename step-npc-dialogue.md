# Step — NPC 상호작용 + 대화 시스템 (MVP 패턴)

프로젝트 네임스페이스 규칙을 참고해서
NPC 상호작용과 대화 시스템을 구현해줘.
네임스페이스는 MMORPG.

---

## 전제 조건

- PlayerController, PlayerStateMachine, PlayerInteractState 완성 상태
- UIManager, GameUIPanel, DialoguePanel(껍데기) 완성 상태
- NPCSO, DialogueSO, DialogueNode 완성 상태
- Find, GetComponent 반복 호출 금지
- MVP 패턴 적용 범위: DialogueSystem(Model) + DialoguePresenter + IDialogueView

---

## 생성 대상 및 위치

```
Assets/Scripts/
├── Core/
│   └── IDialogueView.cs                  -- View 인터페이스 (Core 레이어)
│
└── Game/
    ├── NPC/
    │   ├── NPCController.cs              -- NPCSO 보유, 상호작용 범위 감지
    │   └── NPCInteractionIndicator.cs    -- F키 아이콘 표시/숨김
    │
    ├── Dialogue/
    │   ├── DialogueSystem.cs             -- Model, 대화 흐름 제어
    │   └── DialoguePresenter.cs          -- Presenter, Model↔View 중재
    │
    └── UI/Panels/
        └── DialoguePanel.cs              -- View, IDialogueView 구현 (껍데기 교체)
```

---

## 각 파일 요구사항

### IDialogueView.cs (Core)
```
namespace MMORPG.Core

public interface IDialogueView
{
    void ShowDialogue(string speakerName, string text, Sprite portrait);
    void HideDialogue();
    void SetTypingComplete();   -- 타이핑 중 스킵 요청 시 호출
}
```

---

### NPCController.cs (Game/NPC)
```
namespace MMORPG.Game

public class NPCController : MonoBehaviour

[SerializeField] 필드:
- NPCSO _npcData
- float _interactRange = 2f
- NPCInteractionIndicator _indicator   -- 같은 GameObject 또는 자식에 있음

private 필드:
- Transform _playerTransform           -- Awake에서 찾지 말 것
                                          Start에서 PlayerController 태그로 한 번만 캐싱
                                          (GameObject.FindWithTag 허용 — Start에서 1회만)
- bool _playerInRange

public 프로퍼티:
- NPCSO Data => _npcData
- bool IsPlayerInRange => _playerInRange

이벤트:
- public event Action<NPCController> OnPlayerEnterRange
- public event Action<NPCController> OnPlayerExitRange

Update():
- _playerTransform이 null이면 return
- 거리 계산: Vector3.Distance(transform.position, _playerTransform.position)
- 범위 진입 시 (_playerInRange == false → 범위 안)
  _playerInRange = true
  _indicator.Show()
  OnPlayerEnterRange?.Invoke(this)
- 범위 이탈 시 (_playerInRange == true → 범위 밖)
  _playerInRange = false
  _indicator.Hide()
  OnPlayerExitRange?.Invoke(this)

public DialogueSO GetDialogueForCurrentState(QuestProgressState state):
- state 조건에 따라 반환할 DialogueSO 결정
  현재는 _npcData.defaultDialogue 반환 (QuestManager 연동은 이후 Step)

OnDrawGizmosSelected():
- Gizmos.color = Color.yellow
- Gizmos.DrawWireSphere(transform.position, _interactRange)
```

---

### NPCInteractionIndicator.cs (Game/NPC)
```
namespace MMORPG.Game

public class NPCInteractionIndicator : MonoBehaviour

[SerializeField] 필드:
- GameObject _iconRoot        -- F키 아이콘 루트 오브젝트
- TextMeshProUGUI _keyText    -- "F" 텍스트 (없으면 생략)

public void Show():
- _iconRoot.SetActive(true)

public void Hide():
- _iconRoot.SetActive(false)

Awake():
- Hide() 호출 (시작 시 숨김)

빌보드 효과 (항상 카메라를 바라봄):
LateUpdate():
- if (Camera.main == null) return
- transform.rotation = Camera.main.transform.rotation
  (Camera.main은 LateUpdate에서 1회 호출 — 캐싱 불필요한 수준)
```

---

### DialogueSystem.cs (Game/Dialogue) — Model
```
namespace MMORPG.Game

public class DialogueSystem : SingletonManager<DialogueSystem>

private 필드:
- DialogueSO _currentDialogue
- int _currentNodeIndex
- bool _isTyping                    -- 타이핑 중 여부
- CancellationTokenSource _cts      -- UniTask 취소용

public 프로퍼티:
- bool IsActive => _currentDialogue != null

이벤트:
- public event Action<DialogueNode> OnNodeChanged
- public event Action              OnTypingComplete
- public event Action              OnDialogueEnded

public void StartDialogue(DialogueSO dialogue, QuestProgressState state = QuestProgressState.None):
- _currentDialogue = dialogue
- _currentNodeIndex = 0
- 첫 번째 유효 노드 찾기 (requiredState 조건 체크)
- ShowCurrentNode()

public void Next():
- 타이핑 중이면 → _isTyping = false, OnTypingComplete 발행 후 return (스킵)
- 타이핑 완료 상태면 → 다음 유효 노드로 이동
  다음 노드가 없으면 EndDialogue()

public void EndDialogue():
- _cts?.Cancel()
- _currentDialogue = null
- OnDialogueEnded?.Invoke()

private void ShowCurrentNode():
- 현재 노드를 OnNodeChanged로 발행
- UniTask 타이핑 시작 (StartTypingAsync)

private async UniTaskVoid StartTypingAsync(DialogueNode node):
- _isTyping = true
- _cts = new CancellationTokenSource()
- OnNodeChanged?.Invoke(node)     -- Presenter가 타이핑 연출 시작
- 타이핑 완료 대기 (실제 연출은 Presenter/View에서 처리)
- _isTyping = false
- OnTypingComplete?.Invoke()

private DialogueNode? FindNextValidNode(QuestProgressState state):
- _currentNodeIndex 이후 노드 순서대로 탐색
- node.requiredState == None 또는 node.requiredState == state 이면 유효
- 없으면 null 반환
```

---

### DialoguePresenter.cs (Game/Dialogue) — Presenter
```
namespace MMORPG.Game

public class DialoguePresenter

생성자:
- DialoguePresenter(IDialogueView view, DialogueSystem model)
- 생성 시 model 이벤트 구독
  model.OnNodeChanged    += HandleNodeChanged
  model.OnTypingComplete += HandleTypingComplete
  model.OnDialogueEnded  += HandleDialogueEnded

public void Dispose():
- 이벤트 구독 해제

private void HandleNodeChanged(DialogueNode node):
- _view.ShowDialogue(node.speakerName, node.text, portrait)
  portrait는 NPCController에서 StartDialogue 시 전달받은 것 사용
  (생성자에서 Sprite portrait 추가 파라미터로 받아둠)
- 타이핑 연출 시작: _view에 타이핑 시작 요청
  (View가 UniTask 타이핑 처리)

private void HandleTypingComplete():
- 별도 처리 없음 (View가 알아서 완료 표시)

private void HandleDialogueEnded():
- _view.HideDialogue()

주의:
- Presenter는 MonoBehaviour가 아닌 순수 C# 클래스
- View를 IDialogueView 인터페이스로만 참조
```

---

### DialoguePanel.cs (Game/UI/Panels) — View
```
namespace MMORPG.Game

public class DialoguePanel : GameUIPanel, IDialogueView

[SerializeField] 필드:
- TextMeshProUGUI _speakerNameText
- TextMeshProUGUI _dialogueText
- Image _portraitImage
- float _typingSpeed = 0.03f        -- 글자 간 딜레이 (초)

private 필드:
- DialoguePresenter _presenter
- CancellationTokenSource _typingCts
- bool _isTypingComplete

OnOpen() override:
- _presenter = new DialoguePresenter(this, DialogueSystem.Instance)

OnClose() override:
- _presenter?.Dispose()
- _typingCts?.Cancel()

IDialogueView 구현:

void ShowDialogue(string speakerName, string text, Sprite portrait):
- _speakerNameText.text = speakerName
- _portraitImage.sprite = portrait
- _portraitImage.gameObject.SetActive(portrait != null)
- 타이핑 시작: StartTypingAsync(text) 호출

void HideDialogue():
- UIManager.Instance.ClosePanel(PanelType)

void SetTypingComplete():
- _typingCts?.Cancel()
- _isTypingComplete = true
- _dialogueText.text = _fullText     -- 전체 텍스트 즉시 표시

private async UniTaskVoid StartTypingAsync(string fullText):
- _fullText = fullText
- _isTypingComplete = false
- _dialogueText.text = ""
- _typingCts = new CancellationTokenSource()
- 한 글자씩 추가하며 await UniTask.Delay(typingSpeed)
- 완료 시 _isTypingComplete = true

Update():
- Space 또는 마우스 좌클릭 입력 감지
- DialogueSystem.Instance.IsActive가 false면 return
- DialogueSystem.Instance.Next() 호출
```

---

## NPC 상호작용 연결 — PlayerController 수정 사항

PlayerController.cs에 아래 내용을 추가해줘.

```
private 필드:
- NPCController _currentNPC    -- 범위 안에 있는 NPC

추가 메서드:
public void SetCurrentNPC(NPCController npc):
- _currentNPC = npc

public void ClearCurrentNPC():
- _currentNPC = null

Update()에 F키 입력 처리 추가:
- StateMachine.CurrentState가 PlayerInteractState면 return
- Input.GetKeyDown(KeyCode.F) && _currentNPC != null && _currentNPC.IsPlayerInRange
  → DialogueSO dialogue = _currentNPC.GetDialogueForCurrentState(QuestProgressState.None)
  → UIManager.Instance.OpenPanel<DialoguePanel>(UIPanelType.Dialogue, PanelOpenFlag.KeepPrevious)
  → DialogueSystem.Instance.StartDialogue(dialogue)
  → StateMachine.ChangeState(new PlayerInteractState(StateMachine))

DialogueSystem.OnDialogueEnded 구독 (Start 또는 OnEnable):
- DialogueSystem.Instance.OnDialogueEnded += OnDialogueEnded

private void OnDialogueEnded():
- StateMachine이 PlayerInteractState이면
  (StateMachine.CurrentState as PlayerInteractState)?.EndInteract()
```

---

## 씬 구성 안내

코드 생성 후 아래 내용을 단계별로 안내해줘.

1. NPC GameObject 구성
   - NPCController, NPCInteractionIndicator 부착 방법
   - NPCSO 연결 방법
   - F키 아이콘 World Space Canvas 구성 방법
     (NPC 머리 위 UI — Billboard 효과 포함)

2. DialoguePanel UI 구성
   - Canvas 하단 고정 레이아웃 구조
     (화면 하단 1/4 영역, 초상화 왼쪽 + 이름/대사 오른쪽)
   - TextMeshPro 컴포넌트 연결 방법
   - Portrait Image 설정 (NPC 초상화 없을 때 숨김 처리)

3. DialogueSO 데이터 작성 예시
   - ScriptableObject 생성 방법
   - DialogueNode 2~3개 체인 예시

4. 전체 흐름 테스트 방법
   - NPC 범위 진입 → F키 아이콘 표시 확인
   - F키 → 대화창 열림 + 타이핑 연출 확인
   - Space/클릭 → 다음 노드 또는 스킵 확인
   - 대화 종료 → 플레이어 Idle 복귀 확인

---

## 완료 조건

1. 지정된 경로에 모든 파일이 생성되어 있을 것
2. 네임스페이스가 MMORPG.Core / MMORPG.Game일 것
3. DialoguePanel이 IDialogueView를 구현할 것
4. DialoguePresenter가 MonoBehaviour가 아닌 순수 C# 클래스일 것
5. DialoguePresenter가 IDialogueView 인터페이스로만 View를 참조할 것
6. DialogueSystem이 View를 직접 참조하지 않을 것
7. 타이핑 중 스킵(Space/클릭 1회) → 전체 텍스트 즉시 표시가 동작할 것
8. 대화 종료 시 PlayerController가 Idle 상태로 복귀할 것
9. NPCController 범위 감지에 Physics.OverlapSphere 또는 OnTriggerEnter 미사용
   (Update에서 거리 계산 방식 유지)
10. Unity Editor에서 컴파일 에러가 없을 것
11. 각 파일 상단에 XML 주석으로 역할 설명이 있을 것

완료 후 MVP 각 레이어의 역할과
전체 대화 흐름(NPC 접근 → 대화 시작 → 종료)을 요약해줘.
