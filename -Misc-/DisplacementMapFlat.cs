using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using TheGenesisLib.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;

public class DisplacementMapFlat : MonoBehaviour
{
	private struct NoiseData
	{
		public float mountainStart;

		public float mountainEnd;

		public float mountainAmplitude;

		public float mountainPersistence;

		public float planeAmplitude;

		public float planePersistence;

		public int octaves;

		public float frequency;

		public float lacunarity;

		public int biomeOctaves;

		public float biomePersistence;

		public float biomeFrequency;

		public static NoiseData Create()
		{
			NoiseData result = default(NoiseData);
			result.mountainStart = 0.6f;
			result.mountainEnd = 0.9f;
			result.mountainAmplitude = 0.7f;
			result.mountainPersistence = 0.8f;
			result.planeAmplitude = 0.5f;
			result.planePersistence = 0.5f;
			result.octaves = 4;
			result.frequency = 1.5f;
			result.lacunarity = 1.92f;
			result.biomeOctaves = 3;
			result.biomePersistence = 0.8f;
			result.biomeFrequency = 1.5f;
			return result;
		}
	}

	private struct RoadNoise
	{
		public int octaves;

		public float frequency;

		public float lacunarity;

		public float gain;

		public static RoadNoise Create()
		{
			RoadNoise result = default(RoadNoise);
			result.octaves = 3;
			result.frequency = 2f;
			result.lacunarity = 1.92f;
			result.gain = 0.8f;
			return result;
		}
	}

	public class Chunk : WorldRegion
	{
		private Vector3[] vertices;

		private Coroutine deactivation;

		private RegionAmbience ambience;

		public int ID { get; private set; }

		public bool IsFilled => vertices == null;

		public bool WasFilled { get; private set; }

		public Terrain Terrain { get; private set; }

		public override AudioClip[] Music => ambience.music;

		public override AudioClip StrifeMusic => ambience.strifeMusic;

		public override Color AmbientLight => ambience.ambientLight;

		public override Color BackgroundColor => 0.1f * Color.white;

		public override float ZoomLevel => 2f;

		public static Chunk Make(int x, int y, Transform transform, Vector3[] vertices, Terrain terrain, RegionAmbience ambience, bool wasFilled)
		{
			int num = x * 6 + y;
			GameObject gameObject = new GameObject($"Chunk {num}");
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			terrain.transform.SetParent(gameObject.transform, worldPositionStays: true);
			terrain.gameObject.AddComponent<NavMeshSourceTag>().removeOnDisable = false;
			Chunk chunk = gameObject.AddComponent<Chunk>();
			chunk.ID = num;
			chunk.vertices = vertices;
			chunk.WasFilled = wasFilled;
			chunk.Terrain = terrain;
			chunk.ambience = ambience;
			return chunk;
		}

		public Vector3[] Fill()
		{
			if (IsFilled)
			{
				throw new UnauthorizedAccessException($"Chunk {ID} has already been filled!");
			}
			Vector3[] result = vertices;
			vertices = null;
			return result;
		}

		public void Activate()
		{
			if (deactivation == null)
			{
				WorldManager.SetAreaActive(base.gameObject, to: true);
			}
			else
			{
				StopCoroutine(deactivation);
			}
		}

		public void Deactivate()
		{
			if (deactivation == null)
			{
				deactivation = StartCoroutine(DeactivateRoutine());
			}
		}

		private IEnumerator DeactivateRoutine()
		{
			yield return new WaitForSeconds(10f);
			WorldManager.SetAreaActive(base.gameObject, to: false);
			deactivation = null;
		}
	}

	[BurstCompile]
	private struct HeightMapJob : IJobParallelFor
	{
		private const int height = 193;

		private readonly float houseY;

		private readonly int seed;

		private readonly NoiseData noiseData;

		[WriteOnly]
		private NativeArray<float> heights;

		public HeightMapJob(float houseY, int seed, NoiseData noiseData, NativeArray<float> heights)
		{
			this.houseY = houseY;
			this.seed = seed;
			this.noiseData = noiseData;
			this.heights = heights;
		}

		public void Execute(int index)
		{
			int result;
			int num = Math.DivRem(index, 193, out result);
			heights[index] = GetHeight(num, result, houseY, seed, noiseData);
		}
	}

	private const int width = 6;

	private const int height = 6;

	private const int chunkSize = 32;

	private const int alphamapResolution = 64;

	private const int scale = 5;

	private const float vScale = 50f;

	private const float loadRange = 0.75f;

	private const float villageSize = 9216f;

	private const int centerX = 96;

	private const int centerY = 96;

	private const int matFactor = 96;

	public const float maxSqrDist = 460800f;

	public const float worldCenterX = 480f;

	public const float worldCenterY = 480f;

	public const int borderSize = 5;

	public const int chunkCount = 36;

	[SerializeField]
	private int seed;

	[SerializeField]
	private Material material;

	[SerializeField]
	private Material oceanMaterial;

	[SerializeField]
	private RegionAmbience ambience;

	[SerializeField]
	private string biome;

	[SerializeField]
	private TreeType[] trees;

	[SerializeField]
	private int[] maxItemDensity;

	[SerializeField]
	private LDBItem[] itemTypes;

	[SerializeField]
	private int[] maxCreatureDensity;

	[SerializeField]
	private string[] creatureTypes;

	[SerializeField]
	private GameObject bossDungeon;

	private Chunk[,] chunks;

	private List<int> wasFilledChunks;

	private GameObject water;

	private bool isOceanWalkable;

	private bool isOceanBurning;

	private float oceanSpeed = 1f;

	private float seaLevel;

	private float houseY;

	private NoiseData noiseData = NoiseData.Create();

	private RoadNoise roadNoise = RoadNoise.Create();

	private FastNoise treeNoise;

	private readonly List<Vector3> villagePositions = new List<Vector3>();

	private float heightOffset;

	private int riverCount = 10;

	private float riverFriction = 1.25f;

	private float riverStraightness = 1f;

	private readonly int oceanOffsetID = Shader.PropertyToID("_OceanOffset");

	private float oceanOffset;

	private readonly List<Chunk> loadedChunks = new List<Chunk>(0);

	private readonly HashSet<Chunk> toUnload = new HashSet<Chunk>();

	private float snowline;

	private float sandStart;

	private float sandEnd;

	private const float RIVER_DEPTH = 2f;

	public Material Material => material;

	public Color AmbientLight => ambience.ambientLight;

	public AudioClip[] Music
	{
		set
		{
			ambience.music = value;
		}
	}

	public AudioClip StrifeMusic
	{
		set
		{
			ambience.strifeMusic = value;
		}
	}

	public float SeaY => seaLevel * 50f + heightOffset;

	public event Action PlanetLoaded;

	private void Update()
	{
		if (oceanSpeed != 0f)
		{
			oceanOffset += oceanSpeed * Time.deltaTime;
			material.SetVector(oceanOffsetID, Vector3.up * oceanOffset);
			oceanMaterial.SetVector(oceanOffsetID, Vector3.up * oceanOffset);
		}
	}

	public void Make(int seed, string biome, Material material, Material oceanMaterial, TreeType[] trees, int[] maxItemDensity, string[] itemTypes, int[] maxCreatureDensity, string[] creatureTypes, House house, string mapProps, GameObject bossDungeon)
	{
		this.seed = seed;
		this.biome = biome;
		this.material = material;
		this.oceanMaterial = oceanMaterial;
		this.trees = trees;
		this.maxItemDensity = maxItemDensity;
		this.creatureTypes = creatureTypes;
		this.maxCreatureDensity = maxCreatureDensity;
		this.itemTypes = ItemDownloader.Instance.GetItems(itemTypes);
		this.bossDungeon = bossDungeon;
		ambience = new RegionAmbience();
		Make(house, mapProps);
	}

	private void Make(House house, string mapProps)
	{
		UnityEngine.Random.InitState(seed);
		ReadBiome(biome, out var cloudColor, out var mapGround, out var mapWater, out var mapClouds);
		houseY = Mathf.Max(GetHeight(96f, 96f, ignoreHouse: true), GetHeight(141f, 96f, ignoreHouse: true), GetHeight(51f, 96f, ignoreHouse: true), GetHeight(96f, 141f, ignoreHouse: true), GetHeight(96f, 51f, ignoreHouse: true));
		treeNoise = new FastNoise();
		treeNoise.SetFractalOctaves(3);
		treeNoise.SetFrequency(1.5f);
		treeNoise.SetFractalLacunarity(1.92f);
		treeNoise.SetFractalGain(0.8f);
		StartCoroutine(GenerateChunks(house, cloudColor, mapGround, mapWater, mapClouds, mapProps));
	}

	private void ChunksGenerated(House house, float minHeight, IEnumerable<IEnumerable<Vector3>> villageSuggestions, Color cloudColor, string mapGround, string mapWater, string mapClouds, string mapProps)
	{
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				chunks[i, j].gameObject.SetActive(value: true);
			}
		}
		FlatMap.MapPlanet(base.transform, house.Id, minHeight, 0f - heightOffset, 950, 950);
		Color groundColor = ((!(material.GetFloat("_Snow") * 50f > minHeight)) ? ((material.GetColor("_SnowColorStart") + material.GetColor("_SnowColorEnd")) / 2f) : ((material.GetColor("_GrassColorStart") + material.GetColor("_GrassColorEnd")) / 2f));
		Color waterColor = ((SeaY > minHeight) ? material.GetColor("_OceanColorMid") : Color.clear);
		FlatMap.AddPlanet(house.Id, base.transform.name, groundColor, waterColor, cloudColor, mapGround, mapWater, mapClouds, mapProps);
		for (int k = 0; k < 6; k++)
		{
			for (int l = 0; l < 6; l++)
			{
				chunks[k, l].gameObject.SetActive(value: false);
			}
		}
		if ((object)water != null)
		{
			Visibility.Copy(water, house.Outside.gameObject);
		}
		ProcessVillagePositions(villageSuggestions);
		foreach (Vector3 villagePosition in villagePositions)
		{
			Village.Make(this, villagePosition, 9216f);
		}
		UnityEngine.Random.InitState(seed);
		Vector3[] array = Gate.MakeGates(house, this, villagePositions).ToArray();
		Vector3 randomValidPosition = GetRandomValidPosition(array[1], 16f);
		Vector3 position = base.transform.InverseTransformPoint(randomValidPosition);
		Transform parent = GetChunk(position, fill: false).transform;
		bossDungeon = UnityEngine.Object.Instantiate(bossDungeon, randomValidPosition, Quaternion.identity, parent);
		this.PlanetLoaded?.Invoke();
	}

	private void ProcessVillagePositions(IEnumerable<IEnumerable<Vector3>> villageSuggestions)
	{
		villagePositions.AddRange(villageSuggestions.SelectMany((IEnumerable<Vector3> villages) => from a in villages
			orderby a.sqrMagnitude
			where villagePositions.TrueForAll((Vector3 b) => GetLoopedSqrDist(a, b) > 73728f)
			select a));
	}

	private Chunk GetChunk(int x, int y, bool fill = true)
	{
		if (x < 0 || y < 0 || x >= 6 || y >= 6)
		{
			throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of range; cannot get chunk!");
		}
		if (chunks == null)
		{
			throw new NullReferenceException("Chunks are not generated yet!");
		}
		Chunk chunk = chunks[x, y];
		if ((object)chunk == null)
		{
			throw new NullReferenceException("Chunks are still being generated!");
		}
		if (fill && !chunk.IsFilled)
		{
			GenerateTrees(chunk);
		}
		return chunk;
	}

	public Chunk GetChunk(int id)
	{
		return GetChunk(id / 6, id % 6);
	}

	public Chunk GetChunk(Vector3 position, bool fill = true)
	{
		Chunk[,] array = chunks;
		if ((object)((array != null) ? array[5, 5] : null) == null)
		{
			return null;
		}
		int num = Mathf.FloorToInt(position.x / 5f / 32f + 3f);
		int num2 = Mathf.FloorToInt(position.z / 5f / 32f + 3f);
		if (num < 0 || num2 < 0 || num >= 6 || num2 >= 6)
		{
			return null;
		}
		return GetChunk(num, num2, fill);
	}

	public void LoadChunksNearPoints(IEnumerable<Vector3> points)
	{
		if (chunks == null)
		{
			return;
		}
		toUnload.UnionWith(loadedChunks);
		loadedChunks.Clear();
		foreach (Vector3 point in points)
		{
			float num = point.x / 5f / 32f + 3f;
			float num2 = point.z / 5f / 32f + 3f;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					int num3 = Mathf.FloorToInt(num + (float)i * 0.75f);
					int num4 = Mathf.FloorToInt(num2 + (float)j * 0.75f);
					if (num3 >= 0 && num4 >= 0 && num3 < 6 && num4 < 6)
					{
						Chunk chunk = GetChunk(num3, num4);
						if (!toUnload.Remove(chunk))
						{
							chunk.Activate();
						}
						loadedChunks.Add(chunk);
					}
				}
			}
		}
		foreach (Chunk item in toUnload)
		{
			item.Deactivate();
		}
		toUnload.Clear();
	}

	public float SampleHeight(Vector3 position)
	{
		Chunk chunk = GetChunk(base.transform.InverseTransformPoint(position), fill: false);
		if (chunk == null)
		{
			throw new Exception($"Could not get chunk for position {position}.");
		}
		return chunk.Terrain.SampleHeight(position) + chunk.Terrain.GetPosition().y;
	}

	public Vector3 GetRandomValidPosition(Vector3 near, float range)
	{
		if (near.y < SeaY)
		{
			throw new ArgumentException("Given position was already invalid!");
		}
		Vector3 position = base.transform.position;
		Vector3 vector2;
		while (true)
		{
			Vector2 vector = range * UnityEngine.Random.insideUnitCircle;
			vector2 = near + new Vector3(vector.x, 0f, vector.y);
			if (!(2f * (vector2.x * vector2.x + vector2.z * vector2.z) <= 8100f))
			{
				vector2.x = Mathf.Repeat(vector2.x + 475f, 950f) - 475f;
				vector2.z = Mathf.Repeat(vector2.z + 475f, 950f) - 475f;
				vector2 += position;
				vector2.y = SampleHeight(vector2);
				if (vector2.y >= SeaY + position.y)
				{
					break;
				}
			}
		}
		return vector2;
	}

	private static float GetLoopedSqrDist(Vector3 a, Vector3 b)
	{
		float num = Mathf.Abs(a.x - b.x);
		num = Mathf.Min(num, 950f - num);
		float num2 = Mathf.Abs(a.z - b.z);
		num2 = Mathf.Min(num2, 950f - num2);
		return num * num + num2 * num2;
	}

	private float GetHeight(float x, float y, bool ignoreHouse = false)
	{
		return GetHeight(x, y, houseY, seed, noiseData, ignoreHouse) + heightOffset;
	}

	private static float GetHeight(float x, float y, float houseY, int seed, NoiseData noiseData, bool ignoreHouse = false)
	{
		float num;
		if (ignoreHouse)
		{
			num = 0f;
		}
		else
		{
			float num2 = (x - 96f) * 5f;
			float num3 = (y - 96f) * 5f;
			num = num2 * num2 + num3 * num3;
			num = 8100f / (num * 2f);
			if (num >= 1f)
			{
				return houseY;
			}
			num *= 3f - 2f * Mathf.Sqrt(num);
		}
		float num4 = LoopNoise(1f / 190f * x, 1f / 190f * y, seed, noiseData.biomeOctaves, noiseData.biomeFrequency, noiseData.lacunarity, noiseData.biomePersistence);
		float num5;
		float gain;
		if (num4 < noiseData.mountainStart)
		{
			num5 = noiseData.planeAmplitude;
			gain = noiseData.planePersistence;
		}
		else if (num4 < noiseData.mountainEnd)
		{
			float num6 = (num4 - noiseData.mountainStart) / (noiseData.mountainEnd - noiseData.mountainStart);
			num5 = noiseData.planeAmplitude * (1f - num6) + noiseData.mountainAmplitude * num6;
			gain = noiseData.planePersistence * (1f - num6) + noiseData.mountainPersistence * num6;
		}
		else
		{
			num5 = noiseData.mountainAmplitude;
			gain = noiseData.mountainPersistence;
		}
		float num7 = LoopNoise(1f / 190f * x, 1f / 190f * y, seed, noiseData.octaves, noiseData.frequency, noiseData.lacunarity, gain);
		return (1f - num) * 50f * num5 * num7 + num * houseY;
	}

	private static float LoopNoise(float x, float y, int seed, float octaves, float frequency, float lacunarity, float gain)
	{
		float f = x * 2f * (float)Math.PI;
		float f2 = y * 2f * (float)Math.PI;
		x = Mathf.Cos(f);
		y = Mathf.Cos(f2);
		float z = Mathf.Sin(f);
		float w = Mathf.Sin(f2);
		frequency /= (float)Math.PI * 2f;
		return FastNoise.GetSimplexFractal(x, y, z, w, seed, octaves, frequency, lacunarity, gain);
	}

	private float GetTreeDensity(Vector3 position, int type)
	{
		treeNoise.SetSeed(seed * type);
		float simplexFractal = treeNoise.GetSimplexFractal(position.x, position.y, position.z);
		simplexFractal = (simplexFractal + 1f) / 2f;
		return simplexFractal * simplexFractal * simplexFractal * (simplexFractal * (simplexFractal * 6f - 15f) + 10f);
	}

	private GameObject MakeWater<T>(string name, Mesh mesh) where T : Collider
	{
		if ((object)water == null)
		{
			water = new GameObject("Water");
			water.transform.SetParent(base.transform, worldPositionStays: false);
		}
		GameObject gameObject = new GameObject(name);
		gameObject.transform.SetParent(water.transform, worldPositionStays: false);
		gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
		gameObject.AddComponent<MeshRenderer>().sharedMaterial = oceanMaterial;
		if (!isOceanWalkable && !isOceanBurning)
		{
			return gameObject;
		}
		Collider collider = gameObject.AddComponent<T>();
		if (isOceanWalkable)
		{
			gameObject.AddComponent<NavMeshSourceTag>();
		}
		else
		{
			collider.isTrigger = true;
		}
		if (isOceanBurning)
		{
			PermanentBurningEffect permanentBurningEffect = gameObject.AddComponent<PermanentBurningEffect>();
			permanentBurningEffect.duration = 2f;
			permanentBurningEffect.intensity = 0.25f;
			permanentBurningEffect.period = 0.5f;
			permanentBurningEffect.includeEffects = false;
		}
		return gameObject;
	}

	private void GenerateOcean()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-480f, 0f, -480f),
			new Vector3(480f, 0f, -480f),
			new Vector3(-480f, 0f, 480f),
			new Vector3(480f, 0f, 480f)
		};
		mesh.triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		Mesh mesh2 = mesh;
		GameObject gameObject = MakeWater<BoxCollider>("Ocean", mesh2);
		Vector3 localPosition = gameObject.transform.localPosition;
		localPosition.y = SeaY;
		gameObject.transform.localPosition = localPosition;
		if (isOceanBurning)
		{
			gameObject.AddComponent<NavMeshObstacle>().carving = true;
		}
	}

	private void MakeRivers(Vector3[] vertices)
	{
		foreach (Mesh item in MakeRiverMesh(vertices))
		{
			MakeWater<MeshCollider>("River", item);
		}
	}

	private void GenerateEdges(float minHeight, float maxHeight)
	{
		minHeight -= 32f;
		maxHeight += 50f;
		float num = maxHeight - minHeight;
		float y = num / 2f + minHeight;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				float num2 = ((i == 0) ? 960f : 32f);
				float num3 = ((j == 0) ? 960f : 32f);
				if (i != 0 || j != 0)
				{
					BoxCollider boxCollider = new GameObject("World Edge")
					{
						layer = LayerMask.NameToLayer("Ignore Raycast"),
						tag = "Fixed Layer"
					}.AddComponent<BoxCollider>();
					boxCollider.transform.SetParent(base.transform);
					boxCollider.size = new Vector3(num2, num, num3);
					boxCollider.transform.localPosition = new Vector3((960f + num2) / 2f * (float)i, y, (960f + num3) / 2f * (float)j);
				}
			}
		}
	}

	private IEnumerator GenerateChunks(House house, Color cloudColor, string mapGround, string mapWater, string mapClouds, string mapProps)
	{
		Vector3[] vertices;
		float minHeight2;
		float maxHeight2;
		using (NativeArray<float> heights = new NativeArray<float>(37249, Allocator.Persistent, NativeArrayOptions.UninitializedMemory))
		{
			HeightMapJob jobData = new HeightMapJob(houseY, seed, noiseData, heights);
			JobHandle handle = jobData.Schedule(37249, 16);
			JobHandle.ScheduleBatchedJobs();
			yield return new WaitUntil(() => handle.IsCompleted);
			handle.Complete();
			vertices = new Vector3[37249];
			minHeight2 = float.PositiveInfinity;
			maxHeight2 = float.NegativeInfinity;
			for (int i = 0; i <= 192; i++)
			{
				for (int j = 0; j <= 192; j++)
				{
					int num = i * 193 + j;
					float num2 = heights[num];
					minHeight2 = Mathf.Min(num2, minHeight2);
					maxHeight2 = Mathf.Max(num2, maxHeight2);
					vertices[num].Set((i - 96) * 5, num2, (j - 96) * 5);
				}
			}
		}
		heightOffset = 0f - Mathf.Max(SeaY + 0.5f, houseY + 50f, maxHeight2) - 0.05f;
		for (int k = 0; k < vertices.Length; k++)
		{
			vertices[k] = new Vector3(vertices[k].x, vertices[k].y + heightOffset, vertices[k].z);
		}
		minHeight2 += heightOffset;
		maxHeight2 += heightOffset;
		if (SeaY > minHeight2)
		{
			GenerateOcean();
		}
		GenerateEdges(minHeight2, maxHeight2);
		MakeRivers(vertices);
		if ((bool)water)
		{
			house.Outside.AddExtraChild(water);
		}
		Task<(float[,,], (Vector3, int))>[][] tasks = MakeAlphaMaps(vertices);
		yield return null;
		Material material = new Material(Shader.Find("Noise/GenTexture"));
		material.SetFloat("_Scale", 32f);
		TerrainLayer[] terrainLayers = new TerrainLayer[5]
		{
			MakeTerrainLayer(material, "Grass", 320, 32f),
			MakeTerrainLayer(material, "Mountain", 320, 32f),
			MakeTerrainLayer(material, "Snow", 320, 32f),
			MakeTerrainLayer(material, "Sand", 320, 32f),
			MakeTerrainLayer(material, "Road", 320, 32f)
		};
		float[,] terrainHeights = new float[33, 33];
		chunks = new Chunk[6, 6];
		int x2 = 0;
		while (x2 < 6)
		{
			for (int l = 0; l < 6; l++)
			{
				Vector3[] array = new Vector3[1089];
				for (int m = 0; m <= 32; m++)
				{
					int sourceIndex = l * 32 + (x2 * 32 + m) * 193;
					Array.Copy(vertices, sourceIndex, array, m * 33, 33);
					for (int n = 0; n <= 32; n++)
					{
						terrainHeights[n, m] = (array[m * 33 + n].y - minHeight2) / (maxHeight2 - minHeight2);
					}
				}
				TerrainData terrainData = new TerrainData();
				terrainData.heightmapResolution = 32;
				terrainData.size = new Vector3(160f, maxHeight2 - minHeight2, 160f);
				terrainData.terrainLayers = terrainLayers;
				terrainData.alphamapResolution = 64;
				terrainData.SetHeights(0, 0, terrainHeights);
				Terrain component = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
				component.groupingID = GetInstanceID();
				Vector3 localPosition = new Vector3(5 * (32 * x2 - 96), minHeight2, 5 * (32 * l - 96));
				component.transform.SetParent(base.transform);
				component.transform.localPosition = localPosition;
				bool wasFilled = false;
				if (wasFilledChunks != null && wasFilledChunks.Count > 0 && x2 * 6 + l == wasFilledChunks[0])
				{
					wasFilledChunks.RemoveAt(0);
					wasFilled = true;
				}
				chunks[x2, l] = Chunk.Make(x2, l, base.transform, array, component, ambience, wasFilled);
				chunks[x2, l].SetSameGroup(house.Outside);
				chunks[x2, l].gameObject.SetActive(value: false);
			}
			yield return null;
			int num3 = x2 + 1;
			x2 = num3;
		}
		wasFilledChunks = null;
		List<Vector3>[] villageSuggestions = new List<Vector3>[4]
		{
			new List<Vector3>(),
			new List<Vector3>(),
			new List<Vector3>(),
			new List<Vector3>()
		};
		x2 = 0;
		while (x2 < 6)
		{
			int num3;
			for (int y = 0; y < 6; y = num3)
			{
				Task<(float[,,], (Vector3, int))> task = tasks[x2][y];
				try
				{
					yield return new WaitUntil(() => task.IsCompleted);
					(float[,,], (Vector3, int)) result = task.Result;
					(Vector3, int) item = result.Item2;
					var (map, _) = result;
					var (item2, num4) = item;
					chunks[x2, y].Terrain.terrainData.SetAlphamaps(0, 0, map);
					villageSuggestions[3 - num4].Add(item2);
				}
				finally
				{
					if (task != null)
					{
						((IDisposable)task).Dispose();
					}
				}
				num3 = y + 1;
			}
			num3 = x2 + 1;
			x2 = num3;
		}
		ChunksGenerated(house, minHeight2, villageSuggestions, cloudColor, mapGround, mapWater, mapClouds, mapProps);
	}

	private TerrainLayer MakeTerrainLayer(Material mat, string terrainType, int imageSize, float worldSize)
	{
		mat.SetColor("_Start", material.GetColor("_" + terrainType + "ColorStart"));
		mat.SetColor("_End", material.GetColor("_" + terrainType + "ColorEnd"));
		return new TerrainLayer
		{
			diffuseTexture = TextureGenerator.Generate(mat, imageSize, imageSize),
			tileSize = worldSize * Vector3.one
		};
	}

	private Task<(float[,,], (Vector3, int))>[][] MakeAlphaMaps(IReadOnlyList<Vector3> vertices)
	{
		float[] slopes = new float[36864];
		for (int i = 0; i < 192; i++)
		{
			for (int j = 0; j < 192; j++)
			{
				float y = vertices[i * 193 + j].y;
				float y2 = vertices[i * 193 + j + 1].y;
				float y3 = vertices[(i + 1) * 193 + j].y;
				float num = Mathf.Sqrt((y3 - y) * (y3 - y) + 25f + (y2 - y) * (y2 - y));
				slopes[i * 32 * 6 + j] = 5f / num;
			}
		}
		snowline = material.GetFloat("_Snow");
		sandStart = material.GetFloat("_SandStart");
		sandEnd = material.GetFloat("_SandEnd");
		Task<(float[,,], (Vector3, int))>[][] array = new Task<(float[,,], (Vector3, int))>[6][];
		for (int k = 0; k < 6; k++)
		{
			array[k] = new Task<(float[,,], (Vector3, int))>[6];
			for (int l = 0; l < 6; l++)
			{
				int taskX = k;
				int taskY = l;
				array[k][l] = new Task<(float[,,], (Vector3, int))>(() => MakeAlphaMap(vertices, slopes, taskX, taskY));
				array[k][l].Start();
			}
		}
		return array;
	}

	private (float[,,], (Vector3, int)) MakeAlphaMap(IReadOnlyList<Vector3> vertices, IReadOnlyList<float> slopes, int chunkX, int chunkY)
	{
		float[,,] array = new float[64, 64, 5];
		int num = chunkX * 32;
		int num2 = chunkY * 32;
		Vector3 village = Vector3.zero;
		float minDist = float.PositiveInfinity;
		int maxRoadCount = 0;
		for (int i = 0; i < 64; i++)
		{
			int num3 = num2 + i * 32 / 63;
			int num4 = Math.Min(num3 + 1, 192);
			float num5 = (float)(i * 32 % 63) / 63f;
			for (int j = 0; j < 64; j++)
			{
				int num6 = num + j * 32 / 63;
				int num7 = Math.Min(num6 + 1, 192);
				float num8 = (float)(j * 32 % 63) / 63f;
				float slope = GetSlope(slopes, num6, num3);
				float slope2 = GetSlope(slopes, num6, num4);
				float slope3 = GetSlope(slopes, num7, num4);
				float slope4 = GetSlope(slopes, num7, num3);
				float num9 = (1f - num8) * ((1f - num5) * slope + num5 * slope2) + num8 * ((1f - num5) * slope4 + num5 * slope3);
				num9 = 1f - num9;
				if (num9 > 0.13f)
				{
					array[i, j, 1] = 1f;
					continue;
				}
				float y = vertices[num6 * 193 + num3].y;
				float y2 = vertices[num6 * 193 + num4].y;
				float y3 = vertices[num7 * 193 + num4].y;
				float y4 = vertices[num7 * 193 + num3].y;
				float num10 = (1f - num8) * ((1f - num5) * y + num5 * y2) + num8 * ((1f - num5) * y4 + num5 * y3);
				num10 = (num10 - heightOffset) / 50f;
				float num11 = 0f;
				float num12 = 0f;
				float num13 = 0f;
				if (num10 > snowline)
				{
					num12 = 1f;
				}
				else if (num10 < sandStart)
				{
					num13 = 1f;
				}
				else if (num10 < sandEnd)
				{
					num13 = (sandEnd - num10) / (sandEnd - sandStart);
					num11 = (num10 - sandStart) / (sandEnd - sandStart);
				}
				else
				{
					num11 = 1f;
				}
				if (IsRoad((float)num6 + num8, (float)num3 + num5, num9, 0.13f, num10, ref village, ref minDist, ref maxRoadCount, out var road))
				{
					num11 *= 1f - road;
					num12 *= 1f - road;
					num13 *= 1f - road;
				}
				float num14 = (0.13f - num9) / 0.13f;
				array[i, j, 0] = num11 * num14;
				array[i, j, 1] = num9 / 0.13f;
				array[i, j, 2] = num12 * num14;
				array[i, j, 3] = num13 * num14;
				array[i, j, 4] = road * num14;
			}
		}
		return (array, (village, maxRoadCount));
	}

	private bool IsRoad(float x, float y, float slope, float maxSlope, float h, ref Vector3 village, ref float minDist, ref int maxRoadCount, out float road)
	{
		float num = LoopNoise(x * (1f / 190f) + 0.1f, y * (1f / 190f), seed, roadNoise.octaves, roadNoise.frequency, roadNoise.lacunarity, roadNoise.gain);
		float num2 = LoopNoise(x * (1f / 190f), y * (1f / 190f) + 0.1f, seed, roadNoise.octaves, roadNoise.frequency, roadNoise.lacunarity, roadNoise.gain);
		float num3 = Mathf.Sqrt(3f) / 2f;
		float[] array = new float[3]
		{
			(num >= 0f) ? Mathf.Abs(num2) : float.PositiveInfinity,
			(num <= -2f * num3 * num2) ? Mathf.Abs(num3 * num - num2 / 2f) : float.PositiveInfinity,
			(num <= 2f * num3 * num2) ? Mathf.Abs(num3 * num + num2 / 2f) : float.PositiveInfinity
		};
		int num4 = array.Count((float d) => d < 0.05f);
		if (h > seaLevel && 2f * slope < maxSlope && num4 >= maxRoadCount)
		{
			maxRoadCount = num4;
			float num5 = (x - 96f) * 5f;
			float num6 = (y - 96f) * 5f;
			float num7 = num5 * num5 + num6 * num6;
			if (2f * num7 > 8100f && num7 < minDist)
			{
				minDist = num7;
				village = new Vector3(num5, h * 50f + heightOffset, num6);
			}
		}
		if (num4 >= 1)
		{
			road = 1f - Mathf.Min(array) / 0.05f;
			return true;
		}
		road = 0f;
		return false;
	}

	private static float GetSlope(IReadOnlyList<float> normals, int x, int y)
	{
		if (x == 192 || y == 192)
		{
			return 0f;
		}
		return normals[x * 32 * 6 + y];
	}

	public bool IsVillage(Vector3 position)
	{
		foreach (Vector3 villagePosition in villagePositions)
		{
			if ((villagePosition - position).sqrMagnitude < 9216f)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsSpawnPlace(Vector3 chunkPos, int type, Vector3 pos, Vector3 normal)
	{
		if (pos.y <= SeaY || normal.y < 0.9f)
		{
			return false;
		}
		if (Mathf.Abs(pos.x) <= 45f && Mathf.Abs(pos.z) <= 45f)
		{
			return false;
		}
		return type switch
		{
			0 => true, 
			3 => IsVillage(pos + chunkPos), 
			_ => UnityEngine.Random.value >= GetTreeDensity(pos + chunkPos, type), 
		};
	}

	private void GenerateTrees(Chunk chunk)
	{
		Vector3[] vertices = chunk.Fill();
		UnityEngine.Random.InitState(seed ^ chunk.ID);
		if (trees == null)
		{
			return;
		}
		Transform transform = chunk.transform;
		Vector3 localPosition = transform.localPosition;
		bool flag = Visibility.Get(chunk.gameObject);
		List<Bounds> list = new List<Bounds>();
		for (int i = 0; i < trees.Length; i++)
		{
			if (i == 0 && bossDungeon.transform.parent == chunk.transform)
			{
				continue;
			}
			TreeType treeType = trees[i];
			if (treeType.subtypes.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < treeType.maxDensity; j++)
			{
				Vector3 normal;
				Vector3 randomPosition = GetRandomPosition(vertices, out normal);
				if (!IsSpawnPlace(localPosition, i, randomPosition, normal))
				{
					continue;
				}
				int subtype = UnityEngine.Random.Range(0, treeType.subtypes.Length);
				(GameObject, Bounds, bool) prefabBounds = treeType.GetPrefabBounds(subtype);
				GameObject gameObject = prefabBounds.Item1;
				Bounds item = prefabBounds.Item2;
				bool item2 = prefabBounds.Item3;
				Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, normal), normal);
				quaternion *= gameObject.transform.localRotation;
				Bounds bounds = ModelUtility.TransformBounds(item, randomPosition, quaternion);
				if (list.Any((Bounds bound) => bound.Intersects(bounds)))
				{
					if (item2)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
					continue;
				}
				randomPosition += gameObject.transform.localPosition.y * Vector3.up;
				if (item2)
				{
					gameObject.transform.SetParent(transform, worldPositionStays: false);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate(gameObject, transform);
				}
				gameObject.transform.localPosition = randomPosition;
				gameObject.transform.localRotation = quaternion;
				UnityEngine.Object.Destroy(gameObject.GetComponent<Furniture>());
				list.Add(bounds);
				Vector3 localScale = gameObject.transform.localScale;
				item.center = ModelUtility.Divide(item.center, localScale);
				item.size = ModelUtility.Divide(item.size, localScale);
				ModelUtility.MakeNavMeshObstacle(gameObject, item, local: true);
				if (!flag)
				{
					Visibility.Set(gameObject, value: false);
				}
				gameObject.SetActive(value: true);
				if (i == 0)
				{
					break;
				}
			}
		}
		if (NetworkServer.active && !chunk.WasFilled)
		{
			WorldArea area = base.transform.root.GetComponent<WorldArea>();
			GenerateObjects(localPosition, vertices, trees.Length, itemTypes, maxItemDensity, delegate(LDBItem it, Vector3 pos)
			{
				new NormalItem(it).PutDownLocal(area, pos);
			});
			GenerateObjects(localPosition, vertices, trees.Length + itemTypes.Length, creatureTypes, maxCreatureDensity, delegate(string c, Vector3 p)
			{
				SpawnHelper.instance.Spawn(c, area, p);
			});
		}
	}

	private void GenerateObjects<T>(Vector3 chunkPos, Vector3[] vertices, int offset, T[] types, int[] density, Action<T, Vector3> spawn)
	{
		for (int i = 0; i < types.Length; i++)
		{
			for (int j = 0; j < density[i]; j++)
			{
				Vector3 normal;
				Vector3 randomPosition = GetRandomPosition(vertices, out normal);
				if (IsSpawnPlace(chunkPos, offset + i, randomPosition, normal))
				{
					spawn(types[i], randomPosition);
				}
			}
		}
	}

	private static Vector3 GetRandomPosition(Vector3[] vertices, out Vector3 normal)
	{
		int num = UnityEngine.Random.Range(0, 32);
		int num2 = UnityEngine.Random.Range(0, 32);
		Vector3 vector = ((UnityEngine.Random.Range(0, 2) != 0) ? vertices[(num + 1) * 33 + num2 + 1] : vertices[num * 33 + num2]);
		Vector3 vector2 = vertices[num * 33 + num2 + 1] - vector;
		Vector3 vector3 = vertices[(num + 1) * 33 + num2] - vector;
		normal = Vector3.Cross(vector2, vector3).normalized;
		float value = UnityEngine.Random.value;
		float value2 = UnityEngine.Random.value;
		if (value + value2 <= 1f)
		{
			return vector + value * vector2 + value2 * vector3;
		}
		return vector + (1f - value) * vector2 + (1f - value2) * vector3;
	}

	private void ReadBiome(string name, out Color cloudColor, out string mapGround, out string mapWater, out string mapClouds)
	{
		ambience.ambientLight = Color.white;
		cloudColor = Color.clear;
		mapGround = null;
		mapWater = null;
		mapClouds = null;
		using StreamReader streamReader = GetBiomeFile(name);
		if (streamReader == null)
		{
			return;
		}
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			if (text == "WalkableOceans")
			{
				isOceanWalkable = true;
				continue;
			}
			if (text == "BurningOceans")
			{
				isOceanBurning = true;
				continue;
			}
			int num = text.IndexOf(' ');
			string text2 = "_" + text.Substring(0, num);
			string text3 = text.Substring(num + 1);
			switch (text2)
			{
			case "_MapGround":
				mapGround = text3;
				continue;
			case "_MapWater":
				mapWater = text3;
				continue;
			case "_MapClouds":
			{
				string[] array = text3.Split(new char[1] { ' ' }, 2);
				if (ColorUtility.TryParseHtmlString(array[0], out cloudColor))
				{
					mapClouds = array[1];
					continue;
				}
				break;
			}
			}
			Color color;
			if (float.TryParse(text3, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				switch (text2)
				{
				case "_OceanSpeed":
					oceanSpeed = result * 96f;
					continue;
				case "_Ocean":
					seaLevel = result;
					continue;
				case "_MountainStart":
					noiseData.mountainStart = result;
					continue;
				case "_MountainEnd":
					noiseData.mountainEnd = result;
					continue;
				case "_MountainAmplitude":
					noiseData.mountainAmplitude = result;
					continue;
				case "_MountainPersistence":
					noiseData.mountainPersistence = result;
					continue;
				case "_PlanePersistence":
					noiseData.planePersistence = result;
					continue;
				case "_PlaneAmplitude":
					noiseData.planeAmplitude = result;
					continue;
				case "_Octaves":
					noiseData.octaves = (int)result;
					continue;
				case "_Frequency":
					noiseData.frequency = result;
					continue;
				case "_BiomeFrequency":
					noiseData.biomeFrequency = result;
					continue;
				case "_BiomeOctaves":
					noiseData.biomeOctaves = (int)result;
					continue;
				case "_BiomePersistence":
					noiseData.biomePersistence = result;
					continue;
				case "_Lacunarity":
					noiseData.lacunarity = result;
					roadNoise.lacunarity = result;
					continue;
				case "_RiverCount":
					riverCount = (int)result;
					continue;
				case "_RiverFriction":
					riverFriction = result;
					continue;
				case "_RiverStraightness":
					riverStraightness = result;
					continue;
				case "_RoadOctaves":
					roadNoise.octaves = (int)result;
					continue;
				case "_RoadFrequency":
					roadNoise.frequency = result;
					continue;
				case "_RoadPersistence":
					roadNoise.gain = result;
					continue;
				}
				if (text2.EndsWith("Frequency"))
				{
					result /= 96f;
				}
				material.SetFloat(text2, result);
				if (oceanMaterial.HasProperty(text2))
				{
					oceanMaterial.SetFloat(text2, result);
				}
			}
			else if (ColorUtility.TryParseHtmlString(text3, out color))
			{
				if (text2 == "_Light")
				{
					ambience.ambientLight = color;
					continue;
				}
				material.SetColor(text2, color);
				if (oceanMaterial.HasProperty(text2))
				{
					oceanMaterial.SetColor(text2, color);
				}
			}
			else
			{
				Debug.LogError("Could not parse '" + text3 + "' for property '" + text2 + "'!");
			}
		}
	}

	private StreamReader GetBiomeFile(string name)
	{
		if (StreamingAssets.TryGetFile("Biomes/" + name + ".txt", out var path))
		{
			return File.OpenText(path);
		}
		FileInfo[] array = StreamingAssets.GetDirectoryContents("Biomes/" + name, "*.txt").ToArray();
		if (array.Length != 0)
		{
			return array[UnityEngine.Random.Range(0, array.Length)].OpenText();
		}
		Debug.LogError("Biome '" + name + "' not found!");
		return null;
	}

	public void Save(Stream stream)
	{
		if (chunks != null)
		{
			Chunk[,] array = chunks;
			foreach (Chunk chunk in array)
			{
				if (chunk.IsFilled || chunk.WasFilled)
				{
					HouseLoader.writeInt(chunk.ID, stream);
				}
			}
		}
		HouseLoader.writeInt(-1, stream);
	}

	public void Load(Stream stream, int version)
	{
		wasFilledChunks = new List<int>();
		int item;
		while ((item = HouseLoader.readInt(stream)) != -1)
		{
			wasFilledChunks.Add(item);
		}
	}

	private static IEnumerable<(int, int)> GetNeighboursLooped((int, int) pos)
	{
		return GetNeighboursLooped(pos.Item1, pos.Item2);
	}

	private static IEnumerable<(int, int)> GetNeighboursLooped(int x, int y, int min = 1, int max = 1)
	{
		int xx = -min;
		while (xx <= max)
		{
			int num;
			for (int yy = Math.Abs(xx) - min; yy <= max - Math.Abs(xx); yy = num)
			{
				if (xx != 0 || yy != 0)
				{
					int item = (xx + x + 191 - 1) % 191 + 1;
					int item2 = (yy + y + 191 - 1) % 191 + 1;
					yield return ValueTuple.Create(item, item2);
				}
				num = yy + 1;
			}
			num = xx + 1;
			xx = num;
		}
	}

	private static int FindLocalMaximum(IList<Vector3> vertices, int startIndex)
	{
		int num = startIndex;
		int num2;
		do
		{
			num2 = num;
			int result;
			int x = Math.DivRem(num2, 193, out result);
			float num3 = vertices[num2].y;
			num = -1;
			foreach (var item3 in GetNeighboursLooped(x, result))
			{
				int item = item3.Item1;
				int item2 = item3.Item2;
				int num4 = item * 193 + item2;
				float y = vertices[num4].y;
				if (!(y <= num3))
				{
					num3 = y;
					num = num4;
				}
			}
		}
		while (num != -1);
		return num2;
	}

	private IEnumerable<(int, int)> MakeRiver(IList<Vector3> vertices, ISet<(int, int)> taken, int startIndex)
	{
		float energy = 0f;
		int next = FindLocalMaximum(vertices, startIndex);
		while (vertices[next].y > SeaY - 2f)
		{
			int vertex = next;
			int x = vertex / 193;
			int y = vertex % 193;
			yield return (x, y);
			taken.Add((x, y));
			Vector3 value = vertices[vertex];
			float num = value.y + energy;
			value.y = Mathf.Max(value.y, SeaY) - 2f;
			vertices[vertex] = value;
			float num2 = float.PositiveInfinity;
			foreach (var item in GetNeighboursLooped(x, y))
			{
				int num3 = item.Item1 * 193 + item.Item2;
				float y2 = vertices[num3].y;
				if (!(y2 >= num) && !taken.Contains(item))
				{
					float num4 = (y2 - num) * (riverStraightness + UnityEngine.Random.value);
					if (num4 < num2)
					{
						num2 = num4;
						next = num3;
					}
				}
			}
			if (float.IsPositiveInfinity(num2))
			{
				foreach (var pos in GetNeighboursLooped(x, y, UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(1, 3)))
				{
					if (taken.Add(pos))
					{
						yield return pos;
						vertices[pos.Item1 * 193 + pos.Item2] -= Vector3.up * 2f;
					}
				}
				break;
			}
			energy = (num - vertices[next].y) / riverFriction;
		}
	}

	private IEnumerable<Mesh> MakeRiverMesh(IList<Vector3> landVertices)
	{
		UnityEngine.Random.InitState(seed);
		ISet<(int, int)> taken = new HashSet<(int, int)>();
		List<River> list = new List<River>();
		for (int i = 0; i < riverCount; i++)
		{
			IEnumerable<(int, int)> source = MakeRiver(landVertices, taken, UnityEngine.Random.Range(0, landVertices.Count));
			River.Add(list, source.SelectMany(GetNeighboursLooped));
		}
		foreach (River item in list)
		{
			ICollection<int> triangles;
			IEnumerable<Vector3> source2 = from v in item.GetTriangles(out triangles)
				select new Vector3((v.x - 96f) * 5f, GetHeight(v.x, v.y) + 0.05f, (v.y - 96f) * 5f);
			Mesh mesh = new Mesh
			{
				vertices = source2.ToArray(),
				triangles = triangles.ToArray()
			};
			mesh.Optimize();
			yield return mesh;
		}
	}
}
