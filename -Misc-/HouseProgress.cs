using ProtoBuf;

[ProtoContract]
public struct HouseProgress
{
	[ProtoContract]
	public class LandProgress
	{
		[ProtoMember(1)]
		public int[] filledChunks;
	}

	[ProtoMember(1)]
	public HouseData.Item[] atheneum;

	[ProtoMember(2)]
	public float timerTime;

	[ProtoMember(3)]
	public CruxtruderState cruxtruderState;

	[ProtoMember(4)]
	public LandProgress landProgress;
}
