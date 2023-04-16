using ProtoBuf;

namespace Quest.NET.Interfaces;

[ProtoContract]
public struct QuestObjectiveData
{
	[ProtoMember(1)]
	public byte status;
}
