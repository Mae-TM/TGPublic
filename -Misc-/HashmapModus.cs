using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

public class HashmapModus : Modus
{
	[ProtoContract]
	public struct HashmapData
	{
		[ProtoMember(1)]
		public bool avoidCollision;

		[ProtoMember(2)]
		public HouseData.Item[] data;
	}

	private uint[] charVal = new uint[26]
	{
		1u, 0u, 0u, 0u, 1u, 0u, 0u, 0u, 1u, 0u,
		0u, 0u, 0u, 0u, 1u, 0u, 0u, 0u, 0u, 0u,
		1u, 0u, 0u, 0u, 1u, 0u
	};

	private static readonly char[] vowel = new char[6] { 'a', 'e', 'i', 'o', 'u', 'y' };

	private Card[] itemList;

	private bool avoidCollision;

	public override int ItemCapacity
	{
		get
		{
			return itemCapacity;
		}
		set
		{
			Card[] array = new Card[value];
			for (uint num = 0u; num < itemCapacity; num++)
			{
				if (itemList[num] != null)
				{
					if (num < value)
					{
						array[Hash(itemList[num].item)] = itemList[num];
					}
					else
					{
						ThrowItem(itemList[num]);
					}
				}
			}
			itemList = array;
			itemCapacity = value;
		}
	}

	private new void Awake()
	{
		base.Awake();
		itemCapacity = 6;
		AddToggle("detect collisions", status: false, delegate(bool to)
		{
			avoidCollision = to;
		});
		separation = new Vector2(0f, (0f - complexcardsize.y) / 2f);
		SetColor(new Color(255f, 255f, 0f));
		SetIcon("Hashmap");
		if (itemList == null)
		{
			itemList = new Card[itemCapacity];
		}
	}

	private uint Hash(Item toHash)
	{
		char[] array = toHash.GetItemName().ToLower().ToCharArray();
		uint num = 0u;
		char[] array2 = array;
		foreach (char c in array2)
		{
			if (c >= 'a' && c < '{')
			{
				num += charVal[c - 97];
			}
		}
		return num % (uint)itemCapacity;
	}

	protected override bool AddItemToModus(Item toAdd)
	{
		uint num = Hash(toAdd);
		if (itemList[num] != null)
		{
			if (avoidCollision)
			{
				return false;
			}
			ThrowItem(itemList[num]);
			itemList[num] = MakeCard(toAdd, (int)num, 1);
		}
		else
		{
			itemList[num] = MakeCard(toAdd, (int)num, 1);
		}
		return true;
	}

	protected override bool IsRetrievable(Card item)
	{
		return true;
	}

	protected override IEnumerable<Card> GetItemList()
	{
		return itemList;
	}

	public override ModusData Save()
	{
		HouseData.Item[] array = new HouseData.Item[itemList.Length];
		for (int i = 0; i < itemList.Length; i++)
		{
			Card card = itemList[i];
			if (card != null && card.item != null)
			{
				array[i] = card.item.Save();
			}
			else
			{
				array[i] = null;
			}
		}
		HashmapData obj = default(HashmapData);
		obj.avoidCollision = avoidCollision;
		obj.data = array;
		byte[] modusSpecificData = ProtobufHelpers.ProtoSerialize(obj);
		ModusData result = default(ModusData);
		result.capacity = itemCapacity;
		result.modusSpecificData = modusSpecificData;
		return result;
	}

	public override void Load(ModusData data)
	{
		HashmapData hashmapData = ProtobufHelpers.ProtoDeserialize<HashmapData>(data.modusSpecificData);
		avoidCollision = hashmapData.avoidCollision;
		itemCapacity = data.capacity;
		itemList = new Card[itemCapacity];
		if (hashmapData.data != null)
		{
			HouseData.Item[] data2 = hashmapData.data;
			foreach (Item item in data2)
			{
				int num = (int)Hash(item);
				itemList[num] = MakeCard(item, num, 1);
			}
		}
	}

	protected override void Load(Item[] items)
	{
		itemList = new Card[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null)
			{
				itemList[i] = MakeCard(items[i], i, 1);
			}
		}
	}

	public override int GetAmount()
	{
		return itemCapacity;
	}

	protected override bool RemoveItemFromModus(Card item)
	{
		itemList[Hash(item.item)] = null;
		return true;
	}

	public override Rect GetItemRect()
	{
		return new Rect(GetCardPosition((int)Hash(sylladex.GetDragItem())), complexcardsize);
	}
}
