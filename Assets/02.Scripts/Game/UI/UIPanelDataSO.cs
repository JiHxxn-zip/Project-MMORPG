/// <summary>
/// 씬에 등록할 모든 UI 패널 프리팹 목록을 보관하는 ScriptableObject.
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using MMORPG.Core;

namespace MMORPG.Game
{
    [CreateAssetMenu(menuName = "MMORPG/Data/UIPanelData")]
    public class UIPanelDataSO : ScriptableObject
    {
        [SerializeField] private List<UIPanelEntry> _entries;

        /// <summary>
        /// 지정한 타입의 프리팹을 반환한다. 없으면 false와 함께 에러를 출력한다.
        /// </summary>
        public bool TryGetPrefab(UIPanelType type, out GameUIPanel prefab)
        {
            foreach (var entry in _entries)
            {
                if (entry.panelType == type)
                {
                    prefab = entry.prefab;
                    return true;
                }
            }

            prefab = null;
            Debug.LogError($"[UIPanelDataSO] 패널 타입 '{type}'에 해당하는 프리팹이 등록되어 있지 않습니다.");
            return false;
        }
    }
}
