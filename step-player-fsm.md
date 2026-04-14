# Step — PlayerController (이동 + FSM + Cinemachine 카메라)

CLAUDE.md 또는 프로젝트 네임스페이스 규칙을 참고해서
플레이어 이동 시스템을 구현해줘.
네임스페이스는 MMORPG.

---

## 설계 원칙

- Find, GetComponent 반복 호출 금지 — 모든 참조는 [SerializeField] 또는 Awake 캐싱
- 플레이어 상태는 FSM으로 관리 — State 클래스를 분리해서 각 상태가 스스로 동작
- PlayerController는 상태 전환만 담당, 실제 동작은 각 State 클래스가 담당
- 전투(공격, 스킬, 데미지)는 이번 단계에서 구현하지 않음
- PlayerSO가 이미 Data 레이어에 존재한다고 가정

---

## 생성 대상 및 위치

```
Assets/Scripts/Game/Player/
├── PlayerController.cs          -- 진입점, FSM 소유, CharacterController 래핑
├── PlayerAnimator.cs            -- Animator 래핑, StringToHash 캐싱
├── FSM/
│   ├── PlayerStateMachine.cs    -- 상태 전환 관리
│   ├── PlayerStateBase.cs       -- 추상 베이스
│   ├── PlayerIdleState.cs
│   ├── PlayerMoveState.cs
│   └── PlayerInteractState.cs   -- NPC 대화 중 이동 차단용
```

---

## 각 파일 요구사항

### PlayerController.cs
```
namespace MMORPG.Game

public class PlayerController : MonoBehaviour, IDamageable

[SerializeField] 필드:
- PlayerSO _playerData
- PlayerAnimator _animator          -- 같은 GameObject에 있음, 인스펙터 연결
- Transform _cameraTransform        -- Cinemachine Brain이 붙은 MainCamera Transform

private 필드:
- CharacterController _cc           -- Awake에서 GetComponent (한 번만)
- PlayerStateMachine _stateMachine
- Vector3 _velocity                 -- 중력 누적용
- float _currentHp

프로퍼티:
- public PlayerStateMachine StateMachine => _stateMachine
- public PlayerAnimator Animator => _animator
- public PlayerSO Data => _playerData
- public CharacterController CC => _cc
- public Transform CameraTransform => _cameraTransform
- public bool IsGrounded => _cc.isGrounded

이벤트:
- public event Action<float, float> OnHpChanged   -- (currentHp, maxHp)
- public event Action OnDead

Awake():
- _cc = GetComponent<CharacterController>()
- _currentHp = _playerData.maxHp
- _stateMachine = new PlayerStateMachine(this)
- _stateMachine.ChangeState(new PlayerIdleState(_stateMachine))

Update():
- _stateMachine.Update()
- ApplyGravity()

private void ApplyGravity():
- isGrounded이면 _velocity.y = -2f (지면 고정)
- 아니면 _velocity.y += Physics.gravity.y * Time.deltaTime
- _cc.Move(_velocity * Time.deltaTime)

IDamageable 구현:
- void TakeDamage(float amount) -- _currentHp 감소, OnHpChanged 발행
  Dead 상태 전환은 이후 전투 Step에서 추가 예정 (주석으로 명시)
- bool IsDead => _currentHp <= 0

public void SetVelocityY(float y):
- _velocity.y = y  (점프 등 외부에서 y velocity 제어용)
```

### PlayerAnimator.cs
```
namespace MMORPG.Game

public class PlayerAnimator : MonoBehaviour

[SerializeField]:
- Animator _animator    -- 인스펙터 연결

StringToHash 캐싱 (private static readonly):
- Speed    (float)
- IsInteracting (bool)

public 메서드:
- void SetSpeed(float speed)
- void SetInteracting(bool value)
- void Play(string stateName)        -- 직접 재생이 필요한 경우용
```

### PlayerStateMachine.cs
```
namespace MMORPG.Game

public class PlayerStateMachine

생성자:
- PlayerStateMachine(PlayerController owner)
- public PlayerController Owner { get; }

필드:
- PlayerStateBase _currentState
- public PlayerStateBase CurrentState => _currentState

메서드:
- public void ChangeState(PlayerStateBase newState)
  _currentState?.Exit()
  _currentState = newState
  _currentState.Enter()

- public void Update()
  _currentState?.Update()
```

### PlayerStateBase.cs
```
namespace MMORPG.Game

public abstract class PlayerStateBase

생성자:
- protected PlayerStateBase(PlayerStateMachine stateMachine)
- protected PlayerStateMachine StateMachine { get; }
- protected PlayerController Owner => StateMachine.Owner

추상 메서드:
- public abstract void Enter()
- public abstract void Update()
- public abstract void Exit()
```

### PlayerIdleState.cs
```
namespace MMORPG.Game

public class PlayerIdleState : PlayerStateBase

Enter():
- Owner.Animator.SetSpeed(0f)

Update():
- 입력 감지: Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")
- 입력이 있으면 → StateMachine.ChangeState(new PlayerMoveState(StateMachine))

Exit(): 비어있어도 됨
```

### PlayerMoveState.cs
```
namespace MMORPG.Game

public class PlayerMoveState : PlayerStateBase

Enter():
- 없음

Update():
- horizontal = Input.GetAxisRaw("Horizontal")
- vertical   = Input.GetAxisRaw("Vertical")

- 입력이 없으면 → StateMachine.ChangeState(new PlayerIdleState(StateMachine))

- 카메라 기준 방향 계산:
  Vector3 camForward = Owner.CameraTransform.forward;
  Vector3 camRight   = Owner.CameraTransform.right;
  camForward.y = 0f; camForward.Normalize();
  camRight.y   = 0f; camRight.Normalize();
  Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;

- CharacterController 이동:
  Owner.CC.Move(moveDir * Owner.Data.moveSpeed * Time.deltaTime);

- 캐릭터 회전 (이동 방향으로 Slerp):
  if (moveDir != Vector3.zero)
  {
      Quaternion targetRot = Quaternion.LookRotation(moveDir);
      Owner.transform.rotation = Quaternion.Slerp(
          Owner.transform.rotation,
          targetRot,
          Owner.Data.rotationSpeed * Time.deltaTime);
  }

- Animator 업데이트:
  float speed = new Vector2(horizontal, vertical).magnitude;
  Owner.Animator.SetSpeed(speed);

Exit():
- Owner.Animator.SetSpeed(0f)
```

### PlayerInteractState.cs
```
namespace MMORPG.Game

public class PlayerInteractState : PlayerStateBase

Enter():
- Owner.Animator.SetInteracting(true)
- Owner.Animator.SetSpeed(0f)

Update():
- 이동 입력 무시 (아무것도 하지 않음)
- 상태 전환은 외부(DialogueSystem 종료 이벤트)에서만 가능

Exit():
- Owner.Animator.SetInteracting(false)

public void EndInteract():
- StateMachine.ChangeState(new PlayerIdleState(StateMachine))
  (DialogueSystem이 대화 종료 시 이 메서드 호출)
```

---

## PlayerSO 참고 (이미 존재)

```csharp
// 이미 Data 레이어에 있는 PlayerSO에 아래 필드가 없으면 추가해줘
public float rotationSpeed = 10f;   // 캐릭터 회전 속도
```

---

## Cinemachine 설정 안내

코드 생성 후 아래 씬 구성 방법을 단계별로 안내해줘.

1. 플레이어 GameObject 구성
   - CharacterController 권장 height / radius / center 설정값
   - PlayerController, PlayerAnimator 부착 순서
   - PlayerSO 연결 방법
   - _cameraTransform에 MainCamera Transform 연결 방법

2. Cinemachine 탑다운 3인칭 카메라 설정
   - CinemachineVirtualCamera 추가 방법
   - Body: Transposer 권장 offset (탑다운 MMORPG 시점)
     예: Follow Offset (0, 8, -5) 전후 수치 안내
   - Aim: Composer 또는 HardLockToTarget 중 권장안 제시
   - Follow / LookAt → 플레이어 Transform 연결
   - 카메라가 플레이어를 따라가되 회전은 고정되도록 설정

3. 레이어 / 태그 설정
   - Player 태그 및 레이어 생성

4. FSM 동작 테스트 방법
   - 씬 Play 후 Idle → Move → Idle 전환 확인 방법
   - 현재 State를 인스펙터에서 확인하는 방법 (Debug용 [SerializeField] 또는 커스텀 에디터)

---

## 완료 조건

1. 지정된 경로에 모든 파일이 생성되어 있을 것
2. 네임스페이스가 MMORPG.Game일 것
3. PlayerController가 IDamageable을 구현할 것
4. PlayerController 내부에 GetComponent 반복 호출이 없을 것
   (Awake에서 한 번만 캐싱)
5. 이동 방향이 카메라 기준으로 계산될 것
6. PlayerInteractState에서 이동 입력이 차단될 것
7. Animator 파라미터가 Animator.StringToHash로 캐싱될 것
8. PlayerSO.rotationSpeed가 회전에 실제로 사용될 것
9. Unity Editor에서 컴파일 에러가 없을 것
10. 각 파일 상단에 XML 주석으로 역할 설명이 있을 것

완료 후 FSM 상태 전환 흐름도와
PlayerController의 public 인터페이스를 요약해줘.
