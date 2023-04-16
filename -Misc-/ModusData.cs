using ProtoBuf;

[ProtoContract]
public struct ModusData
{
	[ProtoMember(1)]
	public int capacity;

	[ProtoMember(2)]
	public byte[] modusSpecificData;
}
