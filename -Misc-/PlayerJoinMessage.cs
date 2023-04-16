using Mirror;

public struct PlayerJoinMessage : NetworkMessage
{
	public int id;

	public HouseData house;

	public PlayerData data;
}
