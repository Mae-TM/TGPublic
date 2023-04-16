using System.Collections.Generic;
using UnityEngine;

public class RectUtility
{
	public static IEnumerable<Vector2Int> GetWallPoints(RectInt rect)
	{
		if (rect.width == 0)
		{
			int y2 = 0;
			while (y2 < rect.height)
			{
				yield return new Vector2Int(rect.x, rect.y + y2);
				int num = y2 + 1;
				y2 = num;
			}
		}
		else if (rect.height == 0)
		{
			int y2 = 0;
			while (y2 < rect.width)
			{
				yield return new Vector2Int(rect.x + y2, rect.y);
				int num = y2 + 1;
				y2 = num;
			}
		}
		else
		{
			Debug.LogError($"Rectangle size should be 0 in one direction! Size is {rect.size}.");
		}
	}
}
