using UnityEngine;

namespace Core.Behaviour {
    public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour {
        public static T Instance { get; private set; }
        protected virtual void Awake()
        {
            ToSingleton();
            Initialize();
        }

        protected void ToSingleton(bool dontDestroyOnLoad=false) {
            if (Instance != null) {
                Debug.LogWarning($"Multiple Instances of {typeof(T)} was found on the scene!\n" +
                                 $"{gameObject.name} will be destroyed upon start.");
                Destroy(gameObject);
                return;
            }

            Instance = (T)(MonoBehaviour)this;
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        protected abstract void Initialize();
        protected virtual void BeforeDestroy() {}

        protected virtual void OnDestroy() {
            BeforeDestroy();
            if (Instance == this) {
                Instance = null;
            }
        }
    }
}