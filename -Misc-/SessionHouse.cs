using ProtoBuf;

[ProtoContract]
public struct SessionHouse
{
	[ProtoMember(1)]
	public HouseData house;

	[ProtoMember(2)]
	public HouseProgress progress;
}
