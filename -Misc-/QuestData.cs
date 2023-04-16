using System.Collections.Generic;
using ProtoBuf;
using Quest.NET.Enums;

[ProtoContract]
public struct QuestData
{
	[ProtoMember(1)]
	public string questId;

	[ProtoMember(2)]
	public QuestStatus status;

	[ProtoMember(3)]
	public List<byte[]> objectives;
}
