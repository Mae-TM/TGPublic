using Mirror;
using ProtoBuf;

[ProtoContract]
public struct PlayerData : NetworkMessage
{
	public string name;

	public CharacterSettings character;

	public uint level;

	public float experience;

	public int[] grist;

	public Class role;

	public Aspect aspect;

	public KernelSpriteData kernelSprite;

	public ExileData exile;

	public SylladexData sylladex;

	public Item[] armor;
}
