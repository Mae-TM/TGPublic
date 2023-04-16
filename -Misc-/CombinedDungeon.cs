using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombinedDungeon
{
	private readonly List<HouseData.Attackable> attackables = new List<HouseData.Attackable>();

	private readonly List<HouseData.Furniture> furniture = new List<HouseData.Furniture>();

	private readonly List<HouseData.DroppedItem> items = new List<HouseData.DroppedItem>();

	private readonly List<AAPoly> rooms = new List<AAPoly>();

	private AAPoly incompleteRoom;

	private Vector3Int? spawnPosition;

	private int totalLength;

	public void Add(HouseData data)
	{
		Vector3Int valueOrDefault = spawnPosition.GetValueOrDefault();
		if (!spawnPosition.HasValue)
		{
			valueOrDefault = data.spawnPosition;
			spawnPosition = valueOrDefault;
		}
		int xMin;
		int xMax;
		(AAPoly, AAPoly) outerRooms = GetOuterRooms(data.stories[0].rooms, out xMin, out xMax);
		AAPoly item = outerRooms.Item1;
		AAPoly item2 = outerRooms.Item2;
		int num = totalLength - xMin;
		totalLength += xMax - xMin;
		AAPoly[] array = data.stories[0].rooms;
		foreach (AAPoly aAPoly in array)
		{
			if (aAPoly != null && aAPoly.Count != 0)
			{
				aAPoly.Translate(new Vector2Int(num, 0));
				if (aAPoly == item && incompleteRoom != null)
				{
					incompleteRoom.Add(aAPoly);
				}
				else
				{
					rooms.Add(aAPoly);
				}
			}
		}
		if (item2 != item || incompleteRoom == null)
		{
			incompleteRoom = item2;
		}
		if (data.stories[0].furniture != null)
		{
			HouseData.Furniture[] array2 = data.stories[0].furniture;
			for (int i = 0; i < array2.Length; i++)
			{
				HouseData.Furniture item3 = array2[i];
				item3.x += num;
				furniture.Add(item3);
			}
		}
		if (data.items != null)
		{
			HouseData.DroppedItem[] array3 = data.items;
			for (int i = 0; i < array3.Length; i++)
			{
				HouseData.DroppedItem item4 = array3[i];
				item4.pos += new Vector3(num, 0f, 0f) * 1.5f;
				items.Add(item4);
			}
		}
		if (data.attackables != null)
		{
			HouseData.Attackable[] array4 = data.attackables;
			foreach (HouseData.Attackable attackable in array4)
			{
				attackable.pos += new Vector3(num, 0f, 0f) * 1.5f;
				attackables.Add(attackable);
			}
		}
	}

	public HouseData GetData()
	{
		AAPoly aAPoly = new AAPoly();
		foreach (AAPoly room in rooms)
		{
			aAPoly.Add(room);
		}
		HouseData result = default(HouseData);
		result.spawnPosition = spawnPosition.Value;
		result.attackables = attackables.ToArray();
		result.items = items.ToArray();
		result.stories = new HouseData.Story[2]
		{
			new HouseData.Story
			{
				rooms = rooms.Prepend(null).ToArray(),
				furniture = furniture.ToArray()
			},
			new HouseData.Story
			{
				rooms = new AAPoly[1] { aAPoly }
			}
		};
		return result;
	}

	private static (AAPoly, AAPoly) GetOuterRooms(IEnumerable<AAPoly> rooms, out int xMin, out int xMax)
	{
		xMin = int.MaxValue;
		xMax = int.MinValue;
		AAPoly item = null;
		AAPoly item2 = null;
		foreach (AAPoly room in rooms)
		{
			if (room == null)
			{
				continue;
			}
			foreach (RectInt rectangle in room.GetRectangles())
			{
				if (rectangle.xMax > xMax)
				{
					xMax = rectangle.xMax;
					item2 = room;
				}
				if (rectangle.xMin < xMin)
				{
					xMin = rectangle.xMin;
					item = room;
				}
			}
		}
		return (item, item2);
	}
}
