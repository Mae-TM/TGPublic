using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct SessionPlayer
{
	[ProtoMember(1)]
	public string name;

	[ProtoMember(2)]
	public CharacterSettings character;

	[ProtoMember(3)]
	public uint level;

	[ProtoMember(4)]
	public float experience;

	[ProtoMember(5)]
	public int[] grist;

	[ProtoMember(6)]
	public Class role;

	[ProtoMember(7)]
	public Aspect aspect;

	[ProtoMember(8)]
	public KernelSpriteData kernelSprite;

	[ProtoMember(9)]
	public ExileData exile;

	[ProtoMember(10)]
	public SylladexData sylladex;

	[ProtoMember(11)]
	public HouseData.Item[] armor;

	[ProtoMember(12)]
	public int currentArea;

	[ProtoMember(13)]
	public Vector3 position;
}
