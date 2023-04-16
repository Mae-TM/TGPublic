using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshCollider))]
public class OrientedNavMesh : MonoBehaviour
{
	public Vector3 center;

	private NavMeshDataInstance instance;

	private NavMeshData navMeshData;

	private void Start()
	{
		StartCoroutine(MakeNavMeshData());
	}

	private IEnumerator MakeNavMeshData()
	{
		MeshCollider component = GetComponent<MeshCollider>();
		navMeshData = new NavMeshData
		{
			position = base.transform.position,
			rotation = Quaternion.FromToRotation(Vector3.up, center)
		};
		yield return NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, NavMesh.GetSettingsByID(0), new List<NavMeshBuildSource>
		{
			new NavMeshBuildSource
			{
				shape = NavMeshBuildSourceShape.Mesh,
				component = component,
				sourceObject = component.sharedMesh,
				transform = component.transform.localToWorldMatrix,
				area = 0
			}
		}, new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)));
		instance = NavMesh.AddNavMeshData(navMeshData);
		instance.owner = base.transform;
		yield return null;
		StaticBatchingUtility.Combine(base.gameObject);
	}

	private void OnDestroy()
	{
		instance.Remove();
	}
}
