using System;
using UnityEngine;
public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    private static T _instance;
    public static T instance
    {
        get
        {
            return SingletonBehaviour<T>._instance;
        }
    }
    protected virtual void Awake()
    {
        if (_instance)
        {
            Debug.LogWarning("已经存在相同的单例脚本！");
            return;
        }
        SingletonBehaviour<T>._instance = (T)((object)this);
        DontDestroyOnLoad(gameObject);
    }
}
