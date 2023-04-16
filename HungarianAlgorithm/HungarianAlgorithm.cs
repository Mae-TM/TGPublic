using System;

namespace HungarianAlgorithm;

public static class HungarianAlgorithm
{
	private struct Location
	{
		internal readonly int row;

		internal readonly int column;

		internal Location(int row, int col)
		{
			this.row = row;
			column = col;
		}
	}

	public static int[] FindAssignments(this int[,] costs)
	{
		if (costs == null)
		{
			throw new ArgumentNullException("costs");
		}
		int length = costs.GetLength(0);
		int length2 = costs.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			int num = int.MaxValue;
			for (int j = 0; j < length2; j++)
			{
				num = Math.Min(num, costs[i, j]);
			}
			for (int k = 0; k < length2; k++)
			{
				costs[i, k] -= num;
			}
		}
		byte[,] array = new byte[length, length2];
		bool[] array2 = new bool[length];
		bool[] array3 = new bool[length2];
		for (int l = 0; l < length; l++)
		{
			for (int m = 0; m < length2; m++)
			{
				if (costs[l, m] == 0 && !array2[l] && !array3[m])
				{
					array[l, m] = 1;
					array2[l] = true;
					array3[m] = true;
				}
			}
		}
		ClearCovers(array2, array3, length2, length);
		Location[] path = new Location[length2 * length];
		Location pathStart = default(Location);
		int num2 = 1;
		while (true)
		{
			switch (num2)
			{
			case 1:
				num2 = RunStep1(array, array3, length2, length);
				break;
			case 2:
				num2 = RunStep2(costs, array, array2, array3, length2, length, ref pathStart);
				break;
			case 3:
				num2 = RunStep3(array, array2, array3, length2, length, path, pathStart);
				break;
			case 4:
				num2 = RunStep4(costs, array2, array3, length2, length);
				break;
			case -1:
			{
				int[] array4 = new int[length];
				for (int n = 0; n < length; n++)
				{
					for (int num3 = 0; num3 < length2; num3++)
					{
						if (array[n, num3] == 1)
						{
							array4[n] = num3;
							break;
						}
					}
				}
				return array4;
			}
			}
		}
	}

	private static int RunStep1(byte[,] masks, bool[] colsCovered, int w, int h)
	{
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		if (colsCovered == null)
		{
			throw new ArgumentNullException("colsCovered");
		}
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (masks[i, j] == 1)
				{
					colsCovered[j] = true;
				}
			}
		}
		int num = 0;
		for (int k = 0; k < w; k++)
		{
			if (colsCovered[k])
			{
				num++;
			}
		}
		if (num == h)
		{
			return -1;
		}
		return 2;
	}

	private static int RunStep2(int[,] costs, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, ref Location pathStart)
	{
		if (costs == null)
		{
			throw new ArgumentNullException("costs");
		}
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		if (rowsCovered == null)
		{
			throw new ArgumentNullException("rowsCovered");
		}
		if (colsCovered == null)
		{
			throw new ArgumentNullException("colsCovered");
		}
		Location location;
		while (true)
		{
			location = FindZero(costs, rowsCovered, colsCovered, w, h);
			if (location.row == -1)
			{
				return 4;
			}
			masks[location.row, location.column] = 2;
			int num = FindStarInRow(masks, w, location.row);
			if (num == -1)
			{
				break;
			}
			rowsCovered[location.row] = true;
			colsCovered[num] = false;
		}
		pathStart = location;
		return 3;
	}

	private static int RunStep3(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, Location[] path, Location pathStart)
	{
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		if (rowsCovered == null)
		{
			throw new ArgumentNullException("rowsCovered");
		}
		if (colsCovered == null)
		{
			throw new ArgumentNullException("colsCovered");
		}
		int num = 0;
		path[0] = pathStart;
		while (true)
		{
			int num2 = FindStarInColumn(masks, h, path[num].column);
			if (num2 == -1)
			{
				break;
			}
			num++;
			path[num] = new Location(num2, path[num - 1].column);
			int col = FindPrimeInRow(masks, w, path[num].row);
			num++;
			path[num] = new Location(path[num - 1].row, col);
		}
		ConvertPath(masks, path, num + 1);
		ClearCovers(rowsCovered, colsCovered, w, h);
		ClearPrimes(masks, w, h);
		return 1;
	}

	private static int RunStep4(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		if (costs == null)
		{
			throw new ArgumentNullException("costs");
		}
		if (rowsCovered == null)
		{
			throw new ArgumentNullException("rowsCovered");
		}
		if (colsCovered == null)
		{
			throw new ArgumentNullException("colsCovered");
		}
		int num = FindMinimum(costs, rowsCovered, colsCovered, w, h);
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (rowsCovered[i])
				{
					costs[i, j] += num;
				}
				if (!colsCovered[j])
				{
					costs[i, j] -= num;
				}
			}
		}
		return 2;
	}

	private static int FindMinimum(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		if (costs == null)
		{
			throw new ArgumentNullException("costs");
		}
		if (rowsCovered == null)
		{
			throw new ArgumentNullException("rowsCovered");
		}
		if (colsCovered == null)
		{
			throw new ArgumentNullException("colsCovered");
		}
		int num = int.MaxValue;
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (!rowsCovered[i] && !colsCovered[j])
				{
					num = Math.Min(num, costs[i, j]);
				}
			}
		}
		return num;
	}

	private static int FindStarInRow(byte[,] masks, int w, int row)
	{
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		for (int i = 0; i < w; i++)
		{
			if (masks[row, i] == 1)
			{
				return i;
			}
		}
		return -1;
	}

	private static int FindStarInColumn(byte[,] masks, int h, int col)
	{
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		for (int i = 0; i < h; i++)
		{
			if (masks[i, col] == 1)
			{
				return i;
			}
		}
		return -1;
	}

	private static int FindPrimeInRow(byte[,] masks, int w, int row)
	{
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		for (int i = 0; i < w; i++)
		{
			if (masks[row, i] == 2)
			{
				return i;
			}
		}
		return -1;
	}

	private static Location FindZero(int[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		if (costs == null)
		{
			throw new ArgumentNullException("costs");
		}
		if (rowsCovered == null)
		{
			throw new ArgumentNullException("rowsCovered");
		}
		if (colsCovered == null)
		{
			throw new ArgumentNullException("colsCovered");
		}
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
				{
					return new Location(i, j);
				}
			}
		}
		return new Location(-1, -1);
	}

	private static void ConvertPath(byte[,] masks, Location[] path, int pathLength)
	{
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		for (int i = 0; i < pathLength; i++)
		{
			if (masks[path[i].row, path[i].column] == 1)
			{
				masks[path[i].row, path[i].column] = 0;
			}
			else if (masks[path[i].row, path[i].column] == 2)
			{
				masks[path[i].row, path[i].column] = 1;
			}
		}
	}

	private static void ClearPrimes(byte[,] masks, int w, int h)
	{
		if (masks == null)
		{
			throw new ArgumentNullException("masks");
		}
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (masks[i, j] == 2)
				{
					masks[i, j] = 0;
				}
			}
		}
	}

	private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		if (rowsCovered == null)
		{
			throw new ArgumentNullException("rowsCovered");
		}
		if (colsCovered == null)
		{
			throw new ArgumentNullException("colsCovered");
		}
		for (int i = 0; i < h; i++)
		{
			rowsCovered[i] = false;
		}
		for (int j = 0; j < w; j++)
		{
			colsCovered[j] = false;
		}
	}
}
