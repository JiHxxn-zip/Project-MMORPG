/// <summary>
/// UI 패널의 생성, 활성화, 스택 관리를 담당하는 싱글턴 매니저.
/// 카메라·게임로직 직접 참조 없이 이벤트(Action)로 외부와 연동한다.
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMORPG.Core;

namespace MMORPG.Game
{
    public class UIManager : SingletonManager<UIManager>
    {
        [SerializeField] private UIPanelDataSO _panelData;
        [SerializeField] private Transform _panelRoot;
        [SerializeField] private Transform _alwaysOnTopRoot;
        [SerializeField] private CanvasScaler _canvasScaler;

        private readonly Dictionary<UIPanelType, GameUIPanel> _panelCache = new();
        private readonly Stack<UIPanelType> _panelStack = new();

        public event Action<UIPanelType> OnPanelOpened;
        public event Action<UIPanelType> OnPanelClosed;

        // ─────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────

        /// <summary>패널을 열고 해당 인스턴스를 반환한다.</summary>
        public T OpenPanel<T>(UIPanelType type, PanelOpenFlag flags = PanelOpenFlag.None) where T : GameUIPanel
        {
            if (!TryGetOrCreate(type, out T panel))
                return null;

            // ClearStack: 스택에 쌓인 패널 전부 닫기
            if ((flags & PanelOpenFlag.ClearStack) != 0)
                CloseAllStacked();

            // KeepPrevious가 없으면 현재 최상단 패널을 숨김
            if ((flags & PanelOpenFlag.KeepPrevious) == 0)
                PeekAndSetActive(false);

            // AlwaysOnTop: 별도 루트로 이동, 스택 미포함
            if ((flags & PanelOpenFlag.AlwaysOnTop) != 0)
                panel.transform.SetParent(_alwaysOnTopRoot, false);
            else
                panel.transform.SetParent(_panelRoot, false);

            panel.gameObject.SetActive(true);
            panel.transform.SetAsLastSibling();
            panel.OnOpen();

            if ((flags & PanelOpenFlag.AlwaysOnTop) == 0)
                _panelStack.Push(type);

            OnPanelOpened?.Invoke(type);
            return panel;
        }

        /// <summary>지정한 타입의 패널을 닫는다.</summary>
        public void ClosePanel(UIPanelType type)
        {
            if (!_panelCache.TryGetValue(type, out var panel))
            {
                Debug.LogError($"[UIManager] 닫으려는 패널 '{type}'이 캐시에 없습니다.");
                return;
            }

            panel.OnClose();
            panel.gameObject.SetActive(false);
            RemoveFromStack(type);
            PeekAndSetActive(true);

            OnPanelClosed?.Invoke(type);
        }

        /// <summary>스택 최상단 패널을 닫는다.</summary>
        public void CloseCurrent()
        {
            if (_panelStack.Count == 0) return;
            ClosePanel(_panelStack.Peek());
        }

        /// <summary>뒤로가기 버튼용 명시적 메서드.</summary>
        public void GoBack() => CloseCurrent();

        /// <summary>해당 패널이 현재 활성 상태인지 반환한다.</summary>
        public bool IsPanelOpen(UIPanelType type)
        {
            return _panelCache.TryGetValue(type, out var panel) && panel.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 현재 화면 비율에 맞춰 CanvasScaler.matchWidthOrHeight를 조정한다.
        /// </summary>
        public void RefreshDisplaySize()
        {
            if (_canvasScaler == null) return;

            const float standardRatio = 1080f / 1920f;
            float currentRatio = (float)Screen.height / Screen.width;

            if (currentRatio > standardRatio)
                _canvasScaler.matchWidthOrHeight = 0f;
            else if (currentRatio < standardRatio)
                _canvasScaler.matchWidthOrHeight = 1f;
            else
                _canvasScaler.matchWidthOrHeight = 0.5f;
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        private bool TryGetOrCreate<T>(UIPanelType type, out T panel) where T : GameUIPanel
        {
            if (_panelCache.TryGetValue(type, out var cached))
            {
                panel = cached as T;
                if (panel == null)
                    Debug.LogError($"[UIManager] 캐시된 패널 '{type}'을 {typeof(T).Name}으로 캐스팅할 수 없습니다.");
                return panel != null;
            }

            if (!_panelData.TryGetPrefab(type, out var prefab))
            {
                panel = null;
                return false;
            }

            var instance = Instantiate(prefab, _panelRoot);
            instance.gameObject.SetActive(false);
            _panelCache[type] = instance;

            panel = instance as T;
            if (panel == null)
            {
                Debug.LogError($"[UIManager] 생성된 패널 '{type}'을 {typeof(T).Name}으로 캐스팅할 수 없습니다.");
                return false;
            }
            return true;
        }

        private void PeekAndSetActive(bool active)
        {
            if (_panelStack.Count == 0) return;
            var topType = _panelStack.Peek();
            if (_panelCache.TryGetValue(topType, out var panel))
                panel.gameObject.SetActive(active);
        }

        private void CloseAllStacked()
        {
            while (_panelStack.Count > 0)
            {
                var type = _panelStack.Pop();
                if (_panelCache.TryGetValue(type, out var panel))
                {
                    panel.OnClose();
                    panel.gameObject.SetActive(false);
                }
            }
        }

        private void RemoveFromStack(UIPanelType type)
        {
            var temp = new List<UIPanelType>(_panelStack);
            temp.Remove(type);
            _panelStack.Clear();
            for (int i = temp.Count - 1; i >= 0; i--)
                _panelStack.Push(temp[i]);
        }

        // ─────────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────────

        public override void Initialize()
        {
            _panelCache.Clear();
            _panelStack.Clear();
            RefreshDisplaySize();
        }

#if UNITY_EDITOR
        private void Update() => RefreshDisplaySize();
#endif
    }
}
