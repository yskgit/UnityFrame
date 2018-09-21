using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
public class AOTSafe
{
	public static void Foreach<T>(object enumerable, Action<T> action)
	{
		if (enumerable == null)
		{
			return;
		}
		Type typeFromHandle = typeof(IEnumerable);
		if (!Enumerable.Contains<Type>(enumerable.GetType().GetInterfaces(), typeFromHandle))
		{
			Debug.LogError("Object does not implement IEnumerable interface");
			return;
		}
		MethodInfo method = typeFromHandle.GetMethod("GetEnumerator");
		if (method == null)
		{
			Debug.LogError("Failed to get 'GetEnumberator()' method info from IEnumerable type");
			return;
		}
		IEnumerator enumerator = null;
		try
		{
			enumerator = (IEnumerator)method.Invoke(enumerable, null);
			if (enumerator is IEnumerator)
			{
				while (enumerator.MoveNext())
				{
					action.Invoke((T)((object)enumerator.Current));
				}
			}
			else
			{
				Debug.LogError(string.Format("{0}.GetEnumerator() returned '{1}' instead of IEnumerator.", enumerable.ToString(), enumerator.GetType().Name));
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
