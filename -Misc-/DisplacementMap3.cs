using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class DisplacementMap3 : MonoBehaviour
{
	private class Chunk
	{
		public DisplacementMap3 displacementMap;

		public Vector3[][] triangleArray;

		public bool calculatedVertices;

		public Vector3 center;

		private GameObject chunkObject;

		private Vector3 a;

		private Vector3 b;

		private Vector3 c;

		private GameObject ocean;

		private readonly int chunkNumber;

		private JobHandle job;

		private NativeArray<Vector3> vert;

		public bool startedCalculatingVertices;

		private float startTime;

		private void addChunk(Vector3 v)
		{
			if (!displacementMap.chunksMeetingAtPoint.TryGetValue(v, out var value))
			{
				value = new List<Chunk>();
				displacementMap.chunksMeetingAtPoint.Add(v, value);
			}
			value.Add(this);
		}

		public Chunk(Vector3[] triangle, DisplacementMap3 displacementMap, int chunkNumber)
		{
			triangleArray = new Vector3[1][] { triangle };
			this.displacementMap = displacementMap;
			a = triangle[0];
			b = triangle[1];
			c = triangle[2];
			addChunk(a);
			addChunk(b);
			addChunk(c);
			center = (a + b + c) / 3f;
			this.chunkNumber = chunkNumber;
		}

		public void calculateVertices()
		{
			if (!calculatedVertices)
			{
				calculatedVertices = true;
				if (!startedCalculatingVertices)
				{
					startCalculatingVertices();
				}
				else
				{
					MonoBehaviour.print(Time.time - startTime);
				}
				job.Complete();
				displacementMap.addToMap(triangleArray, vert.ToArray());
				vert.Dispose();
			}
		}

		public void startCalculatingVertices()
		{
			if (!startedCalculatingVertices)
			{
				startedCalculatingVertices = true;
				if (!calculatedVertices)
				{
					startTime = Time.time;
				}
				for (int i = 0; i < displacementMap.furtherSubdivideIterations; i++)
				{
					triangleArray = subdivide(triangleArray);
				}
				Vector3[] array = new Vector3[triangleArray.Length * 3];
				for (int j = 0; j < triangleArray.Length; j++)
				{
					array[3 * j] = triangleArray[j][0];
					array[3 * j + 1] = triangleArray[j][1];
					array[3 * j + 2] = triangleArray[j][2];
				}
				vert = new NativeArray<Vector3>(array, Allocator.Persistent);
				job = new DisplaceJob(vert, displacementMap.getMaterial(), displacementMap.seed).Schedule(array.Length, 32);
				JobHandle.ScheduleBatchedJobs();
			}
		}

		public GameObject getChunkObject()
		{
			if (chunkObject != null)
			{
				return chunkObject;
			}
			foreach (Chunk item in displacementMap.chunksMeetingAtPoint[a])
			{
				item.calculateVertices();
			}
			foreach (Chunk item2 in displacementMap.chunksMeetingAtPoint[b])
			{
				item2.calculateVertices();
			}
			foreach (Chunk item3 in displacementMap.chunksMeetingAtPoint[c])
			{
				item3.calculateVertices();
			}
			chunkObject = new GameObject("Chunk " + chunkNumber);
			chunkObject.transform.SetParent(displacementMap.transform, worldPositionStays: false);
			Mesh mesh = displacementMap.setMesh(triangleArray);
			chunkObject.AddComponent<MeshFilter>().mesh = mesh;
			chunkObject.AddComponent<MeshCollider>();
			chunkObject.AddComponent<MeshRenderer>().material = displacementMap.getMaterial();
			float @float = displacementMap.material.GetFloat("_Ocean");
			if (!float.IsInfinity(@float))
			{
				ocean = new GameObject("Ocean");
				ocean.transform.SetParent(chunkObject.transform, worldPositionStays: false);
				ocean.transform.localScale = Vector3.one * (1f + @float * 0.1f);
				ocean.AddComponent<MeshFilter>().sharedMesh = displacementMap.setSphereMesh(mesh);
				ocean.AddComponent<MeshRenderer>().sharedMaterial = displacementMap.oceanMat;
				if (displacementMap.isOceanWalkable)
				{
					ocean.AddComponent<MeshCollider>();
					ocean.AddComponent<OrientedNavMesh>().center = center;
				}
			}
			if (displacementMap.bridge != null)
			{
				int num = 32;
				UnityEngine.Random.InitState(displacementMap.seed ^ chunkNumber);
				Vector3 position = displacementMap.map[triangleArray[0][1]].position;
				Vector3 vector = displacementMap.map[triangleArray[0][1]].position - position;
				Vector3 vector2 = displacementMap.map[triangleArray[0][2]].position - position;
				Vector3 covectorA = Vector3.Cross(b, c);
				Vector3 covectorB = Vector3.Cross(c, position);
				Vector3 covectorC = Vector3.Cross(position, b);
				Vector3[] array = new Vector3[num * (num + 1) / 2];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					for (int j = 0; j < i; j++)
					{
						array[num2] = position + vector * ((float)i + 0.5f) / num + vector2 * ((float)j + 0.5f) / num;
						num2++;
					}
				}
				Vector3[] uniqueBridges = displacementMap.getUniqueBridges(array, covectorA, covectorB, covectorC);
				if (uniqueBridges.Length != 0)
				{
					Vector3[] bridgeDirections = displacementMap.getBridgeDirections(uniqueBridges);
					for (int k = 0; k < uniqueBridges.Length; k++)
					{
						Vector3 vector3 = uniqueBridges[k];
						if (!displacementMap.isInOcean(vector3))
						{
							GameObject gameObject = UnityEngine.Object.Instantiate(displacementMap.bridge, chunkObject.transform.parent);
							gameObject.SetActive(value: true);
							gameObject.transform.localPosition = vector3 * displacementMap.getHeight(vector3);
							gameObject.transform.SetParent(chunkObject.transform, worldPositionStays: true);
							Quaternion identity = Quaternion.identity;
							identity.SetLookRotation(bridgeDirections[k], vector3);
							gameObject.transform.localRotation = identity;
						}
					}
				}
			}
			if (displacementMap.treeTypes != null)
			{
				UnityEngine.Random.InitState(displacementMap.seed ^ chunkNumber);
				if (displacementMap.treeBounds == null)
				{
					displacementMap.treeBounds = new Bounds[displacementMap.treeTypes.Length][];
				}
				List<Bounds> list = new List<Bounds>();
				for (int l = 0; l < displacementMap.treeTypes.Length; l++)
				{
					if (displacementMap.treeTypes[l].Length == 0)
					{
						continue;
					}
					if (displacementMap.treeBounds[l] == null)
					{
						displacementMap.treeBounds[l] = new Bounds[displacementMap.treeTypes[l].Length];
					}
					for (int m = 0; m < displacementMap.maxTreeDensity[l]; m++)
					{
						Vector3[] array2 = triangleArray[UnityEngine.Random.Range(0, triangleArray.Length)];
						Vector3 position2 = displacementMap.map[array2[0]].position;
						Vector3 vector4 = displacementMap.map[array2[1]].position - position2;
						Vector3 vector5 = displacementMap.map[array2[2]].position - position2;
						Vector3 normalized = Vector3.Cross(vector4, vector5).normalized;
						float value = UnityEngine.Random.value;
						float value2 = UnityEngine.Random.value;
						Vector3 vector6 = ((!(value + value2 <= 1f)) ? (position2 + (1f - value) * vector4 + (1f - value2) * vector5) : (position2 + value * vector4 + value2 * vector5));
						if (Vector3.Dot(vector6.normalized, normalized) < 0.9f || UnityEngine.Random.value > displacementMap.getTreeDensity(vector6, l, l == 2, 20f * (2f * vector6.magnitude - 0.5f) <= @float))
						{
							continue;
						}
						int num3 = UnityEngine.Random.Range(0, displacementMap.treeTypes[l].Length);
						GameObject gameObject2 = displacementMap.treeTypes[l][num3];
						Bounds bounds = displacementMap.treeBounds[l][num3];
						bool flag = bounds == default(Bounds);
						if (flag)
						{
							gameObject2 = UnityEngine.Object.Instantiate(gameObject2);
							bounds = ModelUtility.GetBounds(gameObject2);
							bounds.center = ModelUtility.Divide(bounds.center, gameObject2.transform.localScale);
							bounds.extents = ModelUtility.Divide(bounds.extents, gameObject2.transform.localScale);
							displacementMap.treeBounds[l][num3] = bounds;
						}
						Vector3 vector7 = displacementMap.transform.TransformPoint(vector6);
						Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, normalized), normalized) * gameObject2.transform.localRotation;
						Bounds bounds2 = ModelUtility.TransformBounds(bounds, vector7, quaternion);
						bool flag2 = false;
						foreach (Bounds item4 in list)
						{
							if (item4.Intersects(bounds2))
							{
								flag2 = true;
								if (flag)
								{
									UnityEngine.Object.Destroy(gameObject2);
								}
								break;
							}
						}
						if (!flag2)
						{
							if (!flag)
							{
								gameObject2 = UnityEngine.Object.Instantiate(gameObject2, vector7, quaternion, chunkObject.transform);
								gameObject2.transform.localScale = displacementMap.transform.InverseTransformVector(gameObject2.transform.localScale);
							}
							else
							{
								gameObject2.transform.localPosition = vector7;
								gameObject2.transform.localRotation = quaternion;
								gameObject2.transform.SetParent(chunkObject.transform, worldPositionStays: true);
							}
							UnityEngine.Object.Destroy(gameObject2.GetComponent<Furniture>());
							ModelUtility.MakeNavMeshObstacle(gameObject2, bounds, local: true);
							gameObject2.SetActive(value: true);
							list.Add(bounds2);
						}
					}
				}
			}
			chunkObject.AddComponent<OrientedNavMesh>().center = center;
			return chunkObject;
		}

		public bool loaded()
		{
			return (object)chunkObject != null;
		}

		public bool visible()
		{
			if (chunkObject.layer == 0)
			{
				return chunkObject.activeSelf;
			}
			return false;
		}
	}

	private class VertexData
	{
		public Vector3 position;

		private Vector3 normal;

		private static int numberOfVertices;

		public int vertexNumber;

		public VertexData(Vector3 position)
		{
			this.position = position;
			normal = Vector3.zero;
			vertexNumber = numberOfVertices;
			numberOfVertices++;
		}

		public void addToNormal(Vector3 v)
		{
			normal += v;
		}

		public Vector3 getNormal()
		{
			return normal;
		}
	}

	private struct CalcHeightJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<Vector3> vertices;

		public NativeArray<float> result;

		private readonly int octaves;

		private readonly int biomeOctaves;

		private readonly float frequency;

		private readonly float biomeFrequency;

		private readonly float planeAmplitude;

		private readonly float mountainAmplitude;

		private readonly Vector3 offset;

		private readonly float lacunarity;

		private readonly float biomePersistence;

		private readonly float planePersistence;

		private readonly float mountainPersistence;

		private readonly float ocean;

		private readonly float mountainStart;

		private readonly float mountainEnd;

		private readonly int seed;

		public CalcHeightJob(NativeArray<Vector3> vertices, NativeArray<float> result, Material material, int seed)
		{
			this.vertices = vertices;
			this.result = result;
			octaves = (int)material.GetFloat("_Octaves");
			biomeOctaves = (int)material.GetFloat("_BiomeOctaves");
			frequency = material.GetFloat("_Frequency");
			biomeFrequency = material.GetFloat("_BiomeFrequency");
			planeAmplitude = material.GetFloat("_PlaneAmplitude");
			mountainAmplitude = material.GetFloat("_MountainAmplitude");
			offset = material.GetVector("_Offset");
			lacunarity = material.GetFloat("_Lacunarity");
			biomePersistence = material.GetFloat("_BiomePersistence");
			planePersistence = material.GetFloat("_PlanePersistence");
			mountainPersistence = material.GetFloat("_MountainPersistence");
			ocean = material.GetFloat("_Ocean");
			mountainStart = material.GetFloat("_MountainStart");
			mountainEnd = material.GetFloat("_MountainEnd");
			this.seed = seed;
		}

		public void Execute(int index)
		{
			result[index] = 0.5f + 0.05f * GetH(vertices[index], includeOcean: false, seed, octaves, biomeOctaves, frequency, biomeFrequency, planeAmplitude, mountainAmplitude, offset, lacunarity, biomePersistence, planePersistence, mountainPersistence, ocean, mountainStart, mountainEnd);
		}
	}

	private struct DisplaceJob : IJobParallelFor
	{
		public NativeArray<Vector3> vertices;

		private readonly int octaves;

		private readonly int biomeOctaves;

		private readonly float frequency;

		private readonly float biomeFrequency;

		private readonly float planeAmplitude;

		private readonly float mountainAmplitude;

		private readonly Vector3 offset;

		private readonly float lacunarity;

		private readonly float biomePersistence;

		private readonly float planePersistence;

		private readonly float mountainPersistence;

		private readonly float ocean;

		private readonly float mountainStart;

		private readonly float mountainEnd;

		private readonly int seed;

		public DisplaceJob(NativeArray<Vector3> vertices, Material material, int seed)
		{
			this.vertices = vertices;
			octaves = (int)material.GetFloat("_Octaves");
			biomeOctaves = (int)material.GetFloat("_BiomeOctaves");
			frequency = material.GetFloat("_Frequency");
			biomeFrequency = material.GetFloat("_BiomeFrequency");
			planeAmplitude = material.GetFloat("_PlaneAmplitude");
			mountainAmplitude = material.GetFloat("_MountainAmplitude");
			offset = material.GetVector("_Offset");
			lacunarity = material.GetFloat("_Lacunarity");
			biomePersistence = material.GetFloat("_BiomePersistence");
			planePersistence = material.GetFloat("_PlanePersistence");
			mountainPersistence = material.GetFloat("_MountainPersistence");
			ocean = material.GetFloat("_Ocean");
			mountainStart = material.GetFloat("_MountainStart");
			mountainEnd = material.GetFloat("_MountainEnd");
			this.seed = seed;
		}

		public void Execute(int index)
		{
			vertices[index] *= 0.5f + 0.05f * GetH(vertices[index], includeOcean: false, seed, octaves, biomeOctaves, frequency, biomeFrequency, planeAmplitude, mountainAmplitude, offset, lacunarity, biomePersistence, planePersistence, mountainPersistence, ocean, mountainStart, mountainEnd);
		}
	}

	public int minorSubdivideInterations = 2;

	public int furtherSubdivideIterations = 4;

	public float oceanSpeed = 0.001f;

	public bool isOceanWalkable;

	public Color ambientLight = Color.white;

	public float chunkLoadRadius = 0.3f;

	public bool automaticChunkLoading = true;

	public GameObject[][] treeTypes;

	public GameObject bridge;

	public int[] maxTreeDensity;

	public float highestValueForTreeDensity;

	public ComputeShader displacementComputeShader;

	public Material oceanMat;

	private Material material;

	private Vector3 oceanOffset;

	private Vector3[][] chunkArray;

	private Dictionary<Vector3, VertexData> map;

	private Dictionary<Vector3, List<Chunk>> chunksMeetingAtPoint;

	private Chunk[] chunks;

	private Bounds[][] treeBounds;

	public string biome;

	public int seed;

	private int chunkWeAreOn;

	private static int oceanOffsetID;

	private static Vector3[][] subdivide(Vector3[][] triangleArray)
	{
		Vector3[][] array = new Vector3[triangleArray.Length * 4][];
		for (int i = 0; i < triangleArray.Length; i++)
		{
			Vector3 vector = triangleArray[i][0];
			Vector3 vector2 = triangleArray[i][1];
			Vector3 vector3 = triangleArray[i][2];
			Vector3 vector4 = (vector + vector2).normalized / 2f;
			Vector3 vector5 = (vector2 + vector3).normalized / 2f;
			Vector3 vector6 = (vector3 + vector).normalized / 2f;
			array[4 * i] = new Vector3[3] { vector, vector4, vector6 };
			array[4 * i + 1] = new Vector3[3] { vector2, vector5, vector4 };
			array[4 * i + 2] = new Vector3[3] { vector3, vector6, vector5 };
			array[4 * i + 3] = new Vector3[3] { vector4, vector5, vector6 };
		}
		return array;
	}

	private static Vector3 getNormal(Vector3[] triangle)
	{
		return Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]).normalized;
	}

	private static Vector3 getNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Cross(b - a, c - a).normalized;
	}

	private static Vector3 getWeightedNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Cross(b - a, c - a).normalized;
	}

	private static void precisePrint(Vector3 v)
	{
		MonoBehaviour.print("(" + v.x + ",\t" + v.y + ",\t" + v.z + ")");
	}

	private Vector3[] displaceArray(Vector3[] vertices)
	{
		using NativeArray<Vector3> vertices2 = new NativeArray<Vector3>(vertices, Allocator.TempJob);
		new DisplaceJob(vertices2, material, seed).Schedule(vertices.Length, 32).Complete();
		return vertices2.ToArray();
	}

	private float[] getHeights(Vector3[] vertices)
	{
		using NativeArray<float> result = new NativeArray<float>(vertices.Length, Allocator.TempJob);
		using NativeArray<Vector3> vertices2 = new NativeArray<Vector3>(vertices, Allocator.TempJob);
		new CalcHeightJob(vertices2, result, material, seed).Schedule(vertices.Length, 32).Complete();
		return result.ToArray();
	}

	public void printVector(Vector3 v)
	{
		MonoBehaviour.print("(" + v.x + ",\t" + v.y + ",\t" + v.z + ")");
	}

	public Vector3 gradTest(Vector3 vertex)
	{
		Vector3[] array = new Vector3[1] { vertex };
		ComputeBuffer computeBuffer = new ComputeBuffer(array.Length, 12);
		computeBuffer.SetData(array);
		int kernelIndex = displacementComputeShader.FindKernel("GradTest");
		displacementComputeShader.SetBuffer(kernelIndex, "io", computeBuffer);
		displacementComputeShader.Dispatch(kernelIndex, array.Length, 1, 1);
		_ = new Vector3[array.Length];
		Vector3[] array2 = new Vector3[array.Length];
		computeBuffer.GetData(array2);
		computeBuffer.Dispose();
		return array2[0];
	}

	private Vector3[] getBridges(Vector3[] vertices, Vector3 covectorA, Vector3 covectorB, Vector3 covectorC)
	{
		ComputeBuffer computeBuffer = new ComputeBuffer(vertices.Length, 12);
		computeBuffer.SetData(vertices);
		int kernelIndex = displacementComputeShader.FindKernel("FindBridge");
		displacementComputeShader.SetBuffer(kernelIndex, "io", computeBuffer);
		displacementComputeShader.SetVector("_CovectorA", covectorA);
		displacementComputeShader.SetVector("_CovectorB", covectorB);
		displacementComputeShader.SetVector("_CovectorC", covectorC);
		displacementComputeShader.Dispatch(kernelIndex, vertices.Length, 1, 1);
		_ = new Vector3[vertices.Length];
		Vector3[] array = new Vector3[vertices.Length];
		computeBuffer.GetData(array);
		computeBuffer.Dispose();
		return array;
	}

	private Vector3[] getUniqueBridges(Vector3[] vertices, Vector3 covectorA, Vector3 covectorB, Vector3 covectorC)
	{
		List<Vector3> list = new List<Vector3>();
		Vector3[] bridges = getBridges(vertices, covectorA, covectorB, covectorC);
		foreach (Vector3 vector in bridges)
		{
			if (vector == Vector3.zero)
			{
				continue;
			}
			bool flag = true;
			foreach (Vector3 item in list)
			{
				if ((double)(vector - item).sqrMagnitude <= 0.001)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(vector);
			}
		}
		return list.ToArray();
	}

	private Vector3[] getBridgeDirections(Vector3[] vertices)
	{
		ComputeBuffer computeBuffer = new ComputeBuffer(vertices.Length, 12);
		computeBuffer.SetData(vertices);
		int kernelIndex = displacementComputeShader.FindKernel("RiverGrad");
		displacementComputeShader.SetBuffer(kernelIndex, "io", computeBuffer);
		displacementComputeShader.Dispatch(kernelIndex, vertices.Length, 1, 1);
		_ = new Vector3[vertices.Length];
		Vector3[] array = new Vector3[vertices.Length];
		computeBuffer.GetData(array);
		computeBuffer.Dispose();
		return array;
	}

	private void addToMap(Vector3[][] triangleArray, Vector3[] displacedVertexArray = null)
	{
		for (int i = 0; i < triangleArray.Length; i++)
		{
			Vector3 vector = triangleArray[i][0];
			Vector3 vector2 = triangleArray[i][1];
			Vector3 vector3 = triangleArray[i][2];
			VertexData value;
			if (displacedVertexArray != null)
			{
				value = new VertexData(displacedVertexArray[3 * i]);
				map[vector] = value;
			}
			else if (!map.TryGetValue(vector, out value))
			{
				value = new VertexData(vector * getHeight(vector));
				map.Add(vector, value);
			}
			VertexData value2;
			if (displacedVertexArray != null)
			{
				value2 = new VertexData(displacedVertexArray[3 * i + 1]);
				map[vector2] = value2;
			}
			else if (!map.TryGetValue(vector2, out value2))
			{
				value2 = new VertexData(vector2 * getHeight(vector2));
				map.Add(vector2, value2);
			}
			VertexData value3;
			if (displacedVertexArray != null)
			{
				value3 = new VertexData(displacedVertexArray[3 * i + 2]);
				map[vector3] = value3;
			}
			else if (!map.TryGetValue(vector3, out value3))
			{
				value3 = new VertexData(vector3 * getHeight(vector3));
				map.Add(vector3, value3);
			}
			Vector3 weightedNormal = getWeightedNormal(value.position, value2.position, value3.position);
			value.addToNormal(weightedNormal);
			value2.addToNormal(weightedNormal);
			value3.addToNormal(weightedNormal);
		}
	}

	public Mesh setMesh(Vector3[][] triangleArray)
	{
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>();
		int[] array = new int[3 * triangleArray.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = triangleArray[i / 3][i % 3];
			int value = 0;
			if (!dictionary.TryGetValue(vector, out value))
			{
				value = dictionary.Count;
				dictionary.Add(vector, value);
				if (!map.TryGetValue(vector, out var value2))
				{
					Vector3 vector2 = vector;
					Debug.LogError("Error: Vector " + vector2.ToString() + " missing.");
				}
				list.Add(value2.position);
				list2.Add(value2.getNormal());
			}
			array[i] = value;
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = array;
		mesh.normals = list2.ToArray();
		mesh.RecalculateBounds();
		return mesh;
	}

	public Mesh setSphereMesh(Mesh terrainMesh)
	{
		Vector3[] vertices = terrainMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = vertices[i].normalized / 4f;
		}
		return new Mesh
		{
			vertices = vertices,
			normals = vertices,
			triangles = terrainMesh.triangles
		};
	}

	private void setFacing()
	{
		for (int i = 0; i < chunkArray.Length; i++)
		{
			if (Vector3.Dot(chunkArray[i][0], Vector3.Cross(chunkArray[i][1], chunkArray[i][2])) < 0f)
			{
				Vector3 vector = chunkArray[i][1];
				chunkArray[i][1] = chunkArray[i][2];
				chunkArray[i][2] = vector;
			}
		}
	}

	public Vector3 getGradient(Vector3 v, bool includeOcean = false, float epsilon = 0.0001f)
	{
		v.Normalize();
		Vector3 lhs = ((!(Mathf.Abs(v.y) > 0.5f)) ? Vector3.up : Vector3.forward);
		Vector3 normalized = Vector3.Cross(lhs, v).normalized;
		Vector3 normalized2 = Vector3.Cross(v, normalized).normalized;
		float[] height = getHeight(new Vector3[4]
		{
			v + epsilon * normalized,
			v - epsilon * normalized,
			v + epsilon * normalized2,
			v - epsilon * normalized2
		}, includeOcean);
		float num = (height[0] - height[1]) / (2f * epsilon);
		float num2 = (height[2] - height[3]) / (2f * epsilon);
		return normalized * num + normalized2 * num2;
	}

	public float getH(Vector3 v, bool includeOcean = false)
	{
		int octaves = (int)material.GetFloat("_Octaves");
		int biomeOctaves = (int)material.GetFloat("_BiomeOctaves");
		float @float = material.GetFloat("_Frequency");
		float float2 = material.GetFloat("_BiomeFrequency");
		float float3 = material.GetFloat("_PlaneAmplitude");
		float float4 = material.GetFloat("_MountainAmplitude");
		Vector3 offset = material.GetVector("_Offset");
		float float5 = material.GetFloat("_Lacunarity");
		float float6 = material.GetFloat("_BiomePersistence");
		float float7 = material.GetFloat("_PlanePersistence");
		float float8 = material.GetFloat("_MountainPersistence");
		float float9 = material.GetFloat("_Ocean");
		float float10 = material.GetFloat("_MountainStart");
		float float11 = material.GetFloat("_MountainEnd");
		return GetH(v, includeOcean, seed, octaves, biomeOctaves, @float, float2, float3, float4, offset, float5, float6, float7, float8, float9, float10, float11);
	}

	private static float GetH(Vector3 v, bool includeOcean, int seed, int octaves, int biomeOctaves, float frequency, float biomeFrequency, float planeAmplitude, float mountainAmplitude, Vector3 offset, float lacunarity, float biomePersistence, float planePersistence, float mountainPersistence, float ocean, float mountainStart, float mountainEnd)
	{
		v = v.normalized / 2f;
		FastNoise fastNoise = new FastNoise(seed);
		fastNoise.SetFractalOctaves(biomeOctaves);
		fastNoise.SetFrequency(biomeFrequency);
		fastNoise.SetFractalLacunarity(lacunarity);
		fastNoise.SetFractalGain(biomePersistence);
		float simplexFractal = fastNoise.GetSimplexFractal(v.x, v.y, v.z, ignoreBounding: true);
		float num;
		float fractalGain;
		if (simplexFractal < mountainStart)
		{
			num = planeAmplitude;
			fractalGain = planePersistence;
		}
		else if (simplexFractal < mountainEnd)
		{
			float num2 = (simplexFractal - mountainStart) / (mountainEnd - mountainStart);
			num = planeAmplitude * (1f - num2) + mountainAmplitude * num2;
			fractalGain = planePersistence * (1f - num2) + mountainPersistence * num2;
		}
		else
		{
			num = mountainAmplitude;
			fractalGain = mountainPersistence;
		}
		fastNoise.SetFractalOctaves(octaves);
		fastNoise.SetFrequency(frequency);
		fastNoise.SetFractalGain(fractalGain);
		float num3 = num * fastNoise.GetSimplexFractal(v.x, v.y, v.z, ignoreBounding: true);
		if (includeOcean && num3 < ocean)
		{
			num3 = ocean;
		}
		return num3;
	}

	public float getHeight(Vector3 v, bool includeOcean = false)
	{
		return 0.5f + getH(v, includeOcean) * 0.05f;
	}

	public float[] getHeight(Vector3[] v, bool includeOcean = false)
	{
		float[] heights = getHeights(v);
		for (int i = 0; i < heights.Length; i++)
		{
			heights[i] = 0.5f + (includeOcean ? Mathf.Max(0f, heights[i]) : heights[i]) * 0.05f;
		}
		return heights;
	}

	public float getDepth(Vector3 v)
	{
		float @float = material.GetFloat("_Ocean");
		if (float.IsInfinity(@float))
		{
			return @float;
		}
		return @float - getH(v);
	}

	public float[] getDepth(Vector3[] v)
	{
		float @float = GetComponent<Renderer>().material.GetFloat("_Ocean");
		float[] array = new float[v.Length];
		if (float.IsInfinity(@float))
		{
			for (int i = 0; i < v.Length; i++)
			{
				array[i] = @float;
			}
		}
		else
		{
			float[] heights = getHeights(v);
			for (int j = 0; j < v.Length; j++)
			{
				array[j] = @float - heights[j];
			}
		}
		return array;
	}

	public bool isInOcean(Vector3 v)
	{
		return getDepth(v) > 0f;
	}

	public bool[] isInOcean(Vector3[] v)
	{
		float[] depth = getDepth(v);
		bool[] array = new bool[v.Length];
		for (int i = 0; i < v.Length; i++)
		{
			array[i] = depth[i] > 0f;
		}
		return array;
	}

	public float getTreeDensity(Vector3 v, int type, bool village = false)
	{
		return getTreeDensity(v, type, village, isInOcean(v));
	}

	public float getTreeDensity(Vector3 v, int type, bool village, bool inOcean)
	{
		if (inOcean)
		{
			return 0f;
		}
		FastNoise fastNoise = new FastNoise(seed * type);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFrequency(1.5f);
		fastNoise.SetFractalLacunarity(village ? 1f : 1.92f);
		fastNoise.SetFractalGain(village ? 0.1f : 0.8f);
		float f = 3.3333333f * fastNoise.GetSimplexFractal(v.x, v.y, v.z, ignoreBounding: true);
		f = Mathf.Atan(f) / (float)Math.PI + 0.5f;
		if (village)
		{
			f = Mathf.Max(0f, f - 0.8f) * 5f;
		}
		highestValueForTreeDensity = Mathf.Max(f, highestValueForTreeDensity);
		return f;
	}

	private Dictionary<Vector3, List<VertexData>> displace(Dictionary<Vector3, List<VertexData>> map)
	{
		int octaves = (int)material.GetFloat("_Octaves");
		int octaves2 = (int)material.GetFloat("_BiomeOctaves");
		float @float = material.GetFloat("_Frequency");
		float float2 = material.GetFloat("_BiomeFrequency");
		float float3 = material.GetFloat("_PlaneAmplitude");
		float float4 = material.GetFloat("_MountainAmplitude");
		Vector3 vector = material.GetVector("_Offset");
		float float5 = material.GetFloat("_Lacunarity");
		float float6 = material.GetFloat("_BiomePersistence");
		float float7 = material.GetFloat("_PlanePersistence");
		float float8 = material.GetFloat("_MountainPersistence");
		float float9 = material.GetFloat("_Ocean");
		float float10 = material.GetFloat("_MountainStart");
		float float11 = material.GetFloat("_MountainEnd");
		Dictionary<Vector3, List<VertexData>> dictionary = new Dictionary<Vector3, List<VertexData>>();
		int num = 0;
		foreach (KeyValuePair<Vector3, List<VertexData>> item in map)
		{
			float num2 = Noise.SimplexNormal(item.Key, octaves2, vector, float2, 1f, float5, float6);
			float amplitude;
			float persistence;
			if (num2 < float10)
			{
				amplitude = float3;
				persistence = float7;
			}
			else if (num2 < float11)
			{
				float num3 = (num2 - float10) / (float11 - float10);
				amplitude = float3 * (1f - num3) + float4 * num3;
				persistence = float7 * (1f - num3) + float8 * num3;
			}
			else
			{
				amplitude = float4;
				persistence = float8;
			}
			float num4 = Noise.SimplexNormal(item.Key, octaves, vector, @float, amplitude, float5, persistence);
			if (num4 < float9)
			{
				num4 = float9;
			}
			dictionary.Add(item.Key * (1f + num4 * 0.1f), item.Value);
			num++;
		}
		return dictionary;
	}

	public void setMaterial()
	{
		if (!(material != null))
		{
			UnityEngine.Random.InitState(seed);
			material = GetComponent<Renderer>().material;
			material.SetInt("_Displace", 0);
			material.SetFloat("_BiomeOctaves", 3f);
			material.SetFloat("_RiverOctaves", 3f);
			material.SetFloat("_Octaves", 4f);
			material.SetFloat("_BiomeFrequency", 1.5f);
			material.SetFloat("_Frequency", 1.5f);
			material.SetFloat("_RiverFrequency", 4f);
			material.SetFloat("_PlaneAmplitude", 0.5f);
			material.SetFloat("_MountainAmplitude", 0.7f);
			material.SetFloat("_RiverThickness", 0.02f);
			material.SetFloat("_Lacunarity", 1.92f);
			material.SetFloat("_BiomePersistence", 0.8f);
			material.SetFloat("_PlanePersistence", 0.5f);
			material.SetFloat("_MountainPersistence", 0.8f);
			material.SetFloat("_Persistence", 0.8f);
			material.SetVector("_Offset", new Vector3(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f) * 100f);
			material.SetFloat("_Ocean", 0f);
			material.SetFloat("_MountainStart", 0.6f);
			material.SetFloat("_MountainEnd", 0.9f);
			if (GetComponent<Planet>() != null)
			{
				material.SetFloat("_RoadFrequency", 3f);
				material.SetFloat("_RoadPersistence", 0f);
				material.SetFloat("_RoadStart", 0.005f);
				material.SetFloat("_RoadEnd", 0.01f);
				material.SetColor("_RoadColorStart", new Color(0.5f, 0.4f, 0.25f));
				material.SetColor("_RoadColorEnd", new Color(0.45f, 0.35f, 0.2f));
			}
			ReadBiome(biome, material);
			string[] array = new string[18]
			{
				"_Octaves", "_BiomeOctaves", "_Frequency", "_BiomeFrequency", "_PlaneAmplitude", "_MountainAmplitude", "_Lacunarity", "_BiomePersistence", "_PlanePersistence", "_MountainPersistence",
				"_MountainStart", "_MountainEnd", "_RiverOctaves", "_RiverFrequency", "_RiverPersistence", "_RoadOctaves", "_RoadFrequency", "_RoadPersistence"
			};
			foreach (string text in array)
			{
				displacementComputeShader.SetFloat(text, material.GetFloat(text));
			}
			displacementComputeShader.SetVector("_Road1Offset", material.GetVector("_Road1Offset"));
			displacementComputeShader.SetVector("_Road2Offset", material.GetVector("_Road2Offset"));
			displacementComputeShader.SetVector("_Offset", material.GetVector("_Offset"));
			displacementComputeShader.SetFloat("_Epsilon", 0.0001f);
			displacementComputeShader.SetFloat("_Tries", 32f);
		}
	}

	private void ReadBiome(string name, Material mat)
	{
		StreamReader streamReader;
		try
		{
			streamReader = File.OpenText(Application.streamingAssetsPath + "/Biomes/" + name + ".txt");
		}
		catch
		{
			try
			{
				FileInfo[] files = new DirectoryInfo(Application.streamingAssetsPath + "/Biomes/" + name + "/").GetFiles("*.txt", SearchOption.TopDirectoryOnly);
				streamReader = files[UnityEngine.Random.Range(0, files.Length)].OpenText();
			}
			catch
			{
				Debug.LogError("Biome '" + name + "' not found!");
				return;
			}
		}
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (text == "WalkableOceans")
			{
				isOceanWalkable = true;
				continue;
			}
			int num = text.IndexOf(' ');
			string text2 = "_" + text.Substring(0, num);
			string text3 = text.Substring(num + 1);
			Color color;
			if (float.TryParse(text3, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				if (text2 == "_OceanSpeed")
				{
					oceanSpeed = result;
					continue;
				}
				mat.SetFloat(text2, result);
				if (oceanMat.HasProperty(text2))
				{
					oceanMat.SetFloat(text2, result);
				}
			}
			else if (ColorUtility.TryParseHtmlString(text3, out color))
			{
				if (text2 == "_Light")
				{
					ambientLight = color;
					continue;
				}
				mat.SetColor(text2, color);
				if (oceanMat.HasProperty(text2))
				{
					oceanMat.SetColor(text2, color);
				}
			}
			else
			{
				Debug.LogError("Could not parse '" + text3 + "' for property '" + text2 + "'!");
			}
		}
	}

	public Material getMaterial()
	{
		return material;
	}

	public GameObject getChunk(int index)
	{
		return chunks[index].getChunkObject();
	}

	public void Start()
	{
		if (map == null)
		{
			MonoBehaviour.print("Start time: " + Time.realtimeSinceStartup);
			map = new Dictionary<Vector3, VertexData>();
			chunksMeetingAtPoint = new Dictionary<Vector3, List<Chunk>>();
			seed--;
			do
			{
				seed++;
				material = null;
				setMaterial();
			}
			while (isInOcean(Vector3.up) || isInOcean(Vector3.down));
			float num = (1f + Mathf.Sqrt(5f)) / 2f;
			float num2 = 1f / Mathf.Sqrt(2f + num);
			num *= num2;
			num2 /= 2f;
			num /= 2f;
			chunkArray = new Vector3[20][]
			{
				new Vector3[3]
				{
					new Vector3(0f, num2, num),
					new Vector3(num2, num, 0f),
					new Vector3(num, 0f, num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, num2, 0f - num),
					new Vector3(num2, num, 0f),
					new Vector3(num, 0f, 0f - num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, 0f - num2, num),
					new Vector3(num2, 0f - num, 0f),
					new Vector3(num, 0f, num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, 0f - num2, 0f - num),
					new Vector3(num2, 0f - num, 0f),
					new Vector3(num, 0f, 0f - num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, num2, num),
					new Vector3(0f - num2, num, 0f),
					new Vector3(0f - num, 0f, num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, num2, 0f - num),
					new Vector3(0f - num2, num, 0f),
					new Vector3(0f - num, 0f, 0f - num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, 0f - num2, num),
					new Vector3(0f - num2, 0f - num, 0f),
					new Vector3(0f - num, 0f, num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, 0f - num2, 0f - num),
					new Vector3(0f - num2, 0f - num, 0f),
					new Vector3(0f - num, 0f, 0f - num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, num2, num),
					new Vector3(0f, 0f - num2, num),
					new Vector3(num, 0f, num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, num2, num),
					new Vector3(0f, 0f - num2, num),
					new Vector3(0f - num, 0f, num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, num2, 0f - num),
					new Vector3(0f, 0f - num2, 0f - num),
					new Vector3(num, 0f, 0f - num2)
				},
				new Vector3[3]
				{
					new Vector3(0f, num2, 0f - num),
					new Vector3(0f, 0f - num2, 0f - num),
					new Vector3(0f - num, 0f, 0f - num2)
				},
				new Vector3[3]
				{
					new Vector3(num2, num, 0f),
					new Vector3(0f - num2, num, 0f),
					new Vector3(0f, num2, num)
				},
				new Vector3[3]
				{
					new Vector3(num2, num, 0f),
					new Vector3(0f - num2, num, 0f),
					new Vector3(0f, num2, 0f - num)
				},
				new Vector3[3]
				{
					new Vector3(num2, 0f - num, 0f),
					new Vector3(0f - num2, 0f - num, 0f),
					new Vector3(0f, 0f - num2, num)
				},
				new Vector3[3]
				{
					new Vector3(num2, 0f - num, 0f),
					new Vector3(0f - num2, 0f - num, 0f),
					new Vector3(0f, 0f - num2, 0f - num)
				},
				new Vector3[3]
				{
					new Vector3(num, 0f, num2),
					new Vector3(num, 0f, 0f - num2),
					new Vector3(num2, num, 0f)
				},
				new Vector3[3]
				{
					new Vector3(num, 0f, num2),
					new Vector3(num, 0f, 0f - num2),
					new Vector3(num2, 0f - num, 0f)
				},
				new Vector3[3]
				{
					new Vector3(0f - num, 0f, num2),
					new Vector3(0f - num, 0f, 0f - num2),
					new Vector3(0f - num2, num, 0f)
				},
				new Vector3[3]
				{
					new Vector3(0f - num, 0f, num2),
					new Vector3(0f - num, 0f, 0f - num2),
					new Vector3(0f - num2, 0f - num, 0f)
				}
			};
			setFacing();
			for (int i = 0; i < minorSubdivideInterations; i++)
			{
				chunkArray = subdivide(chunkArray);
			}
			chunks = new Chunk[chunkArray.Length];
			for (int j = 0; j < chunkArray.Length; j++)
			{
				chunks[j] = new Chunk(chunkArray[j], this, j);
			}
			GetComponent<MeshRenderer>().enabled = false;
			MonoBehaviour.print("End time: " + Time.realtimeSinceStartup);
		}
	}

	private void Update()
	{
		if (oceanSpeed != 0f)
		{
			oceanOffset += base.transform.InverseTransformVector(Vector3.up) * (oceanSpeed * base.transform.lossyScale.y * Time.deltaTime);
			if (oceanOffsetID == 0)
			{
				oceanOffsetID = Shader.PropertyToID("_OceanOffset");
			}
			material.SetVector(oceanOffsetID, oceanOffset);
			oceanMat.SetVector(oceanOffsetID, oceanOffset);
		}
		if (automaticChunkLoading && chunkWeAreOn < chunks.Length)
		{
			chunks[chunkWeAreOn].getChunkObject();
			chunkWeAreOn++;
		}
	}

	public void LoadChunksNearPoints(Vector3[] points)
	{
		Chunk chunk = null;
		Chunk chunk2 = null;
		float num = chunkLoadRadius * chunkLoadRadius * 4f;
		float num2 = chunkLoadRadius * chunkLoadRadius;
		Chunk[] array = chunks;
		foreach (Chunk chunk3 in array)
		{
			float num3;
			Vector3[] array2;
			if (!chunk3.loaded() || !chunk3.visible())
			{
				array2 = points;
				for (int j = 0; j < array2.Length; j++)
				{
					num3 = Vector3.SqrMagnitude(array2[j] - chunk3.center);
					if (num3 < num && (num3 < chunkLoadRadius * chunkLoadRadius || !chunk3.calculatedVertices))
					{
						num = num3;
						chunk = chunk3;
					}
				}
				continue;
			}
			num3 = float.PositiveInfinity;
			array2 = points;
			foreach (Vector3 vector in array2)
			{
				num3 = Mathf.Min(num3, Vector3.SqrMagnitude(vector - chunk3.center));
			}
			if (num3 > num2)
			{
				num2 = num3;
				chunk2 = chunk3;
			}
		}
		if (chunk != null)
		{
			if (num < chunkLoadRadius * chunkLoadRadius)
			{
				WorldManager.SetAreaActive(chunk.getChunkObject(), to: true);
			}
			else
			{
				chunk.startCalculatingVertices();
			}
		}
		if (chunk2 != null)
		{
			WorldManager.SetAreaActive(chunk2.getChunkObject(), to: false);
		}
	}
}
