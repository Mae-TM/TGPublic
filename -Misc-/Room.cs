using UnityEngine;

public class Room : WorldRegion
{
	public const int OUTSIDE = 0;

	private int y;

	private RoomMesh roomMesh;

	private RegionAmbience ambience;

	public int Id { get; private set; }

	public bool IsOutside => Id == 0;

	public override AudioClip[] Music => ambience.music;

	public override AudioClip StrifeMusic => ambience.strifeMusic;

	public override Color AmbientLight => ambience.ambientLight;

	public override Color BackgroundColor => (IsOutside ? 0.1f : 0.78f) * Color.white;

	public override float ZoomLevel
	{
		get
		{
			if (!IsOutside)
			{
				return 1f;
			}
			return 1.6666666f;
		}
	}

	public static Room Make(Transform parent, int y, Material material, RegionAmbience ambience, int id = 0, OutsideWalls outside = null)
	{
		Room room = new GameObject($"Room {id}").AddComponent<Room>();
		room.roomMesh = room.gameObject.AddComponent<RoomMesh>();
		room.roomMesh.SetMaterial(material);
		room.roomMesh.outside = outside;
		room.transform.SetParent(parent, worldPositionStays: false);
		room.Id = id;
		room.y = y;
		room.ambience = ambience;
		Visibility.Copy(room.gameObject, parent.gameObject);
		return room;
	}

	public bool AddFloor(AAPoly poly)
	{
		return roomMesh.Add(poly, y);
	}

	public void AddHole(Vector2Int cell)
	{
		roomMesh.RemoveFloor(cell);
	}

	public void RemoveHole(Vector2Int cell)
	{
		roomMesh.AddFloor(cell);
	}

	public void FinalizeHoles()
	{
		roomMesh.FinalizeFloor(y);
	}

	public AAPoly GetWalls()
	{
		return roomMesh.GetWalls();
	}
}
