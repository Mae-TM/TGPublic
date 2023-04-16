using System;
using System.Linq;
using Mirror;
using UnityEngine;

public class SpawnHelper : MonoBehaviour
{
	public static SpawnHelper instance;

	private readonly ConcurrentQueue<GameObject> toDestroy = new ConcurrentQueue<GameObject>();

	private AssetBundle creatureBundle;

	private void Awake()
	{
		instance = this;
		creatureBundle = AssetBundleExtensions.Load("creature");
		GameObject[] array = creatureBundle.LoadAllAssets<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject.GetComponent<NetworkIdentity>())
			{
				NetworkClient.RegisterPrefab(gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		creatureBundle.Unload(unloadAllLoadedObjects: true);
	}

	private void Update()
	{
		while (toDestroy.Count != 0)
		{
			UnityEngine.Object.Destroy(toDestroy.Dequeue());
		}
	}

	public void Destroy(GameObject obj)
	{
		toDestroy.Enqueue(obj);
	}

	public GameObject GetCreature(string name)
	{
		return creatureBundle.LoadAsset<GameObject>(name);
	}

	public T[] GetCreatures<T>(params string[] names)
	{
		return names.Select((string name) => GetCreature(name).GetComponent<T>()).ToArray();
	}

	private bool TryGetAttackable(GameObject prefab, out Attackable att)
	{
		if ((object)prefab != null && prefab.TryGetComponent<Attackable>(out att))
		{
			return true;
		}
		Debug.LogError($"Could not find prefab '{prefab}'");
		att = null;
		return false;
	}

	public bool TryGetCreature(string name, out Attackable att)
	{
		return TryGetAttackable(GetCreature(name), out att);
	}

	public void GetCreatureAsync(string name, Action<Attackable> result, Action final = null)
	{
		creatureBundle.LoadAssetAsync<GameObject>(name).completed += delegate(AsyncOperation req)
		{
			try
			{
				if (TryGetAttackable((GameObject)((AssetBundleRequest)req).asset, out var att))
				{
					result(att);
				}
			}
			finally
			{
				final?.Invoke();
			}
		};
	}

	public string[] GetCreatureNames()
	{
		return creatureBundle.GetAllAssetNames();
	}

	public void Spawn(string prefab, WorldArea area, Vector3 location, Action<Attackable> onCreate = null)
	{
		GetCreatureAsync(prefab, delegate(Attackable att)
		{
			Spawn(att, area, location, 0, onCreate);
		});
	}

	[Server]
	public Attackable Spawn(Attackable prefab, WorldArea area, Vector3 location, int gristType = 0, Action<Attackable> onCreate = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Attackable SpawnHelper::Spawn(Attackable,WorldArea,UnityEngine.Vector3,System.Int32,System.Action`1<Attackable>)' called when server was not active");
			return null;
		}
		Attackable attackable = UnityEngine.Object.Instantiate(prefab);
		attackable.name = prefab.name;
		attackable.GristType = gristType;
		attackable.RegionChild.Area = area;
		attackable.transform.position = location;
		onCreate?.Invoke(attackable);
		NetworkServer.Spawn(attackable.gameObject);
		return attackable;
	}
}
