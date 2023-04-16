using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.AI;

public class Planet : MonoBehaviour
{
	private static AssetBundle musicBundle;

	private static string[] musicOptions;

	private static AssetBundle generalBundle;

	private static readonly Dictionary<string, AssetBundle> specificBundles = new Dictionary<string, AssetBundle>();

	public Consort.Species consorts;

	public float hue;

	public float saturation;

	public float value;

	private DisplacementMapFlat map;

	private readonly List<DungeonEntrance> dungeons = new List<DungeonEntrance>();

	private readonly Dictionary<Transform, Coroutine> players = new Dictionary<Transform, Coroutine>();

	private WorldArea area;

	private Enemy[] enemies;

	private readonly List<Aspect> gristTypes = new List<Aspect>();

	private Vector3[] points;

	public Color AmbientLight => map.AmbientLight;

	public Material Material => map.Material;

	public int GetGrist(int tier)
	{
		return Grist.GetIndex(tier, gristTypes[UnityEngine.Random.Range(0, gristTypes.Count)]);
	}

	public DisplacementMapFlat.Chunk GetChunk(int index)
	{
		return map.GetChunk(index);
	}

	public DisplacementMapFlat.Chunk GetChunk(Vector3 localPos)
	{
		return map.GetChunk(localPos, fill: false);
	}

	public static Planet Build(House house, string file1 = null, string file2 = null, Aspect aspect = Aspect.Count)
	{
		if (aspect == Aspect.Count)
		{
			if (house.Owner == null)
			{
				aspect = Aspect.Time;
				Debug.LogWarning($"Could not find player {house.Id}; defaulting to Time for their land.");
			}
			else
			{
				aspect = house.Owner.classpect.aspect;
			}
		}
		int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		Debug.Log("Building planet with seed " + seed);
		GameObject gameObject = new GameObject();
		gameObject.transform.SetParent(house.transform, worldPositionStays: false);
		if (generalBundle == null)
		{
			generalBundle = AssetBundleExtensions.Load("land");
		}
		string mapProps = null;
		List<int> itemDensities = new List<int>();
		List<string> itemTypes = new List<string>();
		List<int> creatureDensities = new List<int>();
		List<string> creatureTypes = new List<string>();
		FileInfo fileInfo;
		if (file1 == null)
		{
			FileInfo[] array = StreamingAssets.GetDirectoryContents($"Aspects/{aspect}/Land1", "*.txt").ToArray();
			fileInfo = array[UnityEngine.Random.Range(0, array.Length)];
		}
		else
		{
			StreamingAssets.TryGetFile($"Aspects/{aspect}/Land1/{file1}.txt", out var path);
			fileInfo = new FileInfo(path);
		}
		string biome;
		List<TreeType> trees;
		using (StreamReader streamReader = fileInfo.OpenText())
		{
			biome = streamReader.ReadLine();
			GameObject gameObject2 = generalBundle.LoadAsset<GameObject>(streamReader.ReadLine());
			GameObject gameObject3 = generalBundle.LoadAsset<GameObject>("transportalizer_station");
			List<TreeType> list = new List<TreeType>();
			list.Add(new TreeType(10, gameObject2));
			list.Add(new TreeType(1, gameObject3));
			list.Add(new TreeType(1, Resources.Load<GameObject>("Prefabs/Return Node")));
			trees = list;
			ReadLandObjects(streamReader, ref mapProps, ref itemTypes, ref itemDensities, ref creatureTypes, ref creatureDensities, ref trees);
		}
		FileInfo fileInfo2;
		if (file2 == null)
		{
			FileInfo[] array2 = StreamingAssets.GetDirectoryContents($"Aspects/{aspect}/Land2", "*.txt").ToArray();
			fileInfo2 = array2[UnityEngine.Random.Range(0, array2.Length)];
		}
		else
		{
			StreamingAssets.TryGetFile($"Aspects/{aspect}/Land2/{file2}.txt", out var path2);
			fileInfo2 = new FileInfo(path2);
		}
		using (StreamReader streamReader2 = fileInfo2.OpenText())
		{
			string[] array3 = streamReader2.ReadLine().Split(',');
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
			string text = array3[UnityEngine.Random.Range(0, array3.Length)];
			gameObject.name = "Land of " + fileNameWithoutExtension + " and " + text;
			ReadLandObjects(streamReader2, ref mapProps, ref itemTypes, ref itemDensities, ref creatureTypes, ref creatureDensities, ref trees);
		}
		DisplacementMapFlat displacementMapFlat = gameObject.AddComponent<DisplacementMapFlat>();
		Planet planet = gameObject.AddComponent<Planet>();
		planet.hue = UnityEngine.Random.Range(0f, 360f);
		planet.saturation = UnityEngine.Random.Range(0.5f, 1f);
		planet.value = UnityEngine.Random.Range(0.5f, 1f);
		planet.map = displacementMapFlat;
		planet.area = house;
		planet.SetMusic(aspect);
		planet.enemies = SpawnHelper.instance.GetCreatures<Enemy>(new string[4] { "Giclops", "Ogre", "Basilisk", "Imp" });
		GristAssignment.Add(aspect, planet.gristTypes);
		using (StreamReader streamReader3 = File.OpenText(Application.streamingAssetsPath + "/Aspects/" + aspect.ToString() + "/info.txt"))
		{
			string text2 = streamReader3.ReadLine();
			planet.consorts = (Consort.Species)Enum.Parse(typeof(Consort.Species), text2);
		}
		Material material = new Material(Shader.Find("Noise/FlatGen"));
		Material oceanMaterial = new Material(Shader.Find("Noise/SeaShader"));
		GameObject bossDungeon = generalBundle.LoadAsset<GameObject>("Lich Dungeon");
		displacementMapFlat.Make(seed, biome, material, oceanMaterial, trees.ToArray(), itemDensities.ToArray(), itemTypes.ToArray(), creatureDensities.ToArray(), creatureTypes.ToArray(), house, mapProps, bossDungeon);
		planet.SetGrassMaterial(trees, material);
		return planet;
	}

	private void SetGrassMaterial(IEnumerable<TreeType> props, Material mat)
	{
		Color color;
		Color color2;
		float @float;
		float float2;
		float float3;
		float float4;
		if (mat.GetFloat("_Snow") > 0f)
		{
			color = mat.GetColor("_GrassColorStart");
			color2 = mat.GetColor("_GrassColorEnd");
			@float = mat.GetFloat("_GrassColorOctaves");
			float2 = mat.GetFloat("_GrassColorFrequency");
			float3 = mat.GetFloat("_GrassColorAmplitude");
			float4 = mat.GetFloat("_GrassColorPersistence");
		}
		else
		{
			color = mat.GetColor("_SnowColorStart");
			color2 = mat.GetColor("_SnowColorEnd");
			@float = mat.GetFloat("_GrassColorOctaves");
			float2 = mat.GetFloat("_SnowColorFrequency");
			float3 = mat.GetFloat("_SnowColorAmplitude");
			float4 = mat.GetFloat("_SnowColorPersistence");
		}
		Color color3 = -4f / 51f * new Color(1f, 1f, 1f, 0f);
		color += color3;
		color2 += color3;
		foreach (TreeType prop in props)
		{
			for (int i = 0; i < prop.subtypes.Length; i++)
			{
				GameObject gameObject = prop.subtypes[i];
				if (gameObject.GetComponentsInChildren<MeshRenderer>().SelectMany((MeshRenderer r) => r.sharedMaterials).All((Material m) => m?.shader?.name != "Noise/SimpleProceduralShader"))
				{
					continue;
				}
				gameObject = prop.GetPrefabBounds(i).Item1;
				prop.subtypes[i] = gameObject;
				gameObject.SetActive(value: false);
				gameObject.transform.SetParent(base.transform);
				MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer meshRenderer in componentsInChildren)
				{
					Material[] sharedMaterials = meshRenderer.sharedMaterials;
					int num = Array.FindIndex(sharedMaterials, (Material m) => m?.shader?.name == "Noise/SimpleProceduralShader");
					if (num != -1)
					{
						sharedMaterials[num] = new Material(sharedMaterials[num]);
						sharedMaterials[num].SetColor("_ColorStart", color);
						sharedMaterials[num].SetColor("_ColorEnd", color2);
						sharedMaterials[num].SetFloat("_Octaves", @float);
						sharedMaterials[num].SetFloat("_Frequency", float2 * meshRenderer.transform.localScale.x);
						sharedMaterials[num].SetFloat("_Amplitude", float3);
						sharedMaterials[num].SetFloat("_Persistence", float4);
						sharedMaterials[num].renderQueue = 3000;
						meshRenderer.sharedMaterials = sharedMaterials;
					}
				}
			}
		}
	}

	private void SetMusic(Aspect aspect)
	{
		if (musicBundle == null)
		{
			AssetBundleExtensions.LoadAsync("landmusic").completed += OnBundleLoaded;
			return;
		}
		musicBundle.LoadAssetWithSubAssetsAsync<AudioClip>($"{aspect}_land_roaming").completed += delegate(AsyncOperation obj)
		{
			map.Music = new AudioClip[1] { (((AssetBundleRequest)obj).asset as AudioClip) ?? MusicHolder.GetPlanetMusic() };
		};
		musicBundle.LoadAssetAsync<AudioClip>($"{aspect}_land_strife").completed += delegate(AsyncOperation obj)
		{
			map.StrifeMusic = (((AssetBundleRequest)obj).asset as AudioClip) ?? MusicHolder.PlanetStrifeMusic;
		};
		void OnBundleLoaded(AsyncOperation obj)
		{
			musicBundle = ((AssetBundleCreateRequest)obj).assetBundle;
			musicOptions = musicBundle.GetAllAssetNames();
			SetMusic(aspect);
		}
	}

	private static void ReadLandObjects(StreamReader reader, ref string mapProps, ref List<string> itemTypes, ref List<int> itemDensities, ref List<string> creatureTypes, ref List<int> creatureDensities, ref List<TreeType> trees)
	{
		while (!reader.EndOfStream)
		{
			string text = reader.ReadLine();
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			if (text.StartsWith("Map: "))
			{
				mapProps = text.Substring("Map: ".Length);
				continue;
			}
			int num = text.LastIndexOf(' ');
			string text2 = text.Substring(0, num);
			int num2 = int.Parse(text.Substring(num + 1));
			if (text2.StartsWith("Item:"))
			{
				itemTypes.Add(text2.Substring("Item:".Length));
				itemDensities.Add(num2);
			}
			else if (text2.StartsWith("Creature:"))
			{
				creatureTypes.Add(text2.Substring("Creature:".Length));
				creatureDensities.Add(num2);
			}
			else
			{
				trees.Add(new TreeType(num2, LoadTrees(text2)));
			}
		}
	}

	private static GameObject[] LoadTrees(string name)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject gameObject = generalBundle.LoadAsset<GameObject>(name);
		if (gameObject != null)
		{
			list.Add(gameObject);
		}
		string key = "lands/" + name.ToLowerInvariant();
		if (!specificBundles.TryGetValue(key, out var bundle) && AssetBundleExtensions.TryLoad(key, out bundle))
		{
			specificBundles.Add(key, bundle);
		}
		if (bundle != null)
		{
			list.AddRange(bundle.LoadAllAssets<GameObject>());
		}
		Furniture furniturePrefab = HouseManager.Instance.GetFurniturePrefab(name);
		if ((bool)furniturePrefab)
		{
			list.Add(furniturePrefab.gameObject);
		}
		string[] array = new string[15]
		{
			"tree", "tree 1", "tree 2", "tree 3", "shrub 1", "shrub 3", "Mushroom 1", "Flower1", "Flower2", "Flower3",
			"Rock 1", "Rock 2", "Rock 3", "Rock 4", "Rock 5"
		};
		foreach (string text in array)
		{
			if (text.StartsWith(name, StringComparison.OrdinalIgnoreCase))
			{
				furniturePrefab = HouseManager.Instance.GetFurniturePrefab(text);
				if ((bool)furniturePrefab)
				{
					list.Add(furniturePrefab.gameObject);
				}
			}
		}
		return list.ToArray();
	}

	private void OnDestroy()
	{
		GristAssignment.Remove(gristTypes);
	}

	public void Save(Stream stream)
	{
		map.Save(stream);
	}

	public void Load(Stream stream, int version)
	{
		map.Load(stream, version);
	}

	private void Update()
	{
		RefreshChunks();
	}

	public void RefreshChunks()
	{
		if (points == null || points.Length != players.Count)
		{
			points = new Vector3[players.Count];
		}
		int num = 0;
		foreach (Transform key in players.Keys)
		{
			points[num++] = key.localPosition;
		}
		map.LoadChunksNearPoints(points);
	}

	public void AddDungeon(DungeonEntrance dungeon)
	{
		dungeons.Add(dungeon);
	}

	public void AddPlayer(Player player)
	{
		Coroutine coroutine = null;
		if (NetworkServer.active)
		{
			coroutine = StartCoroutine(AutoSpawn(player.transform));
		}
		players.Add(player.transform, coroutine);
	}

	public void RemovePlayer(Player player)
	{
		if (NetworkServer.active)
		{
			StopCoroutine(players[player.transform]);
		}
		players.Remove(player.transform);
	}

	private IEnumerator AutoSpawn(Transform target)
	{
		float maxDist = Mathf.Sqrt(460800f);
		Vector3 prevPos = target.localPosition;
		while (true)
		{
			yield return new WaitForSeconds(2f * UnityEngine.Random.value);
			if (target == null || target.Equals(null))
			{
				break;
			}
			Vector3 pos = target.localPosition;
			if (map.IsVillage(pos))
			{
				continue;
			}
			Vector3 vector = pos - prevPos;
			if (vector.sqrMagnitude < 144f)
			{
				continue;
			}
			vector.Normalize();
			float num = 0f;
			foreach (DungeonEntrance dungeon in dungeons)
			{
				Vector3 rhs = pos - dungeon.transform.localPosition;
				float sqrMagnitude = rhs.sqrMagnitude;
				num += maxDist * Vector3.Dot(vector, rhs) / sqrMagnitude;
			}
			if (!(UnityEngine.Random.value < num) || !NavMesh.SamplePosition(target.position + 12f * vector, out var hit, 12f, -1))
			{
				continue;
			}
			int num2 = Mathf.CeilToInt(Vector3.SqrMagnitude(pos) / 460800f * 26f);
			while (UnityEngine.Random.Range(0, num2 + 1) != 0)
			{
				Enemy[] array = enemies;
				foreach (Enemy enemy in array)
				{
					int cost = enemy.GetCost();
					if (num2 > cost && UnityEngine.Random.Range(0, 2) == 0)
					{
						SpawnHelper.instance.Spawn(enemy, area, hit.position, GetGrist(0));
						num2 -= cost;
					}
				}
			}
			yield return new WaitForSeconds(1.5f * (26f - (float)num2));
			if (!(target == null) && !target.Equals(null))
			{
				prevPos = pos;
				continue;
			}
			break;
		}
	}

	public IEnumerable<LDBItem> GenerateDungeonItems(int amount, int value, int maxCost)
	{
		IEnumerable<NormalItem.Tag> tagsWithout = NormalItem.GetTagsWithout(gristTypes);
		return ItemDownloader.Instance.GenerateDungeonItems(amount, value, maxCost, tagsWithout);
	}
}
