using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class OldHouseLoader
{
	private static Vector3 offset = Vector3.up * 0.1f * 1.5f / 2f;

	public static HouseData LoadOld(Stream stream)
	{
		using BinaryReader binaryReader = new BinaryReader(stream);
		HouseData houseData = default(HouseData);
		houseData.version = (ushort)binaryReader.ReadInt32();
		houseData.stories = new HouseData.Story[binaryReader.ReadInt16()];
		HouseData result = houseData;
		List<List<HouseData.Furniture>> list = new List<List<HouseData.Furniture>>(result.stories.Length);
		short num;
		for (int i = 0; i < result.stories.Length; i++)
		{
			HouseData.Story story = LoadRooms(binaryReader);
			list.Add(new List<HouseData.Furniture>());
			num = binaryReader.ReadInt16();
			for (int j = 0; j < num; j++)
			{
				list[i].Add(LoadFurniture(binaryReader));
			}
			num = binaryReader.ReadInt16();
			for (int k = 0; k < num; k++)
			{
				list[i].Add(LoadFurniture(binaryReader, isFloor: true));
			}
			num = binaryReader.ReadInt16();
			story.brokenGround = new RectInt[num];
			for (int l = 0; l < num; l++)
			{
				story.brokenGround[l] = LoadBrokenGround(binaryReader);
			}
			result.stories[i] = story;
		}
		for (int m = 0; m < result.stories.Length; m++)
		{
			num = binaryReader.ReadInt16();
			for (int n = 0; n < num; n++)
			{
				LoadStairs(binaryReader, list);
			}
			result.stories[m].furniture = list[0].ToArray();
			list.RemoveAt(0);
		}
		num = binaryReader.ReadInt16();
		result.items = new HouseData.DroppedItem[num];
		for (int num2 = 0; num2 < num; num2++)
		{
			result.items[num2].item = LoadItem(binaryReader);
			result.items[num2].pos = LoadVector(binaryReader) + offset;
			result.items[num2].rot = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()).eulerAngles;
		}
		num = binaryReader.ReadInt16();
		result.attackables = new HouseData.Attackable[num];
		for (int num3 = 0; num3 < num; num3++)
		{
			result.attackables[num3] = LoadAttackable(binaryReader);
		}
		num = binaryReader.ReadInt16();
		result.spawnPosition = Building.GetCoords(LoadVector(binaryReader));
		num = binaryReader.ReadInt16();
		for (int num4 = 0; num4 < num; num4++)
		{
			LoadItem(binaryReader);
		}
		result.background = ((stream.Position < stream.Length) ? LoadString(binaryReader) : "suburban");
		return result;
	}

	private static IEnumerable<(int from, int to, int pos)> LoadFloor(BinaryReader reader)
	{
		while (true)
		{
			short num = reader.ReadInt16();
			if (num != short.MinValue)
			{
				short item = reader.ReadInt16();
				short num2 = reader.ReadInt16();
				yield return (num, num + num2, item);
				continue;
			}
			break;
		}
	}

	private static IEnumerable<Vector2Int> LoadWalls(BinaryReader reader)
	{
		short count = reader.ReadInt16();
		int i = 0;
		while (i < count)
		{
			yield return new Vector2Int(reader.ReadInt16(), reader.ReadInt16());
			int num = i + 1;
			i = num;
		}
	}

	private static string LoadString(BinaryReader reader)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			byte b = reader.ReadByte();
			if (b == 0)
			{
				break;
			}
			stringBuilder.Append((char)b);
		}
		return stringBuilder.ToString();
	}

	private static Vector3 LoadVector(BinaryReader reader)
	{
		return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
	}

	private static HouseData.Furniture LoadFurniture(BinaryReader reader, bool isFloor = false)
	{
		string text = LoadString(reader);
		if (string.IsNullOrEmpty(text))
		{
			return default(HouseData.Furniture);
		}
		if (isFloor && text == "AC wall")
		{
			text = "AC ceiling";
		}
		HouseData.Furniture furniture = default(HouseData.Furniture);
		furniture.name = text;
		furniture.orientation = (Orientation)reader.ReadByte();
		furniture.x = reader.ReadInt16();
		furniture.z = reader.ReadInt16();
		HouseData.Furniture result = furniture;
		short num = reader.ReadInt16();
		if (num == 0)
		{
			if (new Dictionary<string, string>
			{
				{ "poster flamingsteed", "FlmngStd" },
				{ "poster failtolaunch", "GameBroP" },
				{ "poster ps", "BoilHard" },
				{ "poster sbahj", "SordSwHa" },
				{ "poster furry sbahj", "FurrSbHj" },
				{ "poster squiddles", "GrimPstr" },
				{ "poster gamegrl", "GameGrlP" },
				{ "poster ghostbusters", "HotGODps" },
				{ "poster conair", "CagePstr" },
				{ "poster gamebro", "GameBroP" },
				{ "poster ghostdad", "SordSwHa" },
				{ "poster little monsters", "SBURBCal" },
				{ "poster magic girl", "GntlTrrr" },
				{ "poster sburb", "SBURBBTA" },
				{ "poster slimer", "SlimPstr" },
				{ "poster time to kill", "LddrPstr" },
				{ "poster sweetbro", "HotGODps" },
				{ "calendar", "SBURBCal" }
			}.TryGetValue(text.ToLowerInvariant(), out var value))
			{
				result.name = "Poster nail";
				result.items = new HouseData.Item[1]
				{
					new HouseData.NormalItem
					{
						code = value
					}
				};
			}
			return result;
		}
		result.items = new HouseData.Item[num];
		for (int i = 0; i < num; i++)
		{
			result.items[i] = LoadItem(reader);
		}
		return result;
	}

	private static RectInt LoadBrokenGround(BinaryReader reader)
	{
		RectInt result = new RectInt(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
		reader.ReadByte();
		return result;
	}

	private static HouseData.Item LoadItem(BinaryReader reader)
	{
		switch (reader.ReadByte())
		{
		default:
			return null;
		case 0:
			return new HouseData.NormalItem
			{
				code = LoadString(reader)
			};
		case 1:
			return new HouseData.Totem
			{
				result = LoadItem(reader),
				color = LoadVector(reader)
			};
		case 2:
			return new HouseData.PunchCard
			{
				result = LoadItem(reader),
				original = LoadItem(reader)
			};
		case 3:
			return new HouseData.NormalItem
			{
				code = LoadString(reader),
				isEntry = true
			};
		case 4:
		{
			HouseData.AlchemyItem alchemyItem = new HouseData.AlchemyItem
			{
				armor = (ArmorKind)reader.ReadByte(),
				code = LoadString(reader)
			};
			reader.ReadSingle();
			alchemyItem.power = reader.ReadSingle();
			alchemyItem.speed = reader.ReadSingle();
			alchemyItem.size = reader.ReadSingle();
			alchemyItem.animation = LoadString(reader);
			int num2 = reader.ReadByte();
			for (int j = 0; j < num2; j++)
			{
				alchemyItem.weaponKind = (WeaponKind)reader.ReadByte();
			}
			alchemyItem.tags = new NormalItem.Tag[reader.ReadByte()];
			for (int k = 0; k < alchemyItem.tags.Length; k++)
			{
				alchemyItem.tags[k] = (NormalItem.Tag)reader.ReadByte();
			}
			alchemyItem.customTags = new NormalItem.Tag[reader.ReadByte()];
			for (int l = 0; l < alchemyItem.customTags.Length; l++)
			{
				alchemyItem.customTags[l] = (NormalItem.Tag)reader.ReadByte();
			}
			alchemyItem.equipSprite = LoadString(reader);
			alchemyItem.sprite = LoadString(reader);
			alchemyItem.name = LoadString(reader);
			return alchemyItem;
		}
		case 5:
		{
			HouseData.NormalItem normalItem = new HouseData.NormalItem
			{
				code = LoadString(reader)
			};
			short num = reader.ReadInt16();
			if (num == 0)
			{
				return normalItem;
			}
			normalItem.contents = new HouseData.Item[num];
			for (int i = 0; i < num; i++)
			{
				normalItem.contents[i] = LoadItem(reader);
			}
			return normalItem;
		}
		}
	}

	private static void LoadStairs(BinaryReader reader, IReadOnlyList<List<HouseData.Furniture>> data)
	{
		int num = reader.ReadInt16();
		int num2 = reader.ReadInt16();
		int num3 = reader.ReadInt16();
		int num4 = reader.ReadInt16();
		Orientation orientation = (Orientation)reader.ReadByte();
		if (orientation == Orientation.NORTH || orientation == Orientation.SOUTH)
		{
			int num5 = (num4 - num2) / 3;
			for (int i = 0; i < num5; i++)
			{
				int z = ((orientation == Orientation.NORTH) ? (num2 + i * 3) : (num4 - (i + 1) * 3));
				for (int j = num; j < num3; j++)
				{
					data[i].Add(new HouseData.Furniture
					{
						name = "Stairs",
						x = j,
						z = z,
						orientation = orientation
					});
				}
			}
			return;
		}
		int num6 = (num3 - num) / 3;
		for (int k = 0; k < num6; k++)
		{
			int x = ((orientation == Orientation.WEST) ? (num + k * 3) : (num3 - (k + 1) * 3));
			for (int l = num2; l < num4; l++)
			{
				data[k].Add(new HouseData.Furniture
				{
					name = "Stairs",
					x = x,
					z = l,
					orientation = orientation
				});
			}
		}
	}

	private static HouseData.Attackable LoadAttackable(BinaryReader reader)
	{
		string text = LoadString(reader);
		Vector3 pos = LoadVector(reader) + offset;
		float health = reader.ReadSingle();
		HouseData.Attackable attackable;
		switch (text)
		{
		case "Bee":
		case "Murderbee":
		case "Guardian":
			attackable = new HouseData.Attackable();
			break;
		default:
			attackable = new HouseData.Enemy
			{
				type = reader.ReadByte()
			};
			break;
		}
		attackable.name = text;
		attackable.pos = pos;
		attackable.health = health;
		reader.ReadInt32();
		reader.ReadInt32();
		return attackable;
	}

	private static HouseData.Story LoadRooms(BinaryReader reader)
	{
		AAPoly aAPoly = new AAPoly();
		ISet<Vector2Int> set = new HashSet<Vector2Int>();
		foreach (var item4 in LoadFloor(reader))
		{
			int item = item4.from;
			int item2 = item4.to;
			int item3 = item4.pos;
			for (int i = item; i < item2; i++)
			{
				set.Add(new Vector2Int(i, item3));
			}
			aAPoly.Add(item2, item, item3);
			aAPoly.Add(item, item2, item3 + 1);
		}
		aAPoly.CancelOpposites();
		ISet<Vector2Int> set2 = new HashSet<Vector2Int>(LoadWalls(reader));
		ISet<Vector2Int> set3 = new HashSet<Vector2Int>(LoadWalls(reader));
		List<AAPoly> list = new List<AAPoly>();
		Stack<Vector2Int> stack = new Stack<Vector2Int>();
		list.Add(aAPoly);
		while (set.Count != 0)
		{
			AAPoly aAPoly2 = new AAPoly();
			stack.Push(set.First());
			do
			{
				Vector2Int vector2Int = stack.Pop();
				if (set.Remove(vector2Int))
				{
					if (!set2.Contains(vector2Int))
					{
						stack.Push(vector2Int + Vector2Int.left);
					}
					if (!set3.Contains(vector2Int))
					{
						stack.Push(vector2Int + Vector2Int.down);
					}
					else
					{
						aAPoly2.Add(vector2Int.x + 1, vector2Int.x, vector2Int.y);
					}
					if (!set2.Contains(vector2Int + Vector2Int.right))
					{
						stack.Push(vector2Int + Vector2Int.right);
					}
					if (!set3.Contains(vector2Int + Vector2Int.up))
					{
						stack.Push(vector2Int + Vector2Int.up);
					}
					else
					{
						aAPoly2.Add(vector2Int.x, vector2Int.x + 1, vector2Int.y + 1);
					}
				}
			}
			while (stack.Count != 0);
			if (aAPoly2.Count != 0 && aAPoly2.Sign && aAPoly2.IsValid())
			{
				list.Add(aAPoly2);
				aAPoly.Remove(aAPoly2);
			}
		}
		HouseData.Story result = default(HouseData.Story);
		result.rooms = list.ToArray();
		return result;
	}
}
