using UnityEngine;

namespace ElevatorSystem.Utils
{
    public class GenericSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        
        public static T Instance { get { return _instance; } }


        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this as T)
            {
                Destroy(gameObject);
            }
        }
    }
}
