using System;
using System.Linq;

namespace HungarianAlgorithm.Extensions;

public static class ArrayExtensions
{
	public static T[,] SquareArray<T>(this T[][] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = array.Length;
		int num2 = array.Select((T[] x) => x.Length).Concat(new int[1]).Max();
		int num3 = ((num > num2) ? num : num2);
		T[,] array2 = new T[num3, num3];
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				try
				{
					array2[i, j] = array[i][j];
				}
				catch (Exception)
				{
					array2[i, j] = default(T);
				}
			}
		}
		return array2;
	}
}
