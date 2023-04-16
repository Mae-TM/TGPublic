using System.Collections.Generic;
using UnityEngine;

public class DisplacementMap : MonoBehaviour
{
	public enum PlanetaryBiome
	{
		Earthlike,
		Lava,
		Ice,
		Oil,
		Desert
	}

	private struct TriangleData
	{
		public int number;

		public int vertex;

		public Vector3 normal;

		public TriangleData(int number, int vertex, Vector3 normal)
		{
			this.number = number;
			this.vertex = vertex;
			this.normal = normal;
		}
	}

	public int subdivideIterations = 6;

	public float oceanSpeed = 1E-05f;

	private bool materialSet;

	public PlanetaryBiome biome;

	public int seed;

	private Material[] material;

	private void subdivide(ref Vector3[][] triangles)
	{
		Vector3[][] array = new Vector3[triangles.Length * 4][];
		for (int i = 0; i < triangles.Length; i++)
		{
			Vector3 vector = triangles[i][0];
			Vector3 vector2 = triangles[i][1];
			Vector3 vector3 = triangles[i][2];
			Vector3 vector4 = (vector + vector2).normalized / 2f;
			Vector3 vector5 = (vector2 + vector3).normalized / 2f;
			Vector3 vector6 = (vector3 + vector).normalized / 2f;
			array[4 * i] = new Vector3[3] { vector, vector4, vector6 };
			array[4 * i + 1] = new Vector3[3] { vector2, vector5, vector4 };
			array[4 * i + 2] = new Vector3[3] { vector3, vector6, vector5 };
			array[4 * i + 3] = new Vector3[3] { vector4, vector5, vector6 };
		}
		triangles = array;
	}

	private Vector3 getNormal(Vector3[] triangle)
	{
		return Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[0]).normalized;
	}

	private Dictionary<Vector3, List<TriangleData>> getMap(Vector3[][] inMesh)
	{
		Dictionary<Vector3, List<TriangleData>> dictionary = new Dictionary<Vector3, List<TriangleData>>();
		for (int i = 0; i < inMesh.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (!dictionary.TryGetValue(inMesh[i][j], out var value))
				{
					value = new List<TriangleData>();
					dictionary.Add(inMesh[i][j], value);
				}
				value.Add(new TriangleData(i, j, getNormal(inMesh[i])));
			}
		}
		return dictionary;
	}

	private void setMesh(Mesh mesh, Pair<Vector3, List<TriangleData>>[] territory, int numberOfTriangles, int sectionNumber)
	{
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		int[] array = new int[3 * numberOfTriangles];
		int num = 0;
		int num2 = numberOfTriangles * sectionNumber;
		foreach (Pair<Vector3, List<TriangleData>> pair in territory)
		{
			bool flag = false;
			for (int j = 0; j < pair.b.Count; j++)
			{
				int number = pair.b[j].number;
				if (num2 <= number && number < num2 + numberOfTriangles)
				{
					array[3 * (number - num2) + pair.b[j].vertex] = list.Count;
					flag = true;
				}
			}
			if (flag)
			{
				list.Add(pair.a);
				Vector3 zero = Vector3.zero;
				foreach (TriangleData item in pair.b)
				{
					zero += item.normal;
				}
				list2.Add(zero);
			}
			num++;
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = array;
		mesh.normals = list2.ToArray();
	}

	private void setFacing(ref Vector3[][] triangles)
	{
		for (int i = 0; i < triangles.Length; i++)
		{
			if (Vector3.Dot(triangles[i][0], Vector3.Cross(triangles[i][1], triangles[i][2])) < 0f)
			{
				Vector3 vector = triangles[i][1];
				triangles[i][1] = triangles[i][2];
				triangles[i][2] = vector;
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
		Material obj = GetComponent<Renderer>().material;
		int octaves = (int)obj.GetFloat("_Octaves");
		int octaves2 = (int)obj.GetFloat("_BiomeOctaves");
		float @float = obj.GetFloat("_Frequency");
		float float2 = obj.GetFloat("_BiomeFrequency");
		float float3 = obj.GetFloat("_PlaneAmplitude");
		float float4 = obj.GetFloat("_MountainAmplitude");
		Vector3 vector = obj.GetVector("_Offset");
		float float5 = obj.GetFloat("_Lacunarity");
		float float6 = obj.GetFloat("_BiomePersistence");
		float float7 = obj.GetFloat("_PlanePersistence");
		float float8 = obj.GetFloat("_MountainPersistence");
		float float9 = obj.GetFloat("_Ocean");
		float float10 = obj.GetFloat("_MountainStart");
		float float11 = obj.GetFloat("_MountainEnd");
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

	private Pair<Vector3, List<TriangleData>>[] displace(Dictionary<Vector3, List<TriangleData>> map)
	{
		Material obj = GetComponent<Renderer>().material;
		int octaves = (int)obj.GetFloat("_Octaves");
		int octaves2 = (int)obj.GetFloat("_BiomeOctaves");
		float @float = obj.GetFloat("_Frequency");
		float float2 = obj.GetFloat("_BiomeFrequency");
		float float3 = obj.GetFloat("_PlaneAmplitude");
		float float4 = obj.GetFloat("_MountainAmplitude");
		Vector3 vector = obj.GetVector("_Offset");
		float float5 = obj.GetFloat("_Lacunarity");
		float float6 = obj.GetFloat("_BiomePersistence");
		float float7 = obj.GetFloat("_PlanePersistence");
		float float8 = obj.GetFloat("_MountainPersistence");
		float float9 = obj.GetFloat("_Ocean");
		float float10 = obj.GetFloat("_MountainStart");
		float float11 = obj.GetFloat("_MountainEnd");
		Pair<Vector3, List<TriangleData>>[] array = new Pair<Vector3, List<TriangleData>>[map.Count];
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
			array[num] = new Pair<Vector3, List<TriangleData>>(item.Key * (1f + num4 * 0.1f), item.Value);
			num++;
		}
		return array;
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
		setMaterial();
		float num = (1f + Mathf.Sqrt(5f)) / 2f;
		float num2 = 1f / Mathf.Sqrt(2f + num);
		num *= num2;
		num2 /= 2f;
		num /= 2f;
		Vector3[][] triangles = new Vector3[20][]
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
		setFacing(ref triangles);
		for (int i = 0; i < subdivideIterations; i++)
		{
			subdivide(ref triangles);
		}
		Dictionary<Vector3, List<TriangleData>> map = getMap(triangles);
		Pair<Vector3, List<TriangleData>>[] territory = displace(map);
		if (subdivideIterations <= 6)
		{
			Mesh mesh = base.gameObject.GetComponent<MeshFilter>().mesh;
			setMesh(base.gameObject.GetComponent<MeshFilter>().mesh, territory, triangles.Length, 0);
			base.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
			material = new Material[1] { GetComponent<Renderer>().material };
			return;
		}
		material = new Material[20];
		for (int j = 0; j < 20; j++)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			obj.GetComponent<Renderer>().material = GetComponent<Renderer>().material;
			Object.Destroy(obj.GetComponent<Collider>());
			obj.name = "Sector " + j;
			Mesh mesh2 = obj.GetComponent<MeshFilter>().mesh;
			setMesh(mesh2, territory, triangles.Length / 20, j);
			obj.AddComponent<MeshCollider>().sharedMesh = mesh2;
			material[j] = GetComponent<Renderer>().material;
		}
		GetComponent<MeshRenderer>().enabled = false;
	}

	private void Update()
	{
		if (biome == PlanetaryBiome.Lava)
		{
			Vector3 vector = material[0].GetVector("_OceanOffset");
			vector += base.transform.InverseTransformVector(Vector3.up) * (oceanSpeed * base.transform.lossyScale.y);
			Material[] array = material;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetVector("_OceanOffset", vector);
			}
		}
	}
}
