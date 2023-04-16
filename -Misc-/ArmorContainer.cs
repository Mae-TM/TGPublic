using System;
using System.Collections.Generic;
using System.Linq;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.UI;

public class ArmorContainer : ItemAcceptorMono
{
	public class Slot : ItemSlot
	{
		private readonly ArmorKind limit;

		private readonly Sprite limitSprite;

		public Slot(Item it, ArmorKind limit, Sprite limitSprite)
			: base(it)
		{
			this.limit = limit;
			this.limitSprite = limitSprite;
		}

		public override bool CanAcceptItem(Item newItemBase)
		{
			if (base.item == null && newItemBase is NormalItem normalItem)
			{
				if (limit != ArmorKind.None)
				{
					return normalItem.armor == limit;
				}
				return normalItem.armor != ArmorKind.None;
			}
			return false;
		}

		public override string VisualUpdate(Image image)
		{
			needsVisualUpdate = false;
			if (base.item == null)
			{
				Item.ClearImage(image);
				image.sprite = limitSprite;
				image.color = new Color(1f, 1f, 1f, 0.3f);
				if (limit == ArmorKind.None)
				{
					return "Armor only";
				}
				return limit.ToString() + " only";
			}
			base.item.ApplyToImage(image);
			image.color = Color.white;
			return base.item.GetItemName();
		}
	}

	public string[] item;

	public ArmorKind limit;

	protected override void SetSlots()
	{
		if (item != null)
		{
			Sprite limitSprite = ItemDownloader.GetArmorKind(limit);
			base.ItemSlot.Set(((IEnumerable<LDBItem>)ItemDownloader.Instance.GetItems(item)).Select((Func<LDBItem, ItemSlot>)((LDBItem it) => new Slot((NormalItem)it, limit, limitSprite))));
			item = null;
		}
	}
}
