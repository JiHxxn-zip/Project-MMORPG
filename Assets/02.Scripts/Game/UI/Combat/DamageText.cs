using System.Collections;
using TMPro;
using UnityEngine;

namespace MMORPG.Game
{
    /// <summary>
    /// 스폰 직후 위로 떠오르며 페이드 아웃되는 데미지 텍스트.
    /// DamageTextSpawner가 Instantiate 후 Spawn()을 호출한다.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageText : MonoBehaviour
    {
        [SerializeField] private float _riseSpeed  = 1.5f;
        [SerializeField] private float _duration   = 0.8f;

        private static readonly Color NormalColor   = Color.white;
        private static readonly Color CriticalColor = new Color(1f, 0.4f, 0f); // 주황

        private TextMeshPro _tmp;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
        }

        public void Spawn(float damage, bool isCritical)
        {
            _tmp.text     = Mathf.RoundToInt(damage).ToString();
            _tmp.color    = isCritical ? CriticalColor : NormalColor;
            _tmp.fontSize = isCritical ? 5f : 3.5f;

            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Color   startColor = _tmp.color;

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _duration;

                transform.position = startPos + Vector3.up * (_riseSpeed * t);
                _tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
