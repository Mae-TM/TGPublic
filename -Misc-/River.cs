using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal readonly struct River
{
	private readonly struct Vertex : IEquatable<Vertex>
	{
		public readonly int index;

		public readonly bool isOuter;

		public readonly List<Vertex> adjacent;

		public Vertex(int index, bool isOuter)
		{
			this.index = index;
			this.isOuter = isOuter;
			adjacent = new List<Vertex>(7);
		}

		public bool Equals(Vertex other)
		{
			return index == other.index;
		}
	}

	private readonly ISet<(int, int)> origIndices;

	private River(ISet<(int, int)> indices)
	{
		origIndices = indices;
	}

	public static void Add(List<River> rivers, IEnumerable<(int, int)> newRiver)
	{
		HashSet<(int, int)> river = new HashSet<(int, int)>(newRiver);
		rivers.RemoveAll(delegate(River other)
		{
			if (!other.origIndices.Overlaps(river))
			{
				return false;
			}
			river.UnionWith(other.origIndices);
			return true;
		});
		river.TrimExcess();
		rivers.Add(new River(river));
	}

	public IEnumerable<Vector2> GetTriangles(out ICollection<int> triangles)
	{
		ISet<(int, int)> adjacent = GetAdjacent(origIndices, 1, 0);
		triangles = new List<int>();
		List<(int, int)> vertices = new List<(int, int)>();
		Dictionary<(int, int), Vertex> dictionary = new Dictionary<(int, int), Vertex>();
		foreach (var item3 in adjacent)
		{
			int item = item3.Item1;
			int item2 = item3.Item2;
			Vertex index = GetIndex(dictionary, vertices, (item, item2));
			Vertex index2 = GetIndex(dictionary, vertices, (item + 1, item2 + 1));
			Vertex index3 = GetIndex(dictionary, vertices, (item + 1, item2));
			AddTriangle(triangles, index, index2, index3);
			Vertex index4 = GetIndex(dictionary, vertices, (item, item2 + 1));
			AddTriangle(triangles, index, index4, index2);
			if (adjacent.Contains((item - 1, item2 + 1)))
			{
				if (!adjacent.Contains((item - 1, item2)))
				{
					Vertex index5 = GetIndex(dictionary, vertices, (item - 1, item2 + 1));
					AddTriangle(triangles, index4, index, index5);
				}
				if (!adjacent.Contains((item, item2 + 1)))
				{
					Vertex index6 = GetIndex(dictionary, vertices, (item, item2 + 2));
					AddTriangle(triangles, index4, index6, index2);
				}
			}
			if (adjacent.Contains((item + 1, item2 + 1)))
			{
				if (!adjacent.Contains((item + 1, item2)))
				{
					Vertex index7 = GetIndex(dictionary, vertices, (item + 2, item2 + 1));
					AddTriangle(triangles, index2, index7, index3);
				}
				if (!adjacent.Contains((item, item2 + 1)))
				{
					Vertex index8 = GetIndex(dictionary, vertices, (item + 1, item2 + 2));
					AddTriangle(triangles, index2, index4, index8);
				}
			}
		}
		foreach (KeyValuePair<(int, int), Vertex> item4 in dictionary)
		{
			item4.Value.adjacent.TrimExcess();
		}
		return PostProcess(vertices, dictionary);
	}

	private static void AddTriangle(ICollection<int> triangles, Vertex one, Vertex two, Vertex three)
	{
		AddTrianglePart(triangles, one, two, three);
		AddTrianglePart(triangles, two, three, one);
		AddTrianglePart(triangles, three, one, two);
	}

	private static void AddTrianglePart(ICollection<int> triangles, Vertex vertex, Vertex a, Vertex b)
	{
		triangles.Add(vertex.index);
		if (!vertex.adjacent.Contains(a))
		{
			vertex.adjacent.Add(a);
		}
		if (!vertex.adjacent.Contains(b))
		{
			vertex.adjacent.Add(b);
		}
	}

	private static IEnumerable<(int, int)> GetNeighbours((int, int) pos, int min = 1, int max = 1)
	{
		(int, int) tuple = pos;
		int x = tuple.Item1;
		int y = tuple.Item2;
		int xx = x - min;
		while (xx <= x + max)
		{
			int num;
			for (int yy = y - min; yy <= y + max; yy = num)
			{
				yield return (xx, yy);
				num = yy + 1;
			}
			num = xx + 1;
			xx = num;
		}
	}

	private static ISet<(int, int)> GetAdjacent(IEnumerable<(int, int)> riverIndices, int min, int max)
	{
		HashSet<(int, int)> hashSet = new HashSet<(int, int)>(riverIndices.SelectMany(((int, int) pos) => GetNeighbours(pos, min, max)));
		hashSet.TrimExcess();
		return hashSet;
	}

	private Vertex GetIndex(IDictionary<(int, int), Vertex> vertexIndices, ICollection<(int, int)> vertices, (int, int) pos)
	{
		if (vertexIndices.TryGetValue(pos, out var value))
		{
			return value;
		}
		value = new Vertex(vertices.Count, !origIndices.Contains(pos));
		vertexIndices.Add(pos, value);
		vertices.Add(pos);
		return value;
	}

	private static IEnumerable<Vector2> PostProcess(IEnumerable<(int, int)> vertices, Dictionary<(int, int), Vertex> vertexIndices)
	{
		Vector2[] array = vertices.Select(((int, int) v) => new Vector2(v.Item1, v.Item2)).ToArray();
		Smoothing(array, GetAdjacency(vertexIndices, isOuter: true, 2, 1));
		Smoothing(array, GetAdjacency(vertexIndices, isOuter: false, 0, 1));
		return array;
	}

	private static IEnumerable<(int, IEnumerable<int>)> GetAdjacency(Dictionary<(int, int), Vertex> vertexIndices, bool isOuter, int innerWeight, int outerWeight)
	{
		return from vertex in vertexIndices.Values
			where vertex.isOuter == isOuter
			select (vertex.index, GetAdjacency(vertex.adjacent, innerWeight, outerWeight));
	}

	private static IEnumerable<int> GetAdjacency(IEnumerable<Vertex> list, int innerWeight, int outerWeight)
	{
		foreach (Vertex vertex in list)
		{
			int weight = (vertex.isOuter ? outerWeight : innerWeight);
			int i = 0;
			while (i < weight)
			{
				yield return vertex.index;
				int num = i + 1;
				i = num;
			}
		}
	}

	private static void Smoothing(IList<Vector2> currVertices, IEnumerable<(int, IEnumerable<int>)> adjacencyList)
	{
		foreach (var adjacency in adjacencyList)
		{
			int item = adjacency.Item1;
			IEnumerable<int> item2 = adjacency.Item2;
			int num = 0;
			Vector2 zero = Vector2.zero;
			foreach (int item3 in item2)
			{
				zero += currVertices[item3];
				num++;
			}
			if (num != 0)
			{
				currVertices[item] = zero / num;
			}
		}
	}
}
