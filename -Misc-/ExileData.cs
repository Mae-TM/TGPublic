using ProtoBuf;

[ProtoContract]
public struct ExileData
{
	[ProtoMember(1)]
	public Exile.Action action;

	[ProtoMember(2)]
	public bool isTalking;

	[ProtoMember(3)]
	public uint type;
}
