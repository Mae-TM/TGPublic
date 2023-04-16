using System;
using UnityEngine.UI;

public class ItemSlot
{
	private Item _item;

	public bool needsVisualUpdate;

	public Action OnItemChanged;

	public Item item
	{
		get
		{
			return _item;
		}
		protected set
		{
			_item = value;
			OnItemChanged?.Invoke();
			needsVisualUpdate = true;
		}
	}

	public ItemSlot(Item it)
	{
		_item = it;
	}

	public virtual void SetItemDirect(Item to)
	{
		_item = to;
		needsVisualUpdate = true;
	}

	public virtual bool CanAcceptItem(Item newItem)
	{
		return item == null;
	}

	public virtual bool AcceptItem(Item newItem)
	{
		if (CanAcceptItem(newItem))
		{
			item = newItem;
			return true;
		}
		return false;
	}

	public virtual bool CanRemoveItem()
	{
		return item != null;
	}

	public virtual bool RemoveItem()
	{
		if (CanRemoveItem())
		{
			item = null;
			return true;
		}
		return false;
	}

	public virtual string VisualUpdate(Image image)
	{
		needsVisualUpdate = false;
		if (item == null)
		{
			Item.ClearImage(image);
			image.enabled = false;
			return null;
		}
		item.ApplyToImage(image);
		image.enabled = true;
		return item.GetItemName();
	}
}
