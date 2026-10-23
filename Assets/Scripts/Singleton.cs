using UnityEngine;

// generic-класс, где T должен быть MonoBehaviour
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            // Блокировка для потокобезопасности (актуально при инициализации)
            lock (_lock)
            {
                if (_instance == null)
                {
                    // Ищем объект на сцене, если он уже там есть
                    _instance = FindFirstObjectByType<T>();

                    // Если объекта на сцене нет, создаем его автоматически
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        
                        // Если хотите, чтобы менеджер не уничтожался между сценами:
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // Если в сцене случайно оказался дубликат — уничтожаем его
            Destroy(gameObject);
        }
    }
}
