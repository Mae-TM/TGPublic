using System;
using UnityEngine;
using UnityEngine.UI;

public class PunchDesignix : ItemAcceptorMono
{
	private class DesignixSlot : ItemSlot
	{
		private readonly Action updateUI;

		private readonly SpriteRenderer sprite;

		public DesignixSlot(Action updateUI, Item it, SpriteRenderer sprite)
			: base(it)
		{
			this.updateUI = updateUI;
			this.sprite = sprite;
		}

		public override bool CanAcceptItem(Item newItem)
		{
			if (base.item == null && !newItem.IsEntry)
			{
				if (!(newItem is NormalItem))
				{
					return newItem is PunchCard;
				}
				return true;
			}
			return false;
		}

		public override bool AcceptItem(Item newItem)
		{
			if (base.AcceptItem(newItem))
			{
				base.item.ApplyToCard(sprite);
				sprite.transform.parent.gameObject.SetActive(value: true);
				return true;
			}
			return false;
		}

		public override bool RemoveItem()
		{
			base.item.ClearCard(sprite);
			sprite.transform.parent.gameObject.SetActive(value: false);
			return base.RemoveItem();
		}

		public override string VisualUpdate(Image image)
		{
			updateUI();
			return base.VisualUpdate(image);
		}
	}

	[SerializeField]
	private SpriteRenderer cardLeft;

	[SerializeField]
	private SpriteRenderer cardRight;

	[SerializeField]
	private RectTransform punchUI;

	[SerializeField]
	private AudioSource audio;

	private Text arrowText;

	private Transform arrow;

	private Transform selfpunch;

	protected override void SetSlots()
	{
		base.ItemSlot.Set(new DesignixSlot(UpdateUI, null, cardLeft), new DesignixSlot(UpdateUI, null, cardRight));
	}

	private void Punch(Item item)
	{
		if (item != null && base.ItemSlot[0].item != null)
		{
			audio.Play();
			if (base.ItemSlot[0].item is PunchCard punchCard)
			{
				punchCard.OR(item);
				return;
			}
			NormalItem normalItem = (NormalItem)base.ItemSlot[0].item;
			base.ItemSlot[0].RemoveItem();
			base.ItemSlot[0].AcceptItem(new PunchCard(item.Copy(), normalItem));
			normalItem.Destroy();
		}
		else
		{
			SoundEffects.Instance.Nope();
		}
	}

	protected override void ApplyToUI()
	{
		RectTransform rectTransform = UnityEngine.Object.Instantiate(punchUI, ItemAcceptorMono.invUI);
		rectTransform.GetChild(0).GetComponent<VisualItemSlot>().SetSlot(base.ItemSlot[0]);
		rectTransform.GetChild(1).GetComponent<VisualItemSlot>().SetSlot(base.ItemSlot[1]);
		selfpunch = rectTransform.GetChild(2);
		selfpunch.GetComponent<Button>().onClick.AddListener(delegate
		{
			Punch(base.ItemSlot[0].item);
		});
		arrow = rectTransform.GetChild(3);
		arrow.GetComponent<Button>().onClick.AddListener(delegate
		{
			Punch(base.ItemSlot[1].item);
		});
		arrowText = arrow.GetChild(0).GetComponent<Text>();
		UpdateUI();
	}

	private void UpdateUI()
	{
		Item item = base.ItemSlot[0].item;
		Item item2 = base.ItemSlot[1].item;
		arrow.gameObject.SetActive(item != null && item2 != null && (!(item is PunchCard punchCard) || !punchCard.IsUnalchemisable(item2)));
		selfpunch.gameObject.SetActive(item != null && !(item is PunchCard));
		arrowText.text = ((item is PunchCard) ? "||" : "Punch");
	}
}
