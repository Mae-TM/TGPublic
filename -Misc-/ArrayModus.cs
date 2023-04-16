using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

public class ArrayModus : Modus
{
	[ProtoContract]
	public struct ArrayData
	{
		[ProtoMember(1)]
		public HouseData.Item[] data;
	}

	private Card[] itemList;

	private int ghostIndex;

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
				if (num < value)
				{
					array[num] = itemList[num];
				}
				else if (itemList[num] != null)
				{
					ThrowItem(itemList[num]);
				}
			}
			itemList = array;
			itemCapacity = value;
		}
	}

	private new void Awake()
	{
		base.Awake();
		itemCapacity = 4;
		SetColor(new Color(0f, 160f, 255f));
		SetIcon("Array");
		separation = new Vector2(0f, (0f - complexcardsize.y) * 3f / 4f);
		if (itemList == null)
		{
			itemList = new Card[itemCapacity];
		}
	}

	private void Update()
	{
		ghostIndex = (int)Mathf.Clamp(((float)Screen.height - Input.mousePosition.y - (base.transform as RectTransform).anchoredPosition.y) / (0f - separation.y) - 1f, 0f, itemCapacity - 1);
		if (itemList[ghostIndex] != null)
		{
			if (ghostIndex + 1 < itemCapacity && itemList[ghostIndex + 1] == null)
			{
				ghostIndex++;
			}
			else if (ghostIndex > 0 && itemList[ghostIndex - 1] == null)
			{
				ghostIndex--;
			}
		}
	}

	protected override bool AddItemToModus(Item toAdd)
	{
		if (itemList[ghostIndex] == null && Player.player.sylladex.GetDragItem() == toAdd)
		{
			itemList[ghostIndex] = MakeCard(toAdd, ghostIndex, 1);
			return true;
		}
		for (int i = 0; i < itemCapacity; i++)
		{
			if (itemList[i] == null)
			{
				itemList[i] = MakeCard(toAdd, i, 1);
				return true;
			}
		}
		return false;
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
		}
		ArrayData obj = default(ArrayData);
		obj.data = array;
		byte[] modusSpecificData = ProtobufHelpers.ProtoSerialize(obj);
		ModusData result = default(ModusData);
		result.capacity = itemCapacity;
		result.modusSpecificData = modusSpecificData;
		return result;
	}

	public override void Load(ModusData data)
	{
		itemCapacity = data.capacity;
		ArrayData arrayData = ProtobufHelpers.ProtoDeserialize<ArrayData>(data.modusSpecificData);
		if (arrayData.data == null)
		{
			itemList = new Card[itemCapacity];
			return;
		}
		HouseData.Item[] data2 = arrayData.data;
		itemList = new Card[itemCapacity];
		for (int i = 0; i < data2.Length; i++)
		{
			if (data2[i] != null)
			{
				itemList[i] = MakeCard(data2[i], i, 1);
			}
		}
	}

	public override int GetAmount()
	{
		return itemCapacity;
	}

	protected override bool RemoveItemFromModus(Card item)
	{
		int num = Array.IndexOf(itemList, item);
		itemList[num].Destroy();
		itemList[num] = null;
		return true;
	}

	public override Rect GetItemRect()
	{
		return new Rect(GetCardPosition(ghostIndex), complexcardsize);
	}

	public override bool IsActive(Item item)
	{
		if (base.gameObject.activeInHierarchy)
		{
			return itemList[ghostIndex] == null;
		}
		return false;
	}
}
