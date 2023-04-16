using System;
using System.Linq;
using UnityEngine;

public class OutsideWalls : MonoBehaviour
{
	private Walls walls;

	private readonly AAPoly sides = new AAPoly();

	private bool hasChanged;

	private void Awake()
	{
		walls = base.gameObject.AddComponent<Walls>();
		walls.isOutside = true;
	}

	public void SetMaterial(Material material)
	{
		walls.material = material;
	}

	public void Add(AAPoly toAdd, int height)
	{
		sides.Add(toAdd);
		hasChanged = true;
	}

	public void Generate(int height)
	{
		if (hasChanged)
		{
			hasChanged = false;
			float y = height * 3;
			walls.SetWalls(sides.GetSides().Select<((int, int), (int, int)), (float, float, float, float, float, float)>((Func<((int, int), (int, int)), (float, float, float, float, float, float)>)delegate(((int, int) from, (int, int) to) pair)
			{
				var (tuple2, tuple3) = pair;
				var (num, num2) = tuple2;
				var (num3, num4) = tuple3;
				return (num3, (float)num - (float)num3, y, 3f, num4, (float)num2 - (float)num4);
			}));
		}
	}
}
