using System;
using UnityEngine;

public class Stairs : FloorFurniture
{
	[SerializeField]
	[HideInInspector]
	private NavMeshSourceTag navTag;

	private Vector2Int Origin
	{
		get
		{
			Vector2Int result = orientation switch
			{
				Orientation.NORTH => new Vector2Int(coords.x, coords.z - 1), 
				Orientation.EAST => new Vector2Int(coords.x - 1, coords.z), 
				Orientation.SOUTH => new Vector2Int(coords.x, coords.z + base.Size.y), 
				Orientation.WEST => new Vector2Int(coords.x + base.Size.x, coords.z), 
				_ => throw new InvalidOperationException(), 
			};
			return result;
		}
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if ((object)navTag == null)
		{
			navTag = GetComponentInChildren<NavMeshSourceTag>();
		}
		if (!navTag)
		{
			Collider componentInChildren = GetComponentInChildren<Collider>();
			if ((bool)componentInChildren)
			{
				navTag = componentInChildren.gameObject.AddComponent<NavMeshSourceTag>();
			}
		}
	}

	protected override void BeforeMoving(Building oldBuilding, Vector3Int oldCoords, Orientation oldOrientation)
	{
		base.BeforeMoving(oldBuilding, oldCoords, oldOrientation);
		oldBuilding.SetHole(Furniture.GetRect(oldCoords, base.Size), oldCoords.y + 1, to: false);
	}

	protected override void AfterMoving(Building newBuilding, Vector3Int newCoords, Orientation newOrientation)
	{
		base.AfterMoving(newBuilding, newCoords, newOrientation);
		newBuilding.SetHole(Furniture.GetRect(newCoords, base.Size), newCoords.y + 1);
		navTag.Refresh();
	}

	protected override bool IsValidPlace()
	{
		if (base.Networkbuilding.IsFloor(new RectInt(Origin, Vector2Int.one), coords.y))
		{
			return base.Networkbuilding.IsEmpty(Furniture.GetRect(coords, base.Size), coords.y);
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
