using UnityEngine;

public class FloorFurniture : Furniture
{
	[SerializeField]
	private bool fusedToGround;

	[SerializeField]
	private bool replacesFloor;

	protected override void CalculateSize()
	{
		Bounds bounds = ModelUtility.GetBounds(base.gameObject, includeInactive: true);
		base.Size = new Vector2Int(Mathf.CeilToInt(bounds.size.x / 1.5f), Mathf.CeilToInt(bounds.size.z / 1.5f));
		if (bounds.size != Vector3.zero && !GetComponentInChildren<NavMeshSourceTag>())
		{
			ModelUtility.MakeNavMeshObstacle(base.gameObject, bounds);
		}
	}

	protected override void BeforeMoving(Building oldBuilding, Vector3Int oldCoords, Orientation oldOrientation)
	{
		oldBuilding.RemoveFromFloor(this, oldCoords.y);
		if (replacesFloor)
		{
			oldBuilding.SetHole(Furniture.GetRect(oldCoords, base.Size), oldCoords.y, to: false);
		}
	}

	protected override void AfterMoving(Building newBuilding, Vector3Int newCoords, Orientation newOrientation)
	{
		newBuilding.AddToFloor(this, newCoords.y);
		if (replacesFloor)
		{
			newBuilding.SetHole(Furniture.GetRect(newCoords, base.Size), newCoords.y);
		}
	}

	protected override bool IsValidPlace()
	{
		if (base.Networkbuilding.IsFloor(Furniture.GetRect(coords, base.Size), coords.y))
		{
			return base.Networkbuilding.IsEmpty(Furniture.GetRect(coords, base.Size), coords.y);
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
