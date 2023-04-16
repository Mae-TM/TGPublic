using UnityEngine;

public abstract class AbstractAttachedSingletonManager<T> : MonoBehaviour where T : AbstractAttachedSingletonManager<T>, new()
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (!((Object)_instance == (Object)null))
			{
				return _instance;
			}
			return new GameObject(typeof(T).FullName).AddComponent<T>();
		}
	}

	public void Awake()
	{
		if ((Object)_instance != (Object)null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_instance = (T)this;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
