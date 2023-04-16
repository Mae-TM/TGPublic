using System.IO;
using Mirror;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
	[SerializeField]
	private House housePrefab;

	private AssetBundle furniture;

	public static HouseManager Instance { get; private set; }

	private void Start()
	{
		Instance = this;
		furniture = AssetBundleExtensions.Load("furniture");
		InitProtoBuf();
		RegisterPrefabs();
	}

	private void OnDestroy()
	{
		furniture.Unload(unloadAllLoadedObjects: true);
	}

	public void RegisterPrefabs()
	{
		NetworkClient.RegisterPrefab(housePrefab.gameObject);
		GameObject[] array = furniture.LoadAllAssets<GameObject>();
		for (int i = 0; i < array.Length; i++)
		{
			NetworkClient.RegisterPrefab(array[i]);
		}
	}

	public Furniture GetFurniturePrefab(string prefabName)
	{
		return furniture.LoadAsset<GameObject>(prefabName)?.GetComponent<Furniture>();
	}

	private static void InitProtoBuf()
	{
		if (!RuntimeTypeModel.Default.IsDefined(typeof(Vector3Int)))
		{
			MetaType metaType = RuntimeTypeModel.Default.Add(typeof(Vector3Int), applyDefaultBehaviour: true);
			metaType.Add("x", "y", "z");
			ValueMember[] fields = metaType.GetFields();
			foreach (ValueMember obj in fields)
			{
				obj.DataFormat = DataFormat.ZigZag;
				obj.DefaultValue = 0;
			}
			MetaType metaType2 = RuntimeTypeModel.Default.Add(typeof(Vector2Int), applyDefaultBehaviour: true);
			metaType2.Add("x", "y");
			fields = metaType2.GetFields();
			foreach (ValueMember obj2 in fields)
			{
				obj2.DataFormat = DataFormat.ZigZag;
				obj2.DefaultValue = 0;
			}
			MetaType metaType3 = RuntimeTypeModel.Default.Add(typeof(RectInt), applyDefaultBehaviour: true);
			metaType3.AddField(1, "x").DataFormat = DataFormat.ZigZag;
			metaType3.AddField(2, "y").DataFormat = DataFormat.ZigZag;
			metaType3.Add("width", "height");
			fields = metaType3.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				fields[i].DefaultValue = 0;
			}
			MetaType metaType4 = RuntimeTypeModel.Default.Add(typeof(Vector3), applyDefaultBehaviour: true);
			metaType4.Add("x", "y", "z");
			fields = metaType4.GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				fields[i].DefaultValue = 0f;
			}
			RuntimeTypeModel.Default.AllowParseableTypes = true;
		}
	}

	public static void SaveHouse(Building house, string path)
	{
		HouseData instance = house.Save();
		instance.spawnPosition = Building.GetCoords(Player.player.transform.localPosition);
		using Stream destination = new FileStream(path, FileMode.Create, FileAccess.Write);
		Serializer.Serialize(destination, instance);
	}

	public static HouseData LoadHouse(string path)
	{
		if (!Path.IsPathRooted(path) && !StreamingAssets.TryGetFile("Houses/" + path, out path))
		{
			return default(HouseData);
		}
		using Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.SequentialScan);
		return path.EndsWith(".bin") ? OldHouseLoader.LoadOld(stream) : Serializer.Deserialize<HouseData>(stream);
	}

	[Server]
	public House SpawnHouse(int id, HouseData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'House HouseManager::SpawnHouse(System.Int32,HouseData)' called when server was not active");
			return null;
		}
		House house = Object.Instantiate(housePrefab);
		house.Init(id);
		house.LoadStructure(data);
		NetworkServer.Spawn(house.gameObject);
		house.LoadObjects(data);
		return house;
	}
}
