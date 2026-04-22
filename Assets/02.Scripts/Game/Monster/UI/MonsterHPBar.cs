using UnityEngine;
using UnityEngine.UI;

namespace MMORPG.Game
{
    /// <summary>
    /// 몬스터 월드 UI HP바.
    /// 몬스터 프리팹 자식 오브젝트에 부착하고, StatHandler 이벤트를 구독해 자동 갱신한다.
    /// </summary>
    public class MonsterHPBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;

        private StatHandler _statHandler;
        private Transform   _cameraTransform;

        private void Awake()
        {
            _statHandler = GetComponentInParent<StatHandler>();
        }

        private void Start()
        {
            _cameraTransform = Camera.main.transform;

            _statHandler.OnHpChanged += OnHpChanged;

            // 초기값 반영
            _fillImage.fillAmount = _statHandler.CurrentHp / _statHandler.MaxHp;
        }

        private void OnDestroy()
        {
            if (_statHandler != null)
                _statHandler.OnHpChanged -= OnHpChanged;
        }

        private void LateUpdate()
        {
            // 카메라를 향해 Billboard 회전
            transform.forward = _cameraTransform.forward;
        }

        private void OnHpChanged(float currentHp, float maxHp)
        {
            _fillImage.fillAmount = currentHp / maxHp;
        }
    }
}
