# MMORPG 전투 시스템 - 데미지 파이프라인 구조 구현 요청

## 프로젝트 컨텍스트

* Unity C# 기반 MMORPG 프로젝트
* 전투 데미지 처리는 **Pipeline 패턴**으로 신규 구현
* 기존 구조를 다시 수정하지 않는 선에서 적용
* 파이프라인 최종 단계에서 기존 EventBus로 결과를 발행하여 연결

---

## 구현 목표

데미지가 아래 단계를 순서대로 통과하는 파이프라인 구조를 구현한다.

```
AttackRequest
    │
    ▼
\[1. HitCheckProcessor]          → 명중/회피 판정
    │
    ▼
\[2. CriticalCheckProcessor]     → 크리티컬 판정
    │
    ▼
\[3. DamageCalculateProcessor]   → 기본 데미지 + 속성 저항 계산
    │
    ▼
\[4. MitigationProcessor]        → 방어력 적용 (고정 피해는 무시)
    │
    ▼
\[5. ShieldProcessor]            → 실드 처리
    │
    ▼
\[6. HealthReduceProcessor]      → 실제 HP 차감
    │
    ▼
\[7. EventDispatchProcessor]     → EventBus로 결과 발행
```

---

## 구현 명세

### DamageContext

파이프라인을 흘러다니는 데이터 객체.

```csharp
public class DamageContext
{
    // 입력값
    public Character Attacker { get; set; }
    public Character Defender { get; set; }
    public float RawDamage { get; set; }
    public DamageType DamageType { get; set; }

    // 파이프라인을 거치며 채워지는 값
    public bool IsHit { get; set; } = true;
    public bool IsCritical { get; set; } = false;
    public float CritMultiplier { get; set; } = 1f;
    public float MitigatedAmount { get; set; } = 0f;
    public float FinalDamage { get; set; } = 0f;

    // 파이프라인 중단 플래그
    public bool IsCancelled { get; set; } = false;
}
```

---

### IDamageProcessor

모든 프로세서가 구현하는 인터페이스.

```csharp
public interface IDamageProcessor
{
    void Process(DamageContext ctx);
}
```

---

### 각 Processor 구현 요구사항

#### 1\. HitCheckProcessor

* `Attacker.Accuracy - Defender.Evasion` 으로 명중률 계산
* 빗나가면 `ctx.IsHit = false`, `ctx.IsCancelled = true` 설정

#### 2\. CriticalCheckProcessor

* `Attacker.CritRate`로 크리티컬 판정
* 크리티컬 시 `ctx.IsCritical = true`, `ctx.CritMultiplier = Attacker.CritDamage` 설정

#### 3\. DamageCalculateProcessor

* `RawDamage \* CritMultiplier` 계산
* `DamageType == Magic`이면 `Defender.MagicResist` 적용
* 결과를 `ctx.FinalDamage`에 저장

#### 4\. MitigationProcessor

* `DamageType == True`이면 방어 계산 스킵 (고정 피해)
* 방어 공식: `reduction = Defense / (Defense + 100f)`
* 감소량을 `ctx.MitigatedAmount`에, 적용 후 값을 `ctx.FinalDamage`에 저장

#### 5\. ShieldProcessor

* `Defender.ShieldAmount`가 없으면 스킵
* 실드가 데미지를 전부 막으면 `ctx.FinalDamage = 0`, `ctx.IsCancelled = true`
* 실드가 부분적으로 막으면 잔여 데미지를 `ctx.FinalDamage`에 저장

#### 6\. HealthReduceProcessor

* `ctx.Defender.StatHandler.ModifyHP(-ctx.FinalDamage)` 호출

#### 7\. EventDispatchProcessor

* 기존 EventBus에 `DamageTakenEvent` 발행
* 이벤트에 포함할 데이터: `Defender`, `FinalDamage`, `IsCritical`, `IsHit`

---

### DamagePipeline

```csharp
public class DamagePipeline
{
    private readonly List<IDamageProcessor> \_processors;

    public DamagePipeline()
    {
        \_processors = new List<IDamageProcessor>
        {
            new HitCheckProcessor(),
            new CriticalCheckProcessor(),
            new DamageCalculateProcessor(),
            new MitigationProcessor(),
            new ShieldProcessor(),
            new HealthReduceProcessor(),
            new EventDispatchProcessor()
        };
    }

    public DamageContext Execute(Character attacker, Character defender, float rawDamage, DamageType type)
    {
        var ctx = new DamageContext
        {
            Attacker   = attacker,
            Defender   = defender,
            RawDamage  = rawDamage,
            DamageType = type
        };

        foreach (var processor in \_processors)
        {
            if (ctx.IsCancelled) break;
            processor.Process(ctx);
        }

        return ctx;
    }
}
```

---

### 호출부 예시 (AttackHandler)

```csharp
public class AttackHandler : MonoBehaviour
{
    private DamagePipeline \_pipeline;

    void Start()
    {
        \_pipeline = new DamagePipeline();
    }

    public void Attack(Character target)
    {
        var ctx = \_pipeline.Execute(
            attacker:  this.character,
            defender:  target,
            rawDamage: stat.AttackPower,
            type:      DamageType.Physical
        );
    }
}
```

---

## DamageType 열거형

```csharp
public enum DamageType
{
    Physical,   // 물리: 방어력 적용
    Magic,      // 마법: 마법 저항 + 방어력 적용
    True        // 고정: 방어 무시
}
```

---

## 파일 구조 (권장)

```
Scripts/
└── Combat/
    ├── DamageContext.cs
    ├── DamageType.cs
    ├── DamagePipeline.cs
    ├── Interfaces/
    │   └── IDamageProcessor.cs
    └── Processors/
        ├── HitCheckProcessor.cs
        ├── CriticalCheckProcessor.cs
        ├── DamageCalculateProcessor.cs
        ├── MitigationProcessor.cs
        ├── ShieldProcessor.cs
        ├── HealthReduceProcessor.cs
        └── EventDispatchProcessor.cs
```

---

## 기존 시스템과의 연결 규칙

* `EventBus`는 기존 구현체를 그대로 사용할 것
* `DamageTakenEvent`는 신규 이벤트 클래스로 추가
* `StatHandler.ModifyHP()`는 기존 StatHandler의 메서드를 호출
* 파이프라인은 전투 로직에만 사용, 퀘스트/대화 흐름은 기존 EventBus 유지

