using ProtoBuf;

[ProtoContract]
public struct SessionDungeon
{
	[ProtoMember(1)]
	public HouseData dungeon;

	[ProtoMember(2)]
	public BossRoom.Data progress;
}
