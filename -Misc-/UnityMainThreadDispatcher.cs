using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
	private static readonly Queue<Action> _executionQueue = new Queue<Action>();

	private static UnityMainThreadDispatcher _instance = null;

	public void Update()
	{
		lock (_executionQueue)
		{
			while (_executionQueue.Count > 0)
			{
				_executionQueue.Dequeue()();
			}
		}
	}

	public void Enqueue(IEnumerator action)
	{
		lock (_executionQueue)
		{
			_executionQueue.Enqueue(delegate
			{
				StartCoroutine(action);
			});
		}
	}

	public void Enqueue(Action action)
	{
		Enqueue(ActionWrapper(action));
	}

	private IEnumerator ActionWrapper(Action a)
	{
		a();
		yield return null;
	}

	public static bool Exists()
	{
		return _instance != null;
	}

	public static UnityMainThreadDispatcher Instance()
	{
		if (!Exists())
		{
			throw new Exception("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
		}
		return _instance;
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
	}
}
