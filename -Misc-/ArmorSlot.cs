using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

internal class ArmorSlot : VisualItemSlot
{
	[SerializeField]
	private Sylladex sylladex;

	[SerializeField]
	private Text text;

	[SerializeField]
	private Text text2;

	[SerializeField]
	private ArmorKind kind = ArmorKind.None;

	[SerializeField]
	private UnityEvent onChange;

	public NormalItem item
	{
		get
		{
			return (NormalItem)slot.item;
		}
		set
		{
			ItemChanged(item, value);
			slot.SetItemDirect(value);
		}
	}

	public void Init(NormalItem item)
	{
		Sprite armorKind = ItemDownloader.GetArmorKind(kind);
		slot = new ArmorContainer.Slot(item, kind, armorKind);
		slot.needsVisualUpdate = true;
		if (item != null)
		{
			text.text = Sylladex.MetricFormat(item.Power);
		}
		text.transform.parent.gameObject.SetActive(value: true);
		if (text2 != null && item != null)
		{
			text2.text = Sylladex.MetricFormat(item.Speed);
			text2.transform.parent.gameObject.SetActive(value: true);
		}
		Player.player.SetArmor((int)kind, item);
	}

	public override bool AcceptItem(Item newItem)
	{
		NormalItem from = item;
		if (base.AcceptItem(newItem))
		{
			ItemChanged(from, (NormalItem)newItem);
			sylladex.PlaySoundEffect(sylladex.strifeSpecibus.clipEquip);
			return true;
		}
		return false;
	}

	protected override bool RemoveItem()
	{
		ItemChanged(item, null);
		return base.RemoveItem();
	}

	protected void ItemChanged(NormalItem from, NormalItem to)
	{
		if (from != null)
		{
			text.transform.parent.gameObject.SetActive(value: false);
			if (text2 != null)
			{
				text2.transform.parent.gameObject.SetActive(value: false);
			}
		}
		if (to != null)
		{
			text.text = Sylladex.MetricFormat(to.Power);
			text.transform.parent.gameObject.SetActive(value: true);
			if (text2 != null)
			{
				text2.text = Sylladex.MetricFormat(to.Speed);
				text2.transform.parent.gameObject.SetActive(value: true);
			}
		}
		Player.player.CmdSetArmor(kind, to);
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(InvokeOnChange());
		}
	}

	private IEnumerator InvokeOnChange()
	{
		yield return null;
		onChange.Invoke();
	}
}
