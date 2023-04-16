using ProtoBuf;

[ProtoContract]
public class KernelSpriteData
{
	[ProtoMember(1)]
	public bool hasEntered;

	[ProtoMember(2)]
	public string[] prototypes;
}
