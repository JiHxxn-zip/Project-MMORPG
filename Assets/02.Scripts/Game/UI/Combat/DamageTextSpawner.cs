using UnityEngine;

namespace MMORPG.Game
{
    /// <summary>
    /// CombatEventBus를 구독해 피격 시 데미지 텍스트를 스폰하는 싱글톤.
    /// 씬의 빈 GameObject에 부착한다.
    /// </summary>
    public class DamageTextSpawner : MonoBehaviour
    {
        public static DamageTextSpawner Instance { get; private set; }

        [SerializeField] private DamageText _damageTextPrefab;

        // 같은 위치에 여러 텍스트가 겹치지 않도록 약간 흩뿌린다
        private static readonly Vector3 SpawnOffset = new Vector3(0f, 1.8f, 0f);

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()  => CombatEventBus.OnDamageTaken += OnDamageTaken;
        private void OnDisable() => CombatEventBus.OnDamageTaken -= OnDamageTaken;

        private void OnDamageTaken(DamageTakenEvent e)
        {
            if (e.Defender == null) return;

            Vector3 spawnPos = e.Defender.transform.position + SpawnOffset
                             + new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);

            var text = Instantiate(_damageTextPrefab, spawnPos, Quaternion.identity);
            text.Spawn(e.FinalDamage, e.IsCritical);
        }
    }
}
