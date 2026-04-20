using System;
using UnityEngine;
using MMORPG.Data;

namespace MMORPG.Game
{
    public class StatHandler : MonoBehaviour
    {
        [SerializeField] private CharacterStatSO _stat;

        private float _currentHp;

        public float CurrentHp => _currentHp;
        public float MaxHp     => _stat.maxHp;
        public bool  IsDead    => _currentHp <= 0f;

        public event Action<float, float> OnHpChanged;  // (currentHp, maxHp)
        public event Action               OnDead;

        private void Awake() => _currentHp = _stat.maxHp;

        public void ModifyHP(float delta)
        {
            if (IsDead) return;
            _currentHp = Mathf.Clamp(_currentHp + delta, 0f, _stat.maxHp);
            OnHpChanged?.Invoke(_currentHp, _stat.maxHp);
            if (IsDead) OnDead?.Invoke();
        }
    }
}
