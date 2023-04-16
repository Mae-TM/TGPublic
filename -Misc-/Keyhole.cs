using System.Collections.Generic;
using UnityEngine;

public class Keyhole : ItemAcceptorMono
{
	public enum KeyType
	{
		Exact,
		Tags
	}

	public class HiddenSlot : ItemSlot
	{
		public HiddenSlot(Item item)
			: base(null)
		{
		}
	}

	public Transform position;

	public static KeyType typeRequired;

	public NormalItem refItem;

	protected override void SetSlots()
	{
		base.ItemSlot.Set(new HiddenSlot(null), new Shelf.ShelfSlot(position));
	}

	public override bool AcceptItem(Item newItem)
	{
		if (base.AcceptItem(newItem))
		{
			if (base.ItemSlot[0] != null && base.ItemSlot[1] != null)
			{
				CheckValid(newItem);
			}
			return true;
		}
		return false;
	}

	protected void CheckValid(Item it)
	{
		if (it is NormalItem normalItem)
		{
			refItem = (NormalItem)base.ItemSlot[0].item;
			if (typeRequired == KeyType.Exact && normalItem.name == refItem.name)
			{
				Unlock();
			}
			if (typeRequired == KeyType.Tags && HasSameTags(refItem.GetTags(), normalItem.GetTags()))
			{
				Unlock();
			}
		}
	}

	protected bool HasSameTags(IEnumerable<NormalItem.Tag> refTags, IEnumerable<NormalItem.Tag> newTags)
	{
		foreach (NormalItem.Tag refTag in refTags)
		{
			bool flag = false;
			foreach (NormalItem.Tag newTag in newTags)
			{
				if (refTag == newTag)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	protected void Unlock()
	{
	}
}
