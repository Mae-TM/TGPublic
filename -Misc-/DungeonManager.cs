using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using ProtoBuf;
using TheGenesisLib.Models;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
	[SerializeField]
	private Dungeon dungeonPrefab;

	public static DungeonManager Instance { get; private set; }

	private void Start()
	{
		Instance = this;
		RegisterPrefabs();
	}

	public void RegisterPrefabs()
	{
		NetworkClient.RegisterPrefab(dungeonPrefab.gameObject);
		BossRoom.RegisterPrefabs();
	}

	[Server]
	public static Dungeon Build(House world, int chunk, int level)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Dungeon DungeonManager::Build(House,System.Int32,System.Int32)' called when server was not active");
			return null;
		}
		SessionRandom.Seed(chunk ^ world.Id);
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			Dungeon dungeon = Instance.BuildSimple(world, chunk);
			if (dungeon != null)
			{
				return dungeon;
			}
		}
		if (!(UnityEngine.Random.value > 0.5f))
		{
			return Instance.BuildLinear(world, chunk, level);
		}
		return Instance.BuildMaze(world, chunk, level);
	}

	[Server]
	public static Dungeon BuildBoss(House world, int chunk, string name)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Dungeon DungeonManager::BuildBoss(House,System.Int32,System.String)' called when server was not active");
			return null;
		}
		string text = Application.streamingAssetsPath + "/Dungeons/Boss/" + name + ".bin";
		using Stream stream = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.SequentialScan);
		HouseData data = (text.EndsWith(".bin") ? OldHouseLoader.LoadOld(stream) : Serializer.Deserialize<HouseData>(stream));
		HouseData.Story[] stories = data.stories;
		for (int i = 0; i < stories.Length; i++)
		{
			HouseData.Story story = stories[i];
			for (int j = 0; j < story.furniture.Length; j++)
			{
				if (story.furniture[j].name == "Spawner")
				{
					story.furniture[j].name = "Boss Entrance";
				}
			}
		}
		FillChests(ref data, world.planet, 14);
		Dungeon dungeon = Instance.Build(data, world, chunk);
		BossRoom.Build(name, dungeon);
		return dungeon;
	}

	[Server]
	private Dungeon BuildSimple(House world, int chunk)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Dungeon DungeonManager::BuildSimple(House,System.Int32)' called when server was not active");
			return null;
		}
		FileInfo[] array = StreamingAssets.GetDirectoryContents("Dungeons/Complete", "*.b??").ToArray();
		int num = array.Length;
		if (num == 0)
		{
			return null;
		}
		FileInfo fileInfo = array[UnityEngine.Random.Range(0, num)];
		using Stream stream = fileInfo.OpenRead();
		HouseData data = ((fileInfo.Extension == ".bin") ? OldHouseLoader.LoadOld(stream) : Serializer.Deserialize<HouseData>(stream));
		return Build(data, world, chunk);
	}

	private static Queue<HouseData.Item> GenerateItems(Planet planet, int amount, int level)
	{
		if ((object)planet == null)
		{
			Debug.LogError("Dungeon without planet?");
			return new Queue<HouseData.Item>();
		}
		int maxCost = Mathf.CeilToInt(Mathf.Sqrt(level));
		return new Queue<HouseData.Item>(from item in planet.GenerateDungeonItems(amount, level * level / 2, maxCost)
			select (HouseData.Item)(NormalItem)item);
	}

	[Server]
	private Dungeon BuildMaze(House world, int chunk, int level)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Dungeon DungeonManager::BuildMaze(House,System.Int32,System.Int32)' called when server was not active");
			return null;
		}
		int width = Mathf.CeilToInt(Mathf.Sqrt(level) * 7f);
		int height = Mathf.FloorToInt(Mathf.Sqrt(level) * 7f);
		Queue<HouseData.Item> items = GenerateItems(world.planet, level * 3 / 2, level);
		HouseData data = DungeonGenerator.Generate(width, height, 5, items, world.Owner.classpect.aspect);
		return Build(data, world, chunk);
	}

	[Server]
	private Dungeon BuildLinear(House world, int chunk, int length)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Dungeon DungeonManager::BuildLinear(House,System.Int32,System.Int32)' called when server was not active");
			return null;
		}
		FileInfo[] array = StreamingAssets.GetDirectoryContents("Dungeons", "*.b??").ToArray();
		CombinedDungeon combinedDungeon = new CombinedDungeon();
		for (int i = 0; i < length; i++)
		{
			if (i == length - 1)
			{
				array = StreamingAssets.GetDirectoryContents("Dungeons/End", "*.b??").ToArray();
			}
			FileInfo fileInfo = array[UnityEngine.Random.Range(0, array.Length)];
			using Stream stream = fileInfo.OpenRead();
			HouseData data = ((fileInfo.Extension == ".bin") ? OldHouseLoader.LoadOld(stream) : Serializer.Deserialize<HouseData>(stream));
			combinedDungeon.Add(data);
		}
		HouseData data2 = combinedDungeon.GetData();
		FillChests(ref data2, world.planet, length);
		return Build(data2, world, chunk);
	}

	private static void FillChests(ref HouseData data, Planet planet, int level)
	{
		HouseData.Item[][] source = (from story in data.stories
			where story.furniture != null
			from furniture in story.furniture
			where furniture.items != null
			select furniture.items).ToArray();
		int num = source.Sum((HouseData.Item[] chest) => chest.Length);
		Queue<HouseData.Item> queue = GenerateItems(planet, Math.Min(num, level * 3 / 2), level);
		foreach (HouseData.Item[] item in source.Reverse())
		{
			for (int i = 0; i < item.Length; i++)
			{
				item[i] = ((UnityEngine.Random.Range(0, num) < queue.Count) ? queue.Dequeue() : null);
				num--;
			}
		}
	}

	[Server]
	private Dungeon Build(HouseData data, House world, int chunk)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Dungeon DungeonManager::Build(HouseData,House,System.Int32)' called when server was not active");
			return null;
		}
		if (data.attackables != null)
		{
			foreach (HouseData.Enemy item in data.attackables.OfType<HouseData.Enemy>())
			{
				item.type = world.GetGrist(Math.Max(0, Grist.GetTier(item.type)));
			}
		}
		Dungeon dungeon = Load(data, Dungeon.GetID(world.Id, chunk));
		HouseData.Furniture data2 = default(HouseData.Furniture);
		data2.name = "Spawner";
		data2.x = data.spawnPosition.x;
		data2.z = data.spawnPosition.z;
		Furniture.Load(data2, dungeon, data.spawnPosition.y);
		return dungeon;
	}

	[Server]
	public Dungeon Load(HouseData data, int id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Dungeon DungeonManager::Load(HouseData,System.Int32)' called when server was not active");
			return null;
		}
		Dungeon dungeon = UnityEngine.Object.Instantiate(dungeonPrefab);
		dungeon.Init(id);
		dungeon.LoadStructure(data);
		NetworkServer.Spawn(dungeon.gameObject);
		dungeon.LoadObjects(data);
		return dungeon;
	}
}
