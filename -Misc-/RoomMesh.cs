using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomMesh : MonoBehaviour
{
	public const float tileSize = 1.5f;

	public const int wallHeight = 3;

	public const float thickness = 0.1f;

	private NavMeshSourceTag navMeshSourceTag;

	private MeshCollider collider;

	private Mesh mesh;

	private Walls wallsComponent;

	private readonly AAPoly walls = new AAPoly();

	private readonly AAPoly floor = new AAPoly();

	public OutsideWalls outside;

	private void Awake()
	{
		mesh = new Mesh();
		base.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
		collider = base.gameObject.AddComponent<MeshCollider>();
		navMeshSourceTag = base.gameObject.AddComponent<NavMeshSourceTag>();
		wallsComponent = base.gameObject.AddComponent<Walls>();
	}

	public void SetMaterial(Material material)
	{
		base.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
		wallsComponent.material = material;
	}

	public AAPoly GetWalls()
	{
		if (!outside)
		{
			return floor;
		}
		return walls;
	}

	public void AddFloor(Vector2Int cell)
	{
		floor.Add(cell);
	}

	public void RemoveFloor(Vector2Int cell)
	{
		floor.Remove(cell);
	}

	public void FinalizeFloor(int height)
	{
		floor.CancelOpposites();
		Generate(height);
	}

	public bool Add(AAPoly poly, int height)
	{
		if ((bool)outside)
		{
			walls.Add(poly);
			outside.Add(poly, height);
			if (walls.Count == 0)
			{
				return true;
			}
		}
		floor.Add(poly);
		Generate(height);
		return false;
	}

	private void Generate(int height)
	{
		CreateMesh(height);
		collider.sharedMesh = mesh;
		navMeshSourceTag.Refresh();
	}

	public static void AddTriangles1(ICollection<int> triangles, int count)
	{
		triangles.Add(count - 4);
		triangles.Add(count - 2);
		triangles.Add(count - 1);
		triangles.Add(count - 1);
		triangles.Add(count - 3);
		triangles.Add(count - 4);
	}

	public static void AddTriangles2(ICollection<int> triangles, int count)
	{
		triangles.Add(count - 1);
		triangles.Add(count - 2);
		triangles.Add(count - 4);
		triangles.Add(count - 4);
		triangles.Add(count - 3);
		triangles.Add(count - 1);
	}

	private void CreateMesh(int height)
	{
		mesh.Clear();
		List<Vector3> list = new List<Vector3>();
		List<int> triangles = new List<int>();
		float y = height * 3;
		foreach (RectInt rectangle in floor.GetRectangles())
		{
			list.Add(1.5f * new Vector3(rectangle.xMin, y + 0.1f, rectangle.yMin));
			list.Add(1.5f * new Vector3(rectangle.xMax, y + 0.1f, rectangle.yMin));
			list.Add(1.5f * new Vector3(rectangle.xMin, y + 0.1f, rectangle.yMax));
			list.Add(1.5f * new Vector3(rectangle.xMax, y + 0.1f, rectangle.yMax));
			AddTriangles1(triangles, list.Count);
			list.Add(1.5f * new Vector3(rectangle.xMin, y, rectangle.yMin));
			list.Add(1.5f * new Vector3(rectangle.xMax, y, rectangle.yMin));
			list.Add(1.5f * new Vector3(rectangle.xMin, y, rectangle.yMax));
			list.Add(1.5f * new Vector3(rectangle.xMax, y, rectangle.yMax));
			AddTriangles2(triangles, list.Count);
		}
		foreach (var (tuple2, tuple3) in floor.GetSides())
		{
			var (num, num2) = tuple2;
			var (num3, num4) = tuple3;
			list.Add(1.5f * new Vector3(num, y, num2));
			list.Add(1.5f * new Vector3(num, y + 0.1f, num2));
			list.Add(1.5f * new Vector3(num3, y, num4));
			list.Add(1.5f * new Vector3(num3, y + 0.1f, num4));
			AddTriangles1(triangles, list.Count);
		}
		wallsComponent.SetWalls(walls.GetSides().Select<((int, int), (int, int)), (float, float, float, float, float, float)>((Func<((int, int), (int, int)), (float, float, float, float, float, float)>)delegate(((int, int) from, (int, int) to) pair)
		{
			var (tuple7, tuple8) = pair;
			var (num5, num6) = tuple7;
			var (num7, num8) = tuple8;
			return (num5, (float)num7 - (float)num5, y + 0.1f, 2.9f, num6, (float)num8 - (float)num6);
		}));
		mesh.SetVertices(list);
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateNormals();
		mesh.Optimize();
	}
}
