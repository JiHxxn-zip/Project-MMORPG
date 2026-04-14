/// <summary>
/// 씬 전환 시에도 유지되는 제네릭 싱글턴 베이스 클래스.
/// </summary>
using UnityEngine;

namespace MMORPG.Core
{
    public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        public virtual void Initialize() { }
    }
}
