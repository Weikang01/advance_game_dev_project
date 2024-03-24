using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> where T : new()
{
    private static T _instance;
    private static object _instanceLock = new object();
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    _instance = new T();
                }
            }
            return _instance;
        }
    }
}


// Monobehaviour Singleton
public class UnitySingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static object _instanceLock = new object();
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).ToString();
                        singleton.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
            }
            return _instance;
        }
    }

    public virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}