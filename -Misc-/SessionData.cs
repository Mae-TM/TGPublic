using ProtoBuf;

[ProtoContract]
public struct SessionData
{
	[ProtoMember(1)]
	public int randomSeed;

	[ProtoMember(2)]
	public ulong[] gateOrder;
}
