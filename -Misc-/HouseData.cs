using Mirror;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public struct HouseData : NetworkMessage
{
	[ProtoContract]
	public struct Story
	{
		[ProtoMember(1)]
		public AAPoly[] rooms;

		[ProtoMember(2)]
		public Furniture[] furniture;

		[ProtoMember(3)]
		public RectInt[] brokenGround;
	}

	[ProtoContract]
	public struct Furniture
	{
		[ProtoMember(1)]
		public string name;

		[ProtoMember(2, DataFormat = DataFormat.ZigZag)]
		public int x;

		[ProtoMember(3, DataFormat = DataFormat.ZigZag)]
		public int z;

		[ProtoMember(4)]
		public Orientation orientation;

		[ProtoMember(5)]
		public Item[] items;
	}

	[ProtoContract]
	[ProtoInclude(1, typeof(NormalItem))]
	[ProtoInclude(2, typeof(Totem))]
	[ProtoInclude(3, typeof(PunchCard))]
	[ProtoInclude(4, typeof(AlchemyItem))]
	public class Item : NetworkMessage
	{
	}

	[ProtoContract]
	public class NormalItem : Item
	{
		[ProtoMember(1)]
		public string code;

		[ProtoMember(2)]
		public Item[] contents;

		[ProtoMember(3)]
		public bool isEntry;
	}

	[ProtoContract]
	public class Totem : Item
	{
		[ProtoMember(1)]
		public Item result;

		[ProtoMember(2)]
		public Vector3 color;
	}

	[ProtoContract]
	public class PunchCard : Item
	{
		[ProtoMember(1)]
		public Item result;

		[ProtoMember(2)]
		public Item original;
	}

	[ProtoContract]
	public class AlchemyItem : Item
	{
		[ProtoMember(1)]
		public string code;

		[ProtoMember(2)]
		public string name;

		[ProtoMember(3)]
		public float power;

		[ProtoMember(4)]
		public float speed;

		[ProtoMember(5)]
		public float size;

		[ProtoMember(6)]
		public string animation;

		[ProtoMember(7)]
		public WeaponKind weaponKind;

		[ProtoMember(8)]
		public ArmorKind armor;

		[ProtoMember(9)]
		public global::NormalItem.Tag[] tags;

		[ProtoMember(10)]
		public global::NormalItem.Tag[] customTags;

		[ProtoMember(11)]
		public string equipSprite;

		[ProtoMember(12)]
		public string sprite;
	}

	[ProtoContract]
	public struct DroppedItem
	{
		[ProtoMember(1)]
		public Item item;

		[ProtoMember(2)]
		public Vector3 pos;

		[ProtoMember(3)]
		public Vector3 rot;
	}

	[ProtoContract]
	[ProtoInclude(10, typeof(Enemy))]
	[ProtoInclude(11, typeof(Consort))]
	public class Attackable
	{
		[ProtoMember(1)]
		public string name;

		[ProtoMember(2)]
		public Vector3 pos;

		[ProtoMember(3)]
		public float health;

		[ProtoMember(4)]
		public StatusEffect.Data[] statusEffects;
	}

	[ProtoContract]
	public class Enemy : Attackable
	{
		[ProtoMember(1)]
		public int type;
	}

	[ProtoContract]
	public class Consort : Attackable
	{
		[ProtoMember(1)]
		public global::Consort.Job job;

		[ProtoMember(2)]
		public string[] quests;
	}

	[ProtoMember(1)]
	public ushort version;

	[ProtoMember(2)]
	public Story[] stories;

	[ProtoMember(3)]
	public Vector3Int spawnPosition;

	[ProtoMember(4)]
	public string background;

	[ProtoMember(5)]
	public DroppedItem[] items;

	[ProtoMember(6)]
	public Attackable[] attackables;
}
