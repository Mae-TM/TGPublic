using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

public class FILOModus : Modus
{
	[ProtoContract]
	public struct FILOData
	{
		[ProtoMember(1)]
		public bool avoidCollision;

		[ProtoMember(2)]
		public HouseData.Item[] data;
	}

	private LinkedList<Card> itemList = new LinkedList<Card>();

	private bool avoidCollision;

	private new void Awake()
	{
		base.Awake();
		itemCapacity = 8;
		AddToggle("detect collisions", status: false, delegate(bool to)
		{
			avoidCollision = to;
		});
		separation = new Vector2((0f - complexcardsize.x) / 4f, complexcardsize.y / 4f);
		SetColor(new Color(255f, 0f, 128f));
		SetIcon("Stack");
	}

	protected override bool AddItemToModus(Item toAdd)
	{
		if (itemList.Count == itemCapacity)
		{
			if (avoidCollision)
			{
				return false;
			}
			ThrowItem(itemList.Last());
			itemList.RemoveLast();
			itemList.AddFirst(MakeCard(toAdd, itemList.Count, -1));
		}
		else
		{
			itemList.AddFirst(MakeCard(toAdd, itemList.Count, -1));
		}
		return true;
	}

	protected override bool IsRetrievable(Card item)
	{
		if (itemList.Count != 0)
		{
			return item == itemList.First();
		}
		return false;
	}

	protected override IEnumerable<Card> GetItemList()
	{
		return itemList.Reverse();
	}

	public override ModusData Save()
	{
		HouseData.Item[] array = new HouseData.Item[itemList.Count];
		int num = 0;
		foreach (Card item in GetItemList())
		{
			if (item != null && item.item != null)
			{
				array[num] = item.item.Save();
			}
			else
			{
				array[num] = null;
			}
			num++;
		}
		FILOData obj = default(FILOData);
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
		FILOData fILOData = ProtobufHelpers.ProtoDeserialize<FILOData>(data.modusSpecificData);
		avoidCollision = fILOData.avoidCollision;
		itemCapacity = data.capacity;
		itemList.Clear();
		if (fILOData.data != null)
		{
			for (int i = 0; i < fILOData.data.Length; i++)
			{
				itemList.AddFirst(MakeCard(fILOData.data[i], i, -1));
			}
		}
	}

	protected override void Load(Item[] items)
	{
		itemList = new LinkedList<Card>();
		for (int i = 0; i < items.Length; i++)
		{
			itemList.AddFirst(MakeCard(items[i], i, -1));
		}
	}

	public override int GetAmount()
	{
		return itemList.Count;
	}

	protected override bool RemoveItemFromModus(Card item)
	{
		if (item == itemList.First())
		{
			itemList.RemoveFirst();
			return true;
		}
		return false;
	}
}
