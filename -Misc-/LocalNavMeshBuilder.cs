using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-102)]
public class LocalNavMeshBuilder : MonoBehaviour
{
	private NavMeshData navMesh;

	private NavMeshDataInstance instance;

	private readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

	private Bounds bounds;

	private bool needsUpdate;

	private readonly List<NavMeshAgent> toEnable = new List<NavMeshAgent>();

	private readonly ISet<Component> toRemove = new HashSet<Component>();

	private readonly ISet<Collider> toRefresh = new HashSet<Collider>();

	private void Start()
	{
		bounds = new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
		NavMeshBuildSettings settingsByID = NavMesh.GetSettingsByID(0);
		navMesh = NavMeshBuilder.BuildNavMeshData(settingsByID, sources, bounds, base.transform.position, base.transform.rotation);
		instance = NavMesh.AddNavMeshData(navMesh);
		needsUpdate = false;
	}

	private void OnEnable()
	{
		StartCoroutine(UpdateNavMesh());
	}

	private void OnDestroy()
	{
		instance.Remove();
	}

	private IEnumerator UpdateNavMesh()
	{
		while (true)
		{
			if (needsUpdate)
			{
				needsUpdate = false;
				NavMeshBuildSettings settingsByID = NavMesh.GetSettingsByID(0);
				yield return NavMeshBuilder.UpdateNavMeshDataAsync(navMesh, settingsByID, sources, bounds);
				continue;
			}
			foreach (NavMeshAgent item in toEnable)
			{
				if (item != null)
				{
					item.enabled = true;
				}
			}
			toEnable.Clear();
			yield return null;
		}
	}

	public void EnableOnMeshBuilt(NavMeshAgent agent)
	{
		toEnable.Add(agent);
	}

	public void ForceUpdate()
	{
		if (!needsUpdate)
		{
			needsUpdate = true;
		}
	}

	public void AddSource(Collider coll)
	{
		if (toRemove.Remove(coll))
		{
			RefreshSource(coll);
			return;
		}
		sources.Add(MakeSource(coll));
		ForceUpdate();
	}

	private static NavMeshBuildSource MakeSource(Collider coll)
	{
		NavMeshBuildSource navMeshBuildSource = default(NavMeshBuildSource);
		navMeshBuildSource.component = coll;
		navMeshBuildSource.transform = coll.transform.localToWorldMatrix;
		navMeshBuildSource.area = 0;
		NavMeshBuildSource result = navMeshBuildSource;
		if (!(coll is MeshCollider meshCollider))
		{
			if (!(coll is BoxCollider boxCollider))
			{
				if (coll is TerrainCollider terrainCollider)
				{
					result.shape = NavMeshBuildSourceShape.Terrain;
					result.sourceObject = terrainCollider.terrainData;
				}
			}
			else
			{
				result.shape = NavMeshBuildSourceShape.Box;
				result.transform *= Matrix4x4.Translate(boxCollider.center);
				result.size = boxCollider.size;
			}
		}
		else
		{
			result.shape = NavMeshBuildSourceShape.Mesh;
			result.sourceObject = meshCollider.sharedMesh;
		}
		return result;
	}

	public void RemoveSource(Component comp)
	{
		if (toRemove.Count == 0)
		{
			StartCoroutine(RemoveSources());
		}
		toRemove.Add(comp);
	}

	public void RemoveSourceImmediate(Component comp)
	{
		toRemove.Remove(comp);
		for (int i = 0; i < sources.Count; i++)
		{
			if (!(sources[i].component != comp))
			{
				sources.RemoveAt(i);
				ForceUpdate();
				break;
			}
		}
	}

	private IEnumerator RemoveSources()
	{
		yield return null;
		if (toRemove.Count != 0)
		{
			if (sources.RemoveAll((NavMeshBuildSource source) => toRemove.Contains(source.component)) > 0)
			{
				ForceUpdate();
			}
			toRemove.Clear();
		}
	}

	private void RefreshSource(Collider coll)
	{
		if (toRefresh.Count == 0)
		{
			StartCoroutine(RefreshSources());
		}
		toRefresh.Add(coll);
	}

	private IEnumerator RefreshSources()
	{
		yield return null;
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i].component is Collider collider && toRefresh.Contains(collider))
			{
				sources[i] = MakeSource(collider);
				ForceUpdate();
			}
		}
		toRefresh.Clear();
	}
}
