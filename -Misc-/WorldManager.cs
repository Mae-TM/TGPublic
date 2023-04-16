using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldManager : AbstractSingletonManager<WorldManager>
{
	public const float minY = -128f;

	private const int AREA_SIZE = 1024;

	public static Color[] colors;

	private readonly Dictionary<int, WorldArea> areas = new Dictionary<int, WorldArea>();

	private readonly MaxHeap<int> freeSpaces = new MaxHeap<int>((int a, int b) => b.CompareTo(a));

	private static int PlayerCount => colors.Length;

	public Func<int, WorldArea> LoadArea { private get; set; }

	public IEnumerable<House> Houses => areas.Values.OfType<House>();

	public IEnumerable<Dungeon> Dungeons => areas.Values.OfType<Dungeon>();

	public void AddArea(WorldArea area)
	{
		int index = ((freeSpaces.Count != 0) ? freeSpaces.ExtractDominating() : areas.Count);
		area.transform.localPosition = GetPosition(index);
		areas.Add(area.Id, area);
	}

	public void RemoveArea(WorldArea area)
	{
		freeSpaces.Add(GetIndex(area.transform.localPosition));
		areas.Remove(area.Id);
	}

	private static Vector3 GetPosition(int index)
	{
		return index * Vector3.right * 1024f;
	}

	private static int GetIndex(Vector3 position)
	{
		return (int)position.x / 1024;
	}

	public WorldArea GetArea(int index)
	{
		if (!areas.TryGetValue(index, out var value))
		{
			return LoadArea(index);
		}
		return value;
	}

	public static void EnsureActive(Transform parent)
	{
		while (parent != null)
		{
			if (parent.gameObject.activeSelf)
			{
				parent = parent.parent;
				continue;
			}
			parent.gameObject.SetActive(value: true);
			break;
		}
	}

	public static void SetAreaActive(GameObject area, bool to)
	{
		if (!to)
		{
			if (area.GetComponentInChildren<Player>() != null)
			{
				Debug.LogError("Tried to disable area with player!");
				return;
			}
			int num = Animator.StringToHash("Idle");
			Animator[] componentsInChildren = area.GetComponentsInChildren<Animator>();
			foreach (Animator animator in componentsInChildren)
			{
				if (animator.runtimeAnimatorController != null && animator.HasState(0, num))
				{
					animator.Play(num, 0);
					animator.Rebind();
				}
			}
		}
		area.SetActive(to);
	}

	public IEnumerable<WorldArea> GetHouses()
	{
		return areas.Values;
	}

	public House FindHouseByOwner(int ownerID)
	{
		if (!areas.TryGetValue(ownerID, out var value) || !(value is House result))
		{
			return null;
		}
		return result;
	}

	public House GetClient()
	{
		for (int num = NetcodeManager.LocalPlayerId - 1 + PlayerCount; num >= NetcodeManager.LocalPlayerId; num--)
		{
			House house = FindHouseByOwner(num % PlayerCount);
			if ((bool)house)
			{
				return house;
			}
		}
		return null;
	}

	public WorldArea GetRelativeHouse(int offset, House house)
	{
		return GetArea((house.Id + offset + PlayerCount) % PlayerCount);
	}
}
