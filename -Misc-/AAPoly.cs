using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using ProtoBuf;
using UnityEngine;

[ProtoContract(Surrogate = typeof(ShortChains))]
public class AAPoly : IEnumerable<(int from, int to, int pos)>, IEnumerable
{
	[ProtoContract]
	public struct ShortChains : NetworkMessage
	{
		[ProtoMember(1, DataFormat = DataFormat.ZigZag)]
		public int[] corners;

		[ProtoMember(2)]
		public int[] chainLengths;

		public static implicit operator ShortChains(AAPoly poly)
		{
			ShortChains result = default(ShortChains);
			ref int[] reference = ref result.corners;
			ref int[] reference2 = ref result.chainLengths;
			(reference, reference2) = ProtobufHelpers.Unjag((from chain in poly?.GetChains()
				select chain.Select(((int, int) pos, int index) => (((uint)index & (true ? 1u : 0u)) != 0) ? pos.Item2 : pos.Item1).ToArray()));
			return result;
		}

		public static implicit operator AAPoly(ShortChains chains)
		{
			AAPoly aAPoly = new AAPoly();
			foreach (int[] item in ProtobufHelpers.Rejag(chains.corners, chains.chainLengths))
			{
				int to = 0;
				int num = 0;
				int? num2 = null;
				bool flag = false;
				int[] array = item;
				foreach (int num3 in array)
				{
					if (!flag)
					{
						if (num2.HasValue)
						{
							aAPoly.Add(num2.Value, num3, num);
						}
						else
						{
							to = num3;
						}
					}
					num2 = num;
					num = num3;
					flag = !flag;
				}
				if (num2.HasValue)
				{
					aAPoly.Add(num2.Value, to, num);
				}
			}
			return aAPoly;
		}
	}

	private readonly List<(int from, int to, int pos)> sides = new List<(int, int, int)>();

	public int Count => sides.Count;

	public bool Sign => sides[0].from > sides[0].to;

	public void Add(IEnumerable<(int from, int to, int pos)> toAdd)
	{
		foreach (var (from, to, pos) in toAdd)
		{
			Add(from, to, pos);
		}
		CancelOpposites();
	}

	public void Remove(IEnumerable<(int from, int to, int pos)> toRemove)
	{
		foreach (var (to, from, pos) in toRemove)
		{
			Add(from, to, pos);
		}
		CancelOpposites();
	}

	public void Add(Vector2Int cell)
	{
		Add(new RectInt(cell, Vector2Int.one));
	}

	public void Remove(Vector2Int cell)
	{
		Remove(new RectInt(cell, Vector2Int.one));
	}

	public void Add(RectInt rect)
	{
		Add(rect.xMax, rect.xMin, rect.yMin);
		Add(rect.xMin, rect.xMax, rect.yMax);
	}

	public void Remove(RectInt rect)
	{
		Add(rect.xMin, rect.xMax, rect.yMin);
		Add(rect.xMax, rect.xMin, rect.yMax);
	}

	public void Add(int from, int to, int pos)
	{
		for (int i = 0; i < sides.Count; i++)
		{
			var (num, num2, num3) = sides[i];
			if (num3 < pos)
			{
				continue;
			}
			if (num3 > pos)
			{
				sides.Insert(i, (from, to, pos));
				return;
			}
			if (from == num2)
			{
				if (num == to)
				{
					sides.RemoveAt(i);
				}
				else
				{
					sides[i] = (num, to, pos);
				}
				return;
			}
			if (num == to)
			{
				sides[i] = (from, num2, pos);
				return;
			}
		}
		sides.Add((from, to, pos));
	}

	public void CancelOpposites()
	{
		for (int i = 0; i < sides.Count; i++)
		{
			(int from, int to, int pos) tuple = sides[i];
			int item = tuple.from;
			int num = tuple.to;
			int item2 = tuple.pos;
			for (int j = i + 1; j < sides.Count; j++)
			{
				(int from, int to, int pos) tuple2 = sides[j];
				var (num2, num3, _) = tuple2;
				if (tuple2.pos != item2)
				{
					break;
				}
				bool num4;
				if (num >= item)
				{
					if (num3 >= num2 || num3 >= num)
					{
						continue;
					}
					num4 = item >= num2;
				}
				else
				{
					if (num2 >= num3 || num >= num3)
					{
						continue;
					}
					num4 = num2 >= item;
				}
				if (!num4)
				{
					sides[j] = (num2, num, item2);
					num = num3;
				}
			}
			sides[i] = (item, num, item2);
		}
		sides.RemoveAll(((int from, int to, int pos) side) => side.from == side.to);
	}

	public void Translate(Vector2Int offset)
	{
		for (int i = 0; i < sides.Count; i++)
		{
			var (num, num2, num3) = sides[i];
			sides[i] = (num + offset.x, num2 + offset.x, num3 + offset.y);
		}
	}

	public IEnumerable<RectInt> GetRectangles()
	{
		List<(int, int)> sep = new List<(int, int)>();
		int r = 0;
		while (r < sides.Count)
		{
			var (num, num2, rPos) = sides[r];
			int num3;
			if (num < num2)
			{
				sep.Add((num, num2));
				int j = r - 1;
				while (true)
				{
					var (lFrom, lTo, lPos) = sides[j];
					if (lFrom > lTo)
					{
						for (int i = sep.Count - 1; i >= 0; i = num3)
						{
							var (sepFrom, sepTo) = sep[i];
							if (lFrom > sepFrom)
							{
								if (lTo >= sepTo)
								{
									break;
								}
								int start = Math.Max(sepFrom, lTo);
								int end = Math.Min(sepTo, lFrom);
								yield return new RectInt(start, lPos, end - start, rPos - lPos);
								if (start != sepFrom)
								{
									sep[i] = (sepFrom, start);
									if (end != sepTo)
									{
										sep.Insert(i + 1, (end, sepTo));
									}
									break;
								}
								if (end != sepTo)
								{
									sep[i] = (end, sepTo);
								}
								else
								{
									sep.RemoveAt(i);
								}
							}
							num3 = i - 1;
						}
						if (sep.Count == 0)
						{
							break;
						}
					}
					num3 = j - 1;
					j = num3;
				}
			}
			num3 = r + 1;
			r = num3;
		}
	}

	public IEnumerable<Vector2Int> GetCells()
	{
		foreach (RectInt rectangle in GetRectangles())
		{
			foreach (Vector2Int item in rectangle.allPositionsWithin)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<IEnumerable<(int, int)>> GetChains()
	{
		bool[] array = new bool[sides.Count];
		int[] next = new int[sides.Count];
		for (int j = 0; j < next.Length; j++)
		{
			next[j] = -1;
		}
		for (int num = sides.Count - 1; num >= 0; num--)
		{
			(int from, int to, int pos) tuple = sides[num];
			int item = tuple.from;
			int item2 = tuple.to;
			bool flag = array[num];
			bool flag2 = next[num] != -1;
			if (!(flag && flag2))
			{
				int num2 = num - 1;
				while (true)
				{
					if (!flag && sides[num2].to == item)
					{
						next[num2] = num;
						if (flag2)
						{
							break;
						}
						flag = true;
					}
					if (!flag2 && item2 == sides[num2].from)
					{
						next[num] = num2;
						array[num2] = true;
						if (flag)
						{
							break;
						}
						flag2 = true;
					}
					num2--;
				}
			}
		}
		int i = 0;
		while (i < next.Length)
		{
			if (next[i] != -1)
			{
				yield return GetChain(i, next);
			}
			int num3 = i + 1;
			i = num3;
		}
	}

	private IEnumerable<(int, int)> GetChain(int start, int[] next)
	{
		int i = start;
		do
		{
			yield return (sides[i].from, sides[i].pos);
			yield return (sides[i].to, sides[i].pos);
			int num = next[i];
			next[i] = -1;
			i = num;
		}
		while (i != start);
	}

	public IEnumerable<((int, int) from, (int, int) to)> GetSides()
	{
		foreach (IEnumerable<(int, int)> chain in GetChains())
		{
			(int, int) first = default((int, int));
			(int, int)? tuple = null;
			foreach (var pair in chain)
			{
				if (tuple.HasValue)
				{
					yield return (tuple.Value, pair);
				}
				else
				{
					first = pair;
				}
				tuple = pair;
			}
			if (tuple.HasValue)
			{
				yield return (tuple.Value, first);
			}
		}
	}

	public bool IsValid()
	{
		try
		{
			using IEnumerator<IEnumerable<(int, int)>> enumerator = GetChains().GetEnumerator();
			enumerator.MoveNext();
			return true;
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<(int, int, int)> GetEnumerator()
	{
		return sides.GetEnumerator();
	}

	public void Clear()
	{
		sides.Clear();
	}

	public void Invert()
	{
		for (int i = 0; i < sides.Count; i++)
		{
			var (item, item2, item3) = sides[i];
			sides[i] = (item2, item, item3);
		}
	}
}
