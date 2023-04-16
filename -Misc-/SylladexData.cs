using ProtoBuf;

[ProtoContract]
public struct SylladexData
{
	[ProtoMember(1)]
	public string modus;

	[ProtoMember(2)]
	public ModusData modusData;

	[ProtoMember(3)]
	public Specibus.Data specibus;

	[ProtoMember(4)]
	public QuestData[] quests;

	[ProtoMember(5)]
	public string characterName;
}
