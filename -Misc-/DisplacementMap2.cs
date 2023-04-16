using System.Collections.Generic;
using UnityEngine;

public class DisplacementMap2 : MonoBehaviour
{
	public enum PlanetaryBiome
	{
		Earthlike,
		Lava,
		Ice,
		Oil,
		Desert
	}

	private class TriangleData
	{
		public Vector3 position;

		public Vector3 normal;

		public TriangleData(Vector3 position)
		{
			if (position == Vector3.zero)
			{
				Debug.LogError("Position should not be zero.");
			}
			this.position = position;
			normal = Vector3.zero;
		}
	}

	public int minorSubdivideInterations = 2;

	public int furtherSubdivideIterations = 4;

	public float oceanSpeed = 1E-05f;

	private bool materialSet;

	private Vector3 oceanOffset;

	private Vector3[][] triangleArray;

	private Dictionary<Vector3, TriangleData> map;

	public PlanetaryBiome biome;

	public int seed;

	private PlanetTriangle[] planetTriangles;

	private int triangleWereOn;

	private void subdivide()
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
		triangleArray = array;
	}

	private Vector3 getNormal(Vector3[] triangle)
	{
		return Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]).normalized;
	}

	private Vector3 getNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Cross(b - a, c - a).normalized;
	}

	private Vector3 getWeightedNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Cross(b - a, c - a).normalized;
	}

	private void getMap()
	{
		map = new Dictionary<Vector3, TriangleData>();
		for (int i = 0; i < triangleArray.Length; i++)
		{
			Vector3 vector = triangleArray[i][0];
			Vector3 vector2 = triangleArray[i][1];
			Vector3 vector3 = triangleArray[i][2];
			if (!map.TryGetValue(vector, out var value))
			{
				value = new TriangleData(vector * getHeight(vector));
				map.Add(vector, value);
			}
			if (!map.TryGetValue(vector2, out var value2))
			{
				value2 = new TriangleData(vector2 * getHeight(vector2));
				map.Add(vector2, value2);
			}
			if (!map.TryGetValue(vector3, out var value3))
			{
				value3 = new TriangleData(vector3 * getHeight(vector3));
				map.Add(vector3, value3);
			}
			value.normal += getWeightedNormal(vector, vector2, vector3);
			value2.normal += getWeightedNormal(vector2, vector3, vector);
			value3.normal += getWeightedNormal(vector3, vector, vector2);
		}
	}

	private void Add(Dictionary<Vector3, Vector3> dict, Vector3 key, Vector3 value)
	{
		if (dict.TryGetValue(key, out var value2))
		{
			dict[key] = value2 + value;
		}
		else
		{
			dict.Add(key, value);
		}
	}

	public Mesh setMesh(int start, int end)
	{
		Mesh mesh = new Mesh();
		mesh.name = "Planet Triangle " + start / (end - start);
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>();
		int[] array = new int[3 * (end - start)];
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = triangleArray[start + i / 3][i % 3];
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
				if (value2.position == Vector3.zero)
				{
					MonoBehaviour.print(vector);
					MonoBehaviour.print(vector * getHeight(vector));
				}
				list.Add(value2.position);
				list2.Add(value2.normal);
			}
			array[i] = value;
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = array;
		mesh.normals = list2.ToArray();
		return mesh;
	}

	private void setFacing()
	{
		for (int i = 0; i < triangleArray.Length; i++)
		{
			if (Vector3.Dot(triangleArray[i][0], Vector3.Cross(triangleArray[i][1], triangleArray[i][2])) < 0f)
			{
				Vector3 vector = triangleArray[i][1];
				triangleArray[i][1] = triangleArray[i][2];
				triangleArray[i][2] = vector;
			}
		}
	}

	public Vector3 getGradient(Vector3 v, bool includeOcean = false, float epsilon = 0.0001f)
	{
		v.Normalize();
		Vector3 lhs = ((!(Mathf.Abs(v.y) > 0.5f)) ? Vector3.up : Vector3.forward);
		Vector3 normalized = Vector3.Cross(lhs, v).normalized;
		Vector3 normalized2 = Vector3.Cross(v, normalized).normalized;
		float num = (getHeight(v + epsilon * normalized, includeOcean) - getHeight(v - epsilon * normalized, includeOcean)) / (2f * epsilon);
		float num2 = (getHeight(v + epsilon * normalized2, includeOcean) - getHeight(v - epsilon * normalized2, includeOcean)) / (2f * epsilon);
		return normalized * num + normalized2 * num2;
	}

	public float getH(Vector3 v, bool includeOcean = false)
	{
		Material material = GetComponent<Renderer>().material;
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
		v = v.normalized / 2f;
		float num = Noise.SimplexNormal(v, octaves2, vector, float2, 1f, float5, float6);
		float amplitude;
		float persistence;
		if (num < float10)
		{
			amplitude = float3;
			persistence = float7;
		}
		else if (num < float11)
		{
			float num2 = (num - float10) / (float11 - float10);
			amplitude = float3 * (1f - num2) + float4 * num2;
			persistence = float7 * (1f - num2) + float8 * num2;
		}
		else
		{
			amplitude = float4;
			persistence = float8;
		}
		float num3 = Noise.SimplexNormal(v, octaves, vector, @float, amplitude, float5, persistence);
		if (includeOcean && num3 < float9)
		{
			num3 = float9;
		}
		return num3;
	}

	public float getHeight(Vector3 v, bool includeOcean = false)
	{
		return 0.5f + getH(v, includeOcean) * 0.05f;
	}

	public float getDepth(Vector3 v)
	{
		return GetComponent<Renderer>().material.GetFloat("_Ocean") - getH(v);
	}

	public bool isInOcean(Vector3 v)
	{
		return getDepth(v) > 0f;
	}

	private Dictionary<Vector3, List<TriangleData>> displace(Dictionary<Vector3, List<TriangleData>> map)
	{
		Material material = GetComponent<Renderer>().material;
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
		Dictionary<Vector3, List<TriangleData>> dictionary = new Dictionary<Vector3, List<TriangleData>>();
		int num = 0;
		foreach (KeyValuePair<Vector3, List<TriangleData>> item in map)
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
		if (materialSet)
		{
			return;
		}
		Random.InitState(seed);
		Material material = GetComponent<Renderer>().material;
		material.SetFloat("_BiomeOctaves", 3f);
		material.SetFloat("_RiverOctaves", 3f);
		material.SetFloat("_Octaves", 8f);
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
		material.SetVector("_Offset", new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * 100f);
		material.SetFloat("_Ocean", 0f);
		material.SetFloat("_MountainStart", 0.6f);
		material.SetFloat("_MountainEnd", 0.9f);
		material.SetColor("_SnowColor", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		switch (biome)
		{
		case PlanetaryBiome.Earthlike:
			material.SetColor("_OceanColorMid", new Color32(0, 0, byte.MaxValue, byte.MaxValue));
			material.SetFloat("_OceanOctaves", 0f);
			material.SetColor("_GrassColor", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			material.SetColor("_MountainColor", new Color32(128, 128, 128, byte.MaxValue));
			material.SetFloat("_Snow", 1f);
			break;
		case PlanetaryBiome.Ice:
			material.SetColor("_OceanColorMid", new Color32(192, 192, byte.MaxValue, byte.MaxValue));
			material.SetFloat("_OceanOctaves", 0f);
			material.SetFloat("_Snow", float.NegativeInfinity);
			material.SetColor("_GrassColor", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			material.SetColor("_MountainColor", new Color32(128, 128, 128, byte.MaxValue));
			break;
		case PlanetaryBiome.Lava:
			material.SetColor("_OceanColorStart", new Color32(64, 0, 0, byte.MaxValue));
			material.SetColor("_OceanColorMid", new Color32(192, 64, 0, byte.MaxValue));
			material.SetColor("_OceanColorEnd", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
			material.SetFloat("_OceanAmplitude", 0.5f);
			material.SetFloat("_OceanOctaves", 3f);
			material.SetFloat("_OceanPersistence", 0.8f);
			material.SetFloat("_OceanFrequency", 100f);
			material.SetFloat("_Snow", float.PositiveInfinity);
			material.SetFloat("_PlanePersistence", 0.8f);
			if ((double)Random.value > 0.5)
			{
				material.SetColor("_GrassColor", new Color32(128, 0, 0, byte.MaxValue));
				material.SetColor("_MountainColor", new Color32(128, 0, 0, byte.MaxValue));
			}
			else
			{
				material.SetColor("_GrassColor", new Color32(32, 32, 32, byte.MaxValue));
				material.SetColor("_MountainColor", new Color32(32, 32, 32, byte.MaxValue));
			}
			break;
		case PlanetaryBiome.Oil:
			material.SetColor("_OceanColorMid", Color.black);
			material.SetFloat("_OceanOctaves", 0f);
			material.SetFloat("_Snow", float.PositiveInfinity);
			material.SetColor("_GrassColor", new Color32(0, 0, 64, byte.MaxValue));
			material.SetColor("_MountainColor", new Color32(0, 0, 64, byte.MaxValue));
			break;
		case PlanetaryBiome.Desert:
			material.SetColor("_OceanColorMid", new Color(0f, 0f, 105f, 255f));
			material.SetFloat("_OceanOctaves", 0f);
			material.SetFloat("_Ocean", float.NegativeInfinity);
			material.SetFloat("_RiverFrequency", 0.5f);
			material.SetFloat("_Snow", float.PositiveInfinity);
			material.SetColor("_GrassColor", new Color32(191, 169, 0, byte.MaxValue));
			material.SetColor("_MountainColor", new Color32(92, 67, 0, byte.MaxValue));
			break;
		}
	}

	private void Start()
	{
		MonoBehaviour.print("Start time: " + Time.realtimeSinceStartup);
		setMaterial();
		float num = (1f + Mathf.Sqrt(5f)) / 2f;
		float num2 = 1f / Mathf.Sqrt(2f + num);
		num *= num2;
		num2 /= 2f;
		num /= 2f;
		triangleArray = new Vector3[20][]
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
			subdivide();
		}
		setMaterial();
		Material material = GetComponent<Renderer>().material;
		planetTriangles = new PlanetTriangle[triangleArray.Length];
		for (int j = 0; j < triangleArray.Length; j++)
		{
			int start = j << 2 * furtherSubdivideIterations;
			int end = j + 1 << 2 * furtherSubdivideIterations;
			planetTriangles[j] = PlanetTriangle.create(start, end, material, this);
		}
		for (int k = 0; k < furtherSubdivideIterations; k++)
		{
			subdivide();
		}
		getMap();
		GetComponent<MeshRenderer>().enabled = false;
		MonoBehaviour.print("End time: " + Time.realtimeSinceStartup);
	}

	private void Update()
	{
		if (biome == PlanetaryBiome.Lava)
		{
			oceanOffset += base.transform.InverseTransformVector(Vector3.up) * (oceanSpeed * base.transform.lossyScale.y);
		}
		if (triangleWereOn < planetTriangles.Length)
		{
			planetTriangles[triangleWereOn].activate = true;
			triangleWereOn++;
		}
	}
}
