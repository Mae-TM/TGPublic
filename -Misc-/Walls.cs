using System.Collections.Generic;
using UnityEngine;

public class Walls : MonoBehaviour
{
	public bool isOutside;

	public Material material;

	private readonly List<Wall> walls = new List<Wall>();

	private void Start()
	{
		if (!Wall.cam)
		{
			Wall.SetCam(MSPAOrthoController.main, newTransparent: false);
		}
	}

	public void SetWalls(IEnumerable<(float, float, float, float, float, float)> tuples)
	{
		foreach (Wall wall in walls)
		{
			Object.Destroy(wall.gameObject);
		}
		walls.Clear();
		foreach (var tuple in tuples)
		{
			float item = tuple.Item1;
			float item2 = tuple.Item2;
			float item3 = tuple.Item3;
			float item4 = tuple.Item4;
			float item5 = tuple.Item5;
			float item6 = tuple.Item6;
			Mesh mesh = new Mesh();
			mesh.vertices = new Vector3[4]
			{
				1.5f * new Vector3(0f, 0f, 0f),
				1.5f * new Vector3(0f, item4, 0f),
				1.5f * new Vector3(item2, 0f, item6),
				1.5f * new Vector3(item2, item4, item6)
			};
			mesh.triangles = new int[6] { 3, 2, 0, 0, 1, 3 };
			Mesh mesh2 = mesh;
			mesh2.RecalculateNormals();
			GameObject obj = new GameObject("Wall");
			obj.transform.parent = base.transform;
			obj.transform.localPosition = 1.5f * new Vector3(item, item3, item5);
			GameObject gameObject = obj;
			gameObject.AddComponent<MeshFilter>().mesh = mesh2;
			gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
			gameObject.AddComponent<MeshCollider>().sharedMesh = mesh2;
			gameObject.AddComponent<NavMeshSourceTag>();
			walls.Add(gameObject.AddComponent<Wall>());
			Visibility.Copy(gameObject, base.gameObject);
		}
	}

	private void Update()
	{
		if (!Visibility.Get(base.gameObject))
		{
			return;
		}
		foreach (Wall wall in walls)
		{
			if (isOutside && !Wall.transparent)
			{
				wall.SetVisible(material);
			}
			else
			{
				wall.UpdateVisibility(material);
			}
		}
	}
}
