using System;
using System.Collections.Generic;
using System.Linq;
using QuadTrees;
using UnityEngine;

public class Story : MonoBehaviour
{
	private int lowestFreeId = 1;

	private readonly List<Room> rooms = new List<Room>();

	private readonly Dictionary<Vector2Int, Room> floors = new Dictionary<Vector2Int, Room>();

	private readonly ISet<Vector2Int> holes = new HashSet<Vector2Int>();

	private readonly QuadTreeRect<FloorFurniture> furniture = new QuadTreeRect<FloorFurniture>(-30, -30, 61, 61);

	private readonly Dictionary<(Orientation, Vector2Int), WallFurniture> wallFurniture = new Dictionary<(Orientation, Vector2Int), WallFurniture>();

	private OutsideWalls outsideWalls;

	private RegionAmbience ambience;

	private Material material;

	private int y;

	private bool isGround;

	public Room Outside => rooms[0];

	public bool IsGround
	{
		get
		{
			return isGround;
		}
		set
		{
			isGround = value;
			StretchOutsideWalls(value ? 1 : 10);
		}
	}

	public static Story Make(Transform parent, int y, Material material, Material outsideMaterial, RegionAmbience ambience)
	{
		Story story = new GameObject($"Story {y}").AddComponent<Story>();
		Transform transform = story.transform;
		transform.SetParent(parent, worldPositionStays: false);
		story.y = y;
		story.isGround = y == 0;
		Room room = Room.Make(transform, y, outsideMaterial, ambience);
		story.rooms.Add(room);
		story.outsideWalls = new GameObject("Walls").AddComponent<OutsideWalls>();
		story.outsideWalls.transform.SetParent(room.transform, worldPositionStays: false);
		story.outsideWalls.SetMaterial(outsideMaterial);
		story.ambience = ambience;
		story.material = material;
		Visibility.Copy(story.gameObject, parent.gameObject);
		return story;
	}

	private int GetNewRoomId()
	{
		while (lowestFreeId < rooms.Count && (object)rooms[lowestFreeId] != null)
		{
			lowestFreeId++;
		}
		return lowestFreeId++;
	}

	private Room GetRoom(int id)
	{
		while (rooms.Count <= id)
		{
			rooms.Add(null);
		}
		Room room = rooms[id];
		if ((object)room != null)
		{
			return room;
		}
		room = Room.Make(base.transform, y, material, ambience, id, outsideWalls);
		rooms[id] = room;
		return room;
	}

	private void DestroyRoom(Room room)
	{
		rooms[room.Id] = null;
		if (room.Id < lowestFreeId)
		{
			lowestFreeId = room.Id;
		}
		UnityEngine.Object.Destroy(room.gameObject);
	}

	public Room GetRoom(Vector2Int coords)
	{
		if (!floors.TryGetValue(coords, out var value))
		{
			return Outside;
		}
		return value;
	}

	public void AddFloor(AAPoly poly, int roomId)
	{
		Room room = GetRoom(roomId);
		if (poly.Sign)
		{
			foreach (Vector2Int cell in poly.GetCells())
			{
				floors.Add(cell, room);
				if (holes.Contains(cell))
				{
					room.AddHole(cell);
				}
			}
			TransferChildren(poly, Outside, room);
		}
		else
		{
			poly.Invert();
			foreach (Vector2Int cell2 in poly.GetCells())
			{
				floors.Remove(cell2);
				if (holes.Contains(cell2))
				{
					room.RemoveHole(cell2);
				}
			}
			TransferChildren(poly, room, Outside);
			poly.Invert();
		}
		if (room.AddFloor(poly))
		{
			DestroyRoom(room);
		}
	}

	public void SetRoom(AAPoly poly, int fromId, int toId)
	{
		Room room = GetRoom(fromId);
		Room room2 = GetRoom(toId);
		foreach (Vector2Int cell in poly.GetCells())
		{
			floors[cell] = room2;
			if (holes.Contains(cell))
			{
				room2.AddHole(cell);
				room.RemoveHole(cell);
			}
		}
		TransferChildren(poly, room, room2);
		room2.AddFloor(poly);
		poly.Invert();
		if (room.AddFloor(poly))
		{
			DestroyRoom(room);
		}
		poly.Invert();
	}

	private void TransferChildren(AAPoly poly, Room from, Room to)
	{
		foreach (RectInt rectangle in poly.GetRectangles())
		{
			foreach (FloorFurniture item in furniture.EnumObjects(rectangle))
			{
				Visibility.SetParent(item, to);
			}
		}
		from.RefreshChildren();
		foreach (var wallCell in GetWallCells(poly))
		{
			if (wallFurniture.TryGetValue(wallCell, out var value))
			{
				if (value.transform.parent != from.transform)
				{
					from.RemoveExtraChild(value.gameObject);
					to.AddExtraChild(value.gameObject);
				}
				else
				{
					Visibility.SetParent(value, to);
				}
			}
		}
	}

	private static IEnumerable<(Orientation, Vector2Int)> GetWallCells(AAPoly poly)
	{
		foreach (var (tuple2, tuple3) in poly.GetSides())
		{
			var (x1, z3) = tuple2;
			var (x2, z4) = tuple3;
			if (x1 == x2)
			{
				Orientation orientation2 = ((z3 > z4) ? Orientation.EAST : Orientation.WEST);
				int z2 = Math.Min(z3, z4);
				while (z2 < Math.Max(z3, z4))
				{
					yield return (orientation2, new Vector2Int(x1, z2));
					int num = z2 + 1;
					z2 = num;
				}
			}
			else
			{
				Orientation orientation2 = ((x1 >= x2) ? Orientation.SOUTH : Orientation.NORTH);
				int z2 = Math.Min(x1, x2);
				while (z2 < Math.Max(x1, x2))
				{
					yield return (orientation2, new Vector2Int(z2, z3));
					int num = z2 + 1;
					z2 = num;
				}
			}
		}
	}

	public (int, BuildingChanges.Change) SetFloor(RectInt rect)
	{
		int num = 0;
		AAPoly aAPoly = new AAPoly();
		foreach (Vector2Int item in rect.allPositionsWithin)
		{
			if (!floors.ContainsKey(item))
			{
				aAPoly.Add(item);
				num++;
			}
		}
		return (num, new BuildingChanges.Change
		{
			room = 0,
			story = y,
			changes = aAPoly
		});
	}

	public BuildingChanges SetRoom(RectInt rect, int roomId)
	{
		if (roomId == 0)
		{
			roomId = GetNewRoomId();
		}
		BuildingChanges buildingChanges = new BuildingChanges();
		foreach (Vector2Int item in rect.allPositionsWithin)
		{
			if (floors.TryGetValue(item, out var value))
			{
				buildingChanges.Transfer(item, y, value.Id, roomId);
			}
			else
			{
				buildingChanges.Add(item, y, roomId);
			}
		}
		return buildingChanges;
	}

	public BuildingChanges RemoveRoom(RectInt rect, Story storyBelow, Story storyAbove)
	{
		Dictionary<Vector2Int, Room> dictionary = storyBelow?.floors;
		Dictionary<Vector2Int, Room> dictionary2 = storyAbove.floors;
		BuildingChanges buildingChanges = new BuildingChanges();
		foreach (Vector2Int item in rect.allPositionsWithin)
		{
			if (floors.TryGetValue(item, out var value))
			{
				if (dictionary != null && dictionary.TryGetValue(item, out var value2) && !value2.IsOutside)
				{
					buildingChanges.Transfer(item, y, value.Id, 0);
				}
				else
				{
					buildingChanges.Remove(item, y, value.Id);
				}
				if (dictionary2.TryGetValue(item, out var value3) && value3.IsOutside)
				{
					buildingChanges.Remove(item, y + 1, 0);
				}
			}
		}
		return buildingChanges;
	}

	public bool OverlapsFloorFurniture(RectInt rect)
	{
		return furniture.EnumObjects(rect).Any();
	}

	public bool AreWallChangesAllowed(AAPoly change, int from, int to)
	{
		if (from == to)
		{
			return true;
		}
		foreach (var side in change.GetSides())
		{
			(int, int) item = side.from;
			(int, int) item2 = side.to;
			int item3 = item.Item1;
			int item4 = item.Item2;
			int item5 = item2.Item1;
			int item6 = item2.Item2;
			RectInt rect = new RectInt(Math.Min(item3, item5), Math.Min(item4, item6), Math.Abs(item5 - item3), Math.Abs(item6 - item4));
			if (OverlapsFloorFurniture(rect))
			{
				return false;
			}
			bool flag = item5 == item3;
			Orientation orientation = ((!flag) ? ((item5 <= item3) ? Orientation.SOUTH : Orientation.NORTH) : ((item6 <= item4) ? Orientation.EAST : Orientation.WEST));
			Orientation orientation2 = (Orientation)((int)(orientation + 2) % 4);
			foreach (Vector2Int wallPoint in RectUtility.GetWallPoints(rect))
			{
				Vector2Int floorCell = GetFloorCell(wallPoint, orientation2);
				Room room = GetRoom(floorCell);
				if (room.Id == to)
				{
					if (wallFurniture.ContainsKey((orientation, wallPoint)) || wallFurniture.ContainsKey((orientation2, wallPoint)))
					{
						return false;
					}
				}
				else if (room.Id == from)
				{
					Vector2Int floorCell2 = GetFloorCell(wallPoint, orientation);
					if (IntersectsWallFurniture(floorCell2, floorCell, flag))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool IntersectsWallFurniture(Vector2Int cell1, Vector2Int cell2, bool isVertical)
	{
		if (isVertical)
		{
			if (wallFurniture.TryGetValue((Orientation.SOUTH, cell1), out var value) && wallFurniture.TryGetValue((Orientation.SOUTH, cell2), out var value2) && value == value2)
			{
				return true;
			}
			if (wallFurniture.TryGetValue((Orientation.NORTH, cell1 + Vector2Int.up), out value) && wallFurniture.TryGetValue((Orientation.NORTH, cell2 + Vector2Int.up), out value2) && value == value2)
			{
				return true;
			}
		}
		else
		{
			if (wallFurniture.TryGetValue((Orientation.WEST, cell1), out var value3) && wallFurniture.TryGetValue((Orientation.WEST, cell2), out var value4) && value3 == value4)
			{
				return true;
			}
			if (wallFurniture.TryGetValue((Orientation.EAST, cell1 + Vector2Int.right), out value3) && wallFurniture.TryGetValue((Orientation.EAST, cell2 + Vector2Int.right), out value4) && value3 == value4)
			{
				return true;
			}
		}
		return false;
	}

	private static Vector2Int GetFloorCell(Vector2Int wallCoords, Orientation wallOrientation)
	{
		return wallOrientation switch
		{
			Orientation.EAST => wallCoords + Vector2Int.left, 
			Orientation.NORTH => wallCoords + Vector2Int.down, 
			_ => wallCoords, 
		};
	}

	public void SetHole(RectInt rect, bool to = true)
	{
		List<Room> list = new List<Room>();
		foreach (Vector2Int item in rect.allPositionsWithin)
		{
			Room value;
			if (to ? (!holes.Add(item)) : (!holes.Remove(item)))
			{
				Debug.LogError("Attempted to double add/remove a hole!");
			}
			else if (floors.TryGetValue(item, out value))
			{
				list.Add(value);
				if (to)
				{
					value.AddHole(item);
				}
				else
				{
					value.RemoveHole(item);
				}
			}
		}
		foreach (Room item2 in list)
		{
			item2.FinalizeHoles();
		}
	}

	public void AddToFloor(FloorFurniture toAdd)
	{
		furniture.Add(toAdd);
		Visibility.SetParent(toAdd, GetRoom(toAdd.Rect.min));
	}

	public void RemoveFromFloor(FloorFurniture toRemove)
	{
		furniture.Remove(toRemove);
	}

	public void AddToWall(RectInt rect, Orientation orientation, WallFurniture toAdd, bool twoSided)
	{
		Orientation orientation2 = (Orientation)((int)(orientation + 2) % 4);
		foreach (Vector2Int wallPoint in RectUtility.GetWallPoints(rect))
		{
			if (wallFurniture.ContainsKey((orientation, wallPoint)))
			{
				Debug.LogWarning($"Trying to add a wall furniture at {orientation} that already exists!");
				continue;
			}
			wallFurniture.Add((orientation, wallPoint), toAdd);
			if (twoSided)
			{
				wallFurniture.Add((orientation2, wallPoint), toAdd);
			}
		}
		Room room = GetRoom(GetFloorCell(rect.min, orientation));
		Visibility.SetParent(toAdd, room);
		if (twoSided)
		{
			GetRoom(GetFloorCell(rect.min, orientation2)).AddExtraChild(toAdd.gameObject);
		}
	}

	public void RemoveFromWall(RectInt rect, Orientation orientation, bool twoSided)
	{
		Orientation orientation2 = (Orientation)((int)(orientation + 2) % 4);
		if (twoSided)
		{
			Room room = GetRoom(GetFloorCell(rect.min, orientation2));
			Furniture furniture = wallFurniture[(orientation, rect.min)];
			room.RemoveExtraChild(furniture.gameObject);
		}
		foreach (Vector2Int wallPoint in RectUtility.GetWallPoints(rect))
		{
			wallFurniture.Remove((orientation, wallPoint));
			if (twoSided)
			{
				wallFurniture.Remove((orientation2, wallPoint));
			}
		}
	}

	public bool ContainsFloor(RectInt rect)
	{
		if (IsGround)
		{
			return true;
		}
		foreach (Vector2Int item in rect.allPositionsWithin)
		{
			if (floors.ContainsKey(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFloor(RectInt rect)
	{
		if (IsGround)
		{
			return true;
		}
		foreach (Vector2Int item in rect.allPositionsWithin)
		{
			if (!floors.ContainsKey(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsWall(RectInt rect)
	{
		Vector2Int other = ((rect.width == 0) ? Vector2Int.left : Vector2Int.down);
		return RectUtility.GetWallPoints(rect).All((Vector2Int coords) => GetRoom(coords) != GetRoom(coords + other));
	}

	public bool IsEmptyWall(RectInt rect, Orientation orientation, bool twoSided)
	{
		if (!IsWall(rect))
		{
			return false;
		}
		Orientation orientation2 = (Orientation)((int)(orientation + 2) % 4);
		if (RectUtility.GetWallPoints(rect).Any((Vector2Int coords) => wallFurniture.ContainsKey((orientation, coords)) || (twoSided && wallFurniture.ContainsKey((orientation2, coords)))))
		{
			return false;
		}
		if (IsWallInOneRoom(rect, orientation))
		{
			return IsWallInOneRoom(rect, orientation2);
		}
		return false;
	}

	private bool IsWallInOneRoom(RectInt rect, Orientation orientation)
	{
		Room room = null;
		foreach (Vector2Int wallPoint in RectUtility.GetWallPoints(rect))
		{
			Room room2 = GetRoom(GetFloorCell(wallPoint, orientation));
			if ((object)room == null)
			{
				room = room2;
			}
			else if (room != room2)
			{
				return false;
			}
		}
		return true;
	}

	private void StretchOutsideWalls(float factor)
	{
		Transform obj = outsideWalls.transform;
		obj.localScale = new Vector3(1f, factor, 1f);
		obj.localPosition = (factor - 1f) * 3f * 1.5f * Vector3.down;
	}

	public void GenerateOutsideWalls()
	{
		outsideWalls.Generate(y);
	}

	public HouseData.Story SaveStructure()
	{
		while ((object)rooms[rooms.Count - 1] == null)
		{
			rooms.RemoveAt(rooms.Count - 1);
		}
		HouseData.Story result = default(HouseData.Story);
		result.rooms = rooms.Select((Room pair) => pair?.GetWalls()).ToArray();
		return result;
	}

	public void SaveObjects(ref HouseData.Story data)
	{
		data.furniture = (from f in furniture.GetAllObjects()
			select f.Save()).Concat(from f in wallFurniture.Values.Distinct()
			select f.Save()).ToArray();
	}

	public void Load(HouseData.Story data)
	{
		if (data.rooms == null)
		{
			return;
		}
		for (int i = 0; i < data.rooms.Length; i++)
		{
			if (data.rooms[i] != null && data.rooms[i].Count != 0)
			{
				AddFloor(data.rooms[i], i);
			}
		}
		GenerateOutsideWalls();
	}
}
