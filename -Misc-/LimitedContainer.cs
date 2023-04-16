using System;
using System.Collections.Generic;
using System.Linq;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.UI;

public class LimitedContainer : ItemAcceptorMono
{
	private class LimitedSlot : ItemSlot
	{
		private readonly WeaponKind limit;

		private readonly Sprite limitSprite;

		public LimitedSlot(Item it, WeaponKind limit, Sprite limitSprite)
			: base(it)
		{
			this.limit = limit;
			this.limitSprite = limitSprite;
		}

		public override bool CanAcceptItem(Item newItemBase)
		{
			if (base.item == null && newItemBase is NormalItem normalItem)
			{
				return Array.IndexOf(normalItem.weaponKind, limit) != -1;
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
				return limit.ToString() + "Kind only";
			}
			base.item.ApplyToImage(image);
			image.color = Color.white;
			return base.item.GetItemName();
		}
	}

	public string[] item;

	public WeaponKind limit;

	protected override void SetSlots()
	{
		if (item != null)
		{
			Sprite limitSprite = ItemDownloader.GetWeaponKind(limit);
			base.ItemSlot.Set(((IEnumerable<LDBItem>)ItemDownloader.Instance.GetItems(item)).Select((Func<LDBItem, ItemSlot>)((LDBItem it) => new LimitedSlot((NormalItem)it, limit, limitSprite))));
			item = null;
		}
	}
}
