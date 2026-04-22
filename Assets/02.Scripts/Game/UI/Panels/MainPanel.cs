/// <summary>
/// 메인 HUD. 플레이어 HP바·EXP바를 표시한다.
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MMORPG.Game
{
    public class MainPanel : GameUIPanel
    {
        [Header("HP")]
        [SerializeField] private Image      _hpFill;
        [SerializeField] private TMP_Text   _hpText;   // "100 / 200" 형식, 없으면 생략

        [Header("EXP")]
        [SerializeField] private Image      _expFill;  // EXP 시스템 구현 전까지 fillAmount = 0

        private StatHandler _playerStat;

        // ── 패널 열릴 때 플레이어 StatHandler에 구독 ─────────────────

        public override void OnOpen()
        {
            var player = PlayerRegistry.Player;
            if (player == null) return;

            _playerStat = player.GetComponent<StatHandler>();
            _playerStat.OnHpChanged += OnHpChanged;

            RefreshHP(_playerStat.CurrentHp, _playerStat.MaxHp);

            if (_expFill != null) _expFill.fillAmount = 0.5f;
        }

        public override void OnClose()
        {
            if (_playerStat != null)
                _playerStat.OnHpChanged -= OnHpChanged;
        }

        // ── 이벤트 핸들러 ─────────────────────────────────────────────

        private void OnHpChanged(float current, float max) => RefreshHP(current, max);

        private void RefreshHP(float current, float max)
        {
            if (_hpFill != null) _hpFill.fillAmount = max > 0f ? current / max : 0f;
            if (_hpText != null) _hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }
}
