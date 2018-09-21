using System;
using UnityEngine;
public abstract class SingletonScriptable<T> : ScriptableObject where T : SingletonScriptable<T>
{
	private static T _instance;
	public static T instance
	{
		get
		{
			if (SingletonScriptable<T>._instance == null)
			{
				SingletonScriptable<T>._instance = ScriptableObject.CreateInstance<T>();
				SingletonScriptable<T>._instance.AfterCreate();
			}
			return SingletonScriptable<T>._instance;
		}
	}
	public static void Init()
	{
		if (SingletonScriptable<T>._instance == null)
		{
			SingletonScriptable<T>._instance = ScriptableObject.CreateInstance<T>();
			SingletonScriptable<T>._instance.AfterCreate();
		}
	}
	protected abstract void OnDestroy();
	protected virtual void AfterCreate()
	{
	}
}
