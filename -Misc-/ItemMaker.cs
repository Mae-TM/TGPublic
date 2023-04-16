using System;
using UnityEngine;

public class ItemMaker : ItemAcceptorMono
{
	private class MakerSlot : ItemSlot
	{
		private readonly Action<Item> onNewItem;

		public MakerSlot(Action<Item> onNewItem)
			: base(null)
		{
			this.onNewItem = onNewItem;
		}

		public override bool CanAcceptItem(Item newItem)
		{
			return true;
		}

		public override bool AcceptItem(Item newItem)
		{
			base.item?.Destroy();
			base.AcceptItem(newItem);
			onNewItem(newItem);
			return true;
		}

		public override bool RemoveItem()
		{
			base.item = base.item.Copy();
			return true;
		}
	}

	private string captcha = string.Empty;

	private NormalItem baseItem;

	private Item.ItemType type;

	private bool[] tags = new bool[46];

	protected override void SetSlots()
	{
		base.ItemSlot.Set(new MakerSlot(OnNewItem));
	}

	private void OnNewItem(Item item)
	{
		baseItem = GetBaseItem(item);
		captcha = baseItem.captchaCode;
		tags = baseItem.GetTagArray();
		type = item.itemType;
	}

	private void OnGUI()
	{
		Vector2 vector = MSPAOrthoController.main.WorldToScreenPoint(renderer.bounds.center);
		vector.y = (float)Screen.height - vector.y;
		string text = GUI.TextArea(new Rect(vector.x - 50f, vector.y + 35f, 100f, 24f), captcha, 8);
		if (text != captcha)
		{
			captcha = text;
			if (captcha.Length == 8)
			{
				baseItem = new NormalItem(text);
				SetItem();
			}
		}
		if (baseItem == null)
		{
			return;
		}
		if (GUI.Button(new Rect(vector.x - 147f, vector.y + 62f, 94f, 24f), "Normal"))
		{
			type = Item.ItemType.Normal;
			SetItem();
		}
		if (GUI.Button(new Rect(vector.x - 47f, vector.y + 62f, 94f, 24f), "Totem"))
		{
			type = Item.ItemType.Totem;
			SetItem();
		}
		if (GUI.Button(new Rect(vector.x + 53f, vector.y + 62f, 94f, 24f), "Punched"))
		{
			type = Item.ItemType.Punched;
			SetItem();
		}
		int num = Mathf.RoundToInt(vector.y) + 89;
		bool flag = false;
		int num2 = 0;
		Color contentColor = GUI.contentColor;
		GUI.contentColor = Color.black;
		for (int i = 0; i < 46; i++)
		{
			Rect position = new Rect(vector.x - 150f + (float)(100 * num2), num, 100f, 24f);
			bool value = tags[i];
			NormalItem.Tag tag = (NormalItem.Tag)i;
			bool flag2 = GUI.Toggle(position, value, tag.ToString());
			if (tags[i] != flag2)
			{
				flag = true;
				tags[i] = flag2;
			}
			num2++;
			if (num2 == 3)
			{
				num += 27;
				num2 = 0;
			}
		}
		GUI.contentColor = contentColor;
		if (flag)
		{
			baseItem.SetTagArray(tags);
		}
	}

	private void SetItem()
	{
		ItemSlot itemSlot = base.ItemSlot[0];
		itemSlot.AcceptItem(type switch
		{
			Item.ItemType.Totem => new Totem(baseItem, GetComponentInParent<House>().cruxiteColor), 
			Item.ItemType.Punched => new PunchCard(baseItem), 
			_ => baseItem, 
		});
	}

	private static NormalItem GetBaseItem(Item item)
	{
		if (!(item is Totem totem))
		{
			if (!(item is PunchCard punchCard))
			{
				if (item is NormalItem result)
				{
					return result;
				}
				throw new InvalidOperationException();
			}
			return punchCard.GetItem();
		}
		return totem.makeItem;
	}

	public override bool AcceptItem(Item item)
	{
		baseItem = GetBaseItem(item);
		SetItem();
		Open();
		return true;
	}
}
