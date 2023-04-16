using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator
{
	private readonly int minRoomSize;

	private readonly List<RectInt> rooms = new List<RectInt>();

	private readonly List<HouseData.Furniture> furniture = new List<HouseData.Furniture>();

	private const string DOOR = "Archway";

	private const string CHEST = "Chest 1";

	private static readonly string[][] furnitureOptions = new string[5][]
	{
		new string[1] { "shackles_2" },
		new string[1] { "Mushroom 1" },
		new string[5] { "Rock 1", "Rock 2", "Rock 3", "Rock 4", "Rock 5" },
		new string[1] { "firestand" },
		new string[1] { "beartrap" }
	};

	private static readonly Enemy[] enemyOptions = SpawnHelper.instance.GetCreatures<Enemy>(new string[5] { "Imp", "Gremlin", "Lich", "Titachnid", "Ogre" });

	public static HouseData Generate(int width, int height, int minRoomSize, Queue<HouseData.Item> items, Aspect aspect)
	{
		DungeonGenerator dungeonGenerator = new DungeonGenerator(minRoomSize);
		dungeonGenerator.SubdivideArea(0, 0, width, height);
		return dungeonGenerator.Fill(width, height, items, "aspect_chest-" + aspect);
	}

	private DungeonGenerator(int minRoomSize)
	{
		this.minRoomSize = minRoomSize;
	}

	private HouseData Fill(int width, int height, Queue<HouseData.Item> items, string aspectChest)
	{
		int num = width * height;
		Array.Sort(enemyOptions);
		HouseData.Attackable[] array = (from att in GenerateEnemies(enemyOptions, num, num * 2 / 25)
			select att.Save()).ToArray();
		int num2 = array.Length;
		bool flag = true;
		for (int num3 = rooms.Count - 1; num3 > 0; num3--)
		{
			RectInt rectInt = rooms[num3];
			Vector2Int vector2Int = Vector2Int.FloorToInt(rectInt.center);
			HouseData.Furniture furniture = default(HouseData.Furniture);
			furniture.orientation = (Orientation)UnityEngine.Random.Range(0, 4);
			furniture.x = vector2Int.x;
			furniture.z = vector2Int.y;
			HouseData.Furniture item = furniture;
			if (flag || UnityEngine.Random.Range(0, 3 * num3) < items.Count)
			{
				item.name = (flag ? aspectChest : "Chest 1");
				item.items = new HouseData.Item[9];
				FillChest(item.items, items, flag ? 0.5f : 0.3f);
				flag = false;
			}
			else
			{
				string[] array2 = furnitureOptions[UnityEngine.Random.Range(0, furnitureOptions.Length)];
				item.name = array2[UnityEngine.Random.Range(0, array2.Length)];
			}
			this.furniture.Add(item);
			foreach (Vector2Int item2 in rectInt.allPositionsWithin)
			{
				if (UnityEngine.Random.Range(0, num) < num2)
				{
					array[--num2].pos = Building.GetPosition(new Vector3Int(item2.x, 0, item2.y));
				}
				num--;
			}
		}
		HouseData result = default(HouseData);
		result.spawnPosition = new Vector3Int(rooms[0].x + rooms[0].width / 2, 0, rooms[0].y + rooms[0].height / 2);
		result.attackables = array;
		result.stories = new HouseData.Story[2]
		{
			new HouseData.Story
			{
				rooms = rooms.Select((RectInt room) => new AAPoly { room }).Prepend(null).ToArray(),
				furniture = this.furniture.ToArray()
			},
			new HouseData.Story
			{
				rooms = new AAPoly[1]
				{
					new AAPoly
					{
						new RectInt(0, 0, width, height)
					}
				}
			}
		};
		return result;
	}

	private static void FillChest(HouseData.Item[] slots, Queue<HouseData.Item> items, float rate = 0.3f)
	{
		int num = 1;
		for (int i = 1; i < slots.Length; i++)
		{
			if (UnityEngine.Random.value < rate)
			{
				num++;
			}
		}
		for (int num2 = slots.Length; num2 > 0; num2--)
		{
			if (items.Count != 0 && UnityEngine.Random.Range(0, num2) < num)
			{
				slots[num2 - 1] = items.Dequeue();
				num--;
			}
		}
	}

	private static IEnumerable<Enemy> GenerateEnemies(IReadOnlyList<Enemy> enemies, int max, int value, int rate = 25)
	{
		while (max > 0)
		{
			if (UnityEngine.Random.Range(0, rate) == 0)
			{
				int i;
				for (i = 0; i < enemies.Count && enemies[i].GetCost() <= Mathf.Min(value * 2 * rate / max, value); i++)
				{
				}
				if (i == 0)
				{
					continue;
				}
				int index = UnityEngine.Random.Range(0, i);
				yield return enemies[index];
				value -= enemies[index].GetCost();
				if (value <= 0)
				{
					break;
				}
			}
			int num = max - 1;
			max = num;
		}
	}

	private void SubdivideArea(int x, int y, int width, int height)
	{
		if (width < 2 * minRoomSize && height < 2 * minRoomSize)
		{
			rooms.Add(new RectInt(x, y, width, height));
		}
		else if (width > height || (width == height && (double)UnityEngine.Random.value > 0.5))
		{
			int num = UnityEngine.Random.Range(minRoomSize, width - minRoomSize + 1);
			furniture.Add(new HouseData.Furniture
			{
				name = "Archway",
				orientation = Orientation.EAST,
				x = x + num,
				z = UnityEngine.Random.Range(y, y + height)
			});
			SubdivideArea(x, y, num, height);
			SubdivideArea(x + num, y, width - num, height);
		}
		else
		{
			int num2 = UnityEngine.Random.Range(minRoomSize, height - minRoomSize + 1);
			furniture.Add(new HouseData.Furniture
			{
				name = "Archway",
				orientation = Orientation.NORTH,
				x = UnityEngine.Random.Range(x, x + width),
				z = y + num2
			});
			SubdivideArea(x, y, width, num2);
			SubdivideArea(x, y + num2, width, height - num2);
		}
	}
}
