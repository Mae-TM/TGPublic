using UnityEngine;

public class PlanetTriangle : MonoBehaviour
{
	public bool activate;

	private GameObject child;

	private int start;

	private int end;

	private Material mat;

	private DisplacementMap2 displacementMap;

	public static PlanetTriangle create(int start, int end, Material mat, DisplacementMap2 displacementMap)
	{
		PlanetTriangle planetTriangle = new GameObject().AddComponent<PlanetTriangle>();
		planetTriangle.start = start;
		planetTriangle.end = end;
		planetTriangle.displacementMap = displacementMap;
		planetTriangle.mat = mat;
		return planetTriangle;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (activate && child == null)
		{
			child = GameObject.CreatePrimitive(PrimitiveType.Cube);
			child.transform.SetParent(base.transform, worldPositionStays: false);
			Mesh mesh = displacementMap.setMesh(start, end);
			child.GetComponent<MeshFilter>().mesh = mesh;
			Object.Destroy(child.GetComponent<Collider>());
			child.GetComponent<Renderer>().material = mat;
		}
	}
}
