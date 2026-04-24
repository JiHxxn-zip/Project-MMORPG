using System;
using UnityEngine;

namespace MMORPG.Game
{
    /// <summary>
    /// 플레이어의 현재 전투 타겟을 관리하는 싱글톤.
    /// 씬에 배치하지 않아도 최초 접근 시 자동으로 생성된다.
    /// </summary>
    public class TargetingSystem : MonoBehaviour
    {
        private static TargetingSystem _instance;
        private static bool _isQuitting = false;
        public static TargetingSystem Instance
        {
            get
            {
                // 앱 종료 중이면 새로 만들지 않음
                if (_isQuitting) return null;

                if (_instance == null)
                {
                    var go = new GameObject(nameof(TargetingSystem));
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<TargetingSystem>();
                }
                return _instance;
            }
        }
        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            _isQuitting = true;
        }

        public Character CurrentTarget { get; private set; }

        // 타겟 마커/포커스 이펙트 or 스킬 사거리 체크
        public event Action<Character> OnTargetChanged;

        public void SetTarget(Character target)
        {
            CurrentTarget = target;
            OnTargetChanged?.Invoke(CurrentTarget);
        }

        public void ClearTarget() => SetTarget(null);
    }
}
