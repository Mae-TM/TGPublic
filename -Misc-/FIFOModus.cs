using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

public class FIFOModus : Modus
{
	[ProtoContract]
	public struct FIFOData
	{
		[ProtoMember(1)]
		public bool avoidCollision;

		[ProtoMember(2)]
		public HouseData.Item[] data;
	}

	private Queue<Card> itemList = new Queue<Card>();

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
		SetIcon("Queue");
		SetColor(new Color(255f, 128f, 0f));
	}

	protected override bool AddItemToModus(Item toAdd)
	{
		if (itemList.Count == itemCapacity)
		{
			if (avoidCollision)
			{
				return false;
			}
			ThrowItem(itemList.Dequeue());
			itemList.Enqueue(MakeCard(toAdd, itemList.Count, 1));
		}
		else
		{
			itemList.Enqueue(MakeCard(toAdd, itemList.Count, 1));
		}
		return true;
	}

	protected override bool IsRetrievable(Card item)
	{
		if (itemList.Count != 0)
		{
			return item == itemList.Peek();
		}
		return false;
	}

	protected override IEnumerable<Card> GetItemList()
	{
		return itemList;
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
		FIFOData fIFOData = default(FIFOData);
		fIFOData.avoidCollision = avoidCollision;
		fIFOData.data = array;
		FIFOData obj = fIFOData;
		byte[] modusSpecificData = ProtobufHelpers.ProtoSerialize(obj);
		Debug.Log("Saved FIFO modus with " + obj.data.Length + " items");
		ModusData result = default(ModusData);
		result.capacity = itemCapacity;
		result.modusSpecificData = modusSpecificData;
		return result;
	}

	public override void Load(ModusData data)
	{
		FIFOData fIFOData = ProtobufHelpers.ProtoDeserialize<FIFOData>(data.modusSpecificData);
		avoidCollision = fIFOData.avoidCollision;
		itemCapacity = data.capacity;
		itemList.Clear();
		if (fIFOData.data == null)
		{
			Debug.Log("FIFO modus data is null");
			return;
		}
		Debug.Log("Loaded FIFO modus with " + fIFOData.data.Length + " items");
		for (int i = 0; i < fIFOData.data.Length; i++)
		{
			itemList.Enqueue(MakeCard(fIFOData.data[i], i, 1));
		}
	}

	protected override void Load(Item[] items)
	{
		itemList = new Queue<Card>();
		for (int i = 0; i < items.Length; i++)
		{
			itemList.Enqueue(MakeCard(items[i], i, 1));
		}
	}

	public override int GetAmount()
	{
		return itemList.Count;
	}

	protected override bool RemoveItemFromModus(Card item)
	{
		if (item == itemList.Peek())
		{
			itemList.Dequeue();
			return true;
		}
		return false;
	}
}
