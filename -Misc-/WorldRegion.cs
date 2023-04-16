using System.Collections.Generic;
using UnityEngine;

public abstract class WorldRegion : MonoBehaviour
{
	private readonly ICollection<GameObject> extraChildren = new List<GameObject>();

	private List<WorldRegion> group;

	public abstract AudioClip[] Music { get; }

	public abstract AudioClip StrifeMusic { get; }

	public abstract Color AmbientLight { get; }

	public abstract Color BackgroundColor { get; }

	public abstract float ZoomLevel { get; }

	public bool IsSameGroup(WorldRegion other)
	{
		if (!(this == other))
		{
			if (group != null && (bool)other)
			{
				return group == other.group;
			}
			return false;
		}
		return true;
	}

	public void SetSameGroup(WorldRegion other)
	{
		if (!(this == other))
		{
			if (other.group == null)
			{
				other.group = new List<WorldRegion> { other };
			}
			other.group.Add(this);
			group = other.group;
		}
	}

	public void AddExtraChild(GameObject child)
	{
		extraChildren.Add(child);
	}

	public void RemoveExtraChild(GameObject child)
	{
		extraChildren.Remove(child);
	}

	public void SetVisible(bool value)
	{
		if (group == null)
		{
			SetSelfVisible(value);
			return;
		}
		foreach (WorldRegion item in group)
		{
			item.SetSelfVisible(value);
		}
	}

	private void SetSelfVisible(bool value)
	{
		Visibility.Set(base.gameObject, value);
		foreach (GameObject extraChild in extraChildren)
		{
			Visibility.Set(extraChild, value);
		}
	}

	public void RefreshChildren()
	{
		RegionChild[] componentsInChildren = GetComponentsInChildren<RegionChild>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetRegion();
		}
	}
}
