using UnityEngine;

public class WallFurniture : Furniture
{
	[SerializeField]
	private bool twoSided;

	protected override void CalculateSize()
	{
		base.Size = new Vector2Int(Mathf.CeilToInt(ModelUtility.GetBounds(base.gameObject, includeInactive: true).size.x / 1.5f), 0);
	}

	protected override void BeforeMoving(Building oldBuilding, Vector3Int oldCoords, Orientation oldOrientation)
	{
		oldBuilding.RemoveFromWall(oldCoords.y, Furniture.GetRect(oldCoords, base.Size), oldOrientation, twoSided);
	}

	protected override void AfterMoving(Building newBuilding, Vector3Int newCoords, Orientation newOrientation)
	{
		newBuilding.AddToWall(newCoords.y, Furniture.GetRect(newCoords, base.Size), newOrientation, this, twoSided);
	}

	protected override bool IsValidPlace()
	{
		return base.Networkbuilding.IsEmptyWall(Furniture.GetRect(coords, base.Size), coords.y, orientation, twoSided);
	}

	private void MirrorProcessed()
	{
	}
}
