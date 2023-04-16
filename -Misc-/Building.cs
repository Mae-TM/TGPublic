using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mirror;
using RandomExtensions;
using UnityEngine;

public class Building : WorldArea
{
	public const int MAX_COORD = 30;

	public const float SIZE = 90f;

	[SerializeField]
	private Material outsideMaterial;

	[SerializeField]
	protected RegionAmbience ambience;

	private readonly List<Story> stories = new List<Story>();

	private Vector3Int spawnPosition;

	[field: SerializeField]
	public Material material { get; protected set; }

	public int StoryMax => stories.Count - 1;

	public override Vector3 SpawnPosition => GetPosition(spawnPosition);

	public WorldRegion Outside => GetStory(0).Outside;

	private void Awake()
	{
		ambience.music.Shuffle();
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		using (AbstractSingletonManager<ReallyBasicProfiler>.Instance.Track(MethodBase.GetCurrentMethod()))
		{
			if (!initialState)
			{
				return base.OnSerialize(writer, initialState: false);
			}
			writer.Write(base.Id);
			writer.Write(SaveStructure());
			return true;
		}
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		using (AbstractSingletonManager<ReallyBasicProfiler>.Instance.Track(MethodBase.GetCurrentMethod()))
		{
			if (!initialState)
			{
				base.OnDeserialize(reader, initialState: false);
				return;
			}
			Init(reader.Read<int>());
			LoadStructure(reader.Read<HouseData>());
		}
	}

	public HouseData Save()
	{
		HouseData result = SaveStructure();
		for (int i = 0; i < stories.Count; i++)
		{
			stories[i].SaveObjects(ref result.stories[i]);
		}
		result.items = (from item in GetComponentsInChildren<ItemObject>()
			where item.GetComponent<RegionChild>().enabled
			select item).Where(delegate(ItemObject item)
		{
			bool num = item.Item == null;
			if (num)
			{
				Debug.LogError("Item " + item.name + " has no item");
			}
			return !num;
		}).Select(delegate(ItemObject item)
		{
			Transform transform = item.transform;
			HouseData.DroppedItem result2 = default(HouseData.DroppedItem);
			result2.item = item.Item.Save();
			result2.pos = transform.localPosition;
			result2.rot = transform.localRotation.eulerAngles;
			return result2;
		}).ToArray();
		result.attackables = (from att in GetComponentsInChildren<Attackable>()
			where att.IsSavedWithHouse
			select att.Save()).ToArray();
		return result;
	}

	protected virtual HouseData SaveStructure()
	{
		using (AbstractSingletonManager<ReallyBasicProfiler>.Instance.Track(MethodBase.GetCurrentMethod()))
		{
			HouseData result = default(HouseData);
			result.stories = stories.Select((Story story) => story.SaveStructure()).ToArray();
			result.spawnPosition = spawnPosition;
			return result;
		}
	}

	public virtual void LoadStructure(HouseData data)
	{
		using (AbstractSingletonManager<ReallyBasicProfiler>.Instance.Track(MethodBase.GetCurrentMethod()))
		{
			if (data.stories != null)
			{
				for (int i = 0; i < data.stories.Length; i++)
				{
					GetStory(i).Load(data.stories[i]);
				}
			}
			spawnPosition = data.spawnPosition;
		}
	}

	[Server]
	public void LoadObjects(HouseData data)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Building::LoadObjects(HouseData)' called when server was not active");
			return;
		}
		if (data.stories != null)
		{
			for (int i = 0; i < data.stories.Length; i++)
			{
				HouseData.Story story = data.stories[i];
				if (story.furniture != null)
				{
					HouseData.Furniture[] furniture = story.furniture;
					for (int j = 0; j < furniture.Length; j++)
					{
						Furniture.Load(furniture[j], this, i);
					}
				}
			}
		}
		if (data.items != null)
		{
			HouseData.DroppedItem[] items = data.items;
			for (int j = 0; j < items.Length; j++)
			{
				HouseData.DroppedItem droppedItem = items[j];
				Item.Load(droppedItem.item)?.PutDownLocal(this, droppedItem.pos, Quaternion.Euler(droppedItem.rot));
			}
		}
		if (data.attackables != null)
		{
			HouseData.Attackable[] attackables = data.attackables;
			for (int j = 0; j < attackables.Length; j++)
			{
				Attackable attackable = Attackable.Load(attackables[j]);
				attackable.RegionChild.Area = this;
				NetworkServer.Spawn(attackable.gameObject);
			}
		}
	}

	protected Story GetStory(int y, bool createStories = true)
	{
		if (y < 0)
		{
			return null;
		}
		while (stories.Count <= y)
		{
			if (!createStories)
			{
				return null;
			}
			Story story = Story.Make(base.transform, stories.Count, material, outsideMaterial, ambience);
			stories.Add(story);
			story.Outside.SetSameGroup(stories[0].Outside);
		}
		return stories[y];
	}

	private static bool IsInBuildArea(RectInt rect)
	{
		if (rect.xMin >= -30 && rect.xMax <= 30 && rect.yMin >= -30)
		{
			return rect.yMax <= 30;
		}
		return false;
	}

	public bool IsFloor(RectInt rect, int story)
	{
		return stories[story].IsFloor(rect);
	}

	public bool IsEmptyWall(RectInt rect, int story, Orientation orientation, bool twoSided)
	{
		return stories[story].IsEmptyWall(rect, orientation, twoSided);
	}

	public bool IsEmpty(RectInt rect, int story)
	{
		if (!IsInBuildArea(rect))
		{
			return false;
		}
		Vector3 position = GetPosition(rect, story) + 2.25f * Vector3.up;
		position = base.transform.TransformPoint(position);
		Vector3 vector = 0.75f * new Vector3(rect.width, 2.9f, rect.height);
		return !Physics.CheckBox(position, vector - 0.1f * Vector3.one);
	}

	public void AddToFloor(FloorFurniture toAdd, int y)
	{
		stories[y].AddToFloor(toAdd);
	}

	public void RemoveFromFloor(FloorFurniture toRemove, int y)
	{
		stories[y].RemoveFromFloor(toRemove);
	}

	public void AddToWall(int y, RectInt rect, Orientation orientation, WallFurniture furniture, bool twoSided)
	{
		stories[y].AddToWall(rect, orientation, furniture, twoSided);
	}

	public void RemoveFromWall(int y, RectInt rect, Orientation orientation, bool twoSided)
	{
		stories[y].RemoveFromWall(rect, orientation, twoSided);
	}

	public void SetHole(RectInt rect, int story, bool to = true)
	{
		if (story < stories.Count)
		{
			stories[story].SetHole(rect, to);
		}
	}

	public static IEnumerable<Vector3Int> GetCoords(Vector3Int topLeft, Vector2Int size)
	{
		int x = topLeft.x;
		while (x < topLeft.x + size.x)
		{
			int num;
			for (int z = topLeft.z; z < topLeft.z + size.y; z = num)
			{
				yield return new Vector3Int(x, topLeft.y, z);
				num = z + 1;
			}
			num = x + 1;
			x = num;
		}
	}

	public static Vector3Int GetCoords(Vector3 position)
	{
		position.y /= 3f;
		return Vector3Int.FloorToInt(position / 1.5f);
	}

	public Vector3 GetWorldPosition(Vector3Int coords)
	{
		return base.transform.TransformPoint(GetPosition(coords));
	}

	public static Vector3 GetPosition(Vector3Int coords)
	{
		return GetPosition(new RectInt(coords.x, coords.z, 1, 1), coords.y);
	}

	public static Vector3 GetPosition(RectInt rect, int story)
	{
		return 1.5f * new Vector3(rect.center.x, (float)(story * 3) + 0.1f, rect.center.y);
	}

	public override WorldRegion GetRegion(Vector3 position)
	{
		Vector3Int coords = GetCoords(position);
		if (coords.y < 0)
		{
			return stories[0].Outside;
		}
		if (coords.y >= stories.Count)
		{
			return stories[stories.Count - 1].Outside;
		}
		return GetRoom(coords);
	}

	public Room GetRoom(Vector3 position)
	{
		return GetRoom(GetCoords(position));
	}

	public Room GetRoom(Vector3Int coords)
	{
		return GetStory(coords.y)?.GetRoom(new Vector2Int(coords.x, coords.z));
	}

	public int SetStoryVisible(int newStory, int oldStory = -1)
	{
		newStory = Mathf.Clamp(newStory, -1, stories.Count - 1);
		for (int i = oldStory + 1; i <= newStory; i++)
		{
			Visibility.Set(stories[i].gameObject, value: true);
		}
		if (oldStory == -1)
		{
			oldStory = stories.Count - 1;
		}
		for (int j = newStory + 1; j <= oldStory; j++)
		{
			Visibility.Set(stories[j].gameObject, value: false);
		}
		return newStory;
	}

	private void MirrorProcessed()
	{
	}
}
