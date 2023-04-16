using UnityEngine;
using UnityEngine.UI;

public class PC : ItemAcceptorMono
{
	private class CDSlot : ItemSlot
	{
		private static Sprite limitSprite;

		private readonly PC parent;

		public CDSlot(PC nparent)
			: base(null)
		{
			parent = nparent;
			if (limitSprite == null)
			{
				limitSprite = ItemDownloader.GetSprite("CD");
			}
		}

		private void ActivateSburb()
		{
			parent.screen.gameObject.SetActive(value: true);
			parent.screen.material.mainTexture = parent.texture;
			parent.screen.transform.parent.GetComponent<Interactable>().AddOption<BuildModeAction>();
		}

		public override bool AcceptItem(Item newItemBase)
		{
			if (!(newItemBase is NormalItem normalItem) || normalItem.itemType != 0)
			{
				return false;
			}
			if (normalItem.captchaCode == "CD000000" && base.AcceptItem(normalItem))
			{
				Exile.StopAction(Exile.Action.disc);
				ActivateSburb();
				return true;
			}
			if (normalItem.captchaCode == "SW4PP3D!" && base.AcceptItem(normalItem))
			{
				parent.screen.gameObject.SetActive(value: true);
				parent.screen.material.mainTexture = parent.hstexture;
				return true;
			}
			return false;
		}

		public override void SetItemDirect(Item to)
		{
			if (base.item != null)
			{
				if (((NormalItem)base.item).captchaCode == "CD000000")
				{
					parent.screen.transform.parent.GetComponent<Interactable>().RemoveOption(parent.screen.transform.parent.gameObject.GetComponent<BuildModeAction>());
				}
				parent.screen.gameObject.SetActive(value: false);
			}
			if (to != null)
			{
				if (((NormalItem)to).captchaCode == "CD000000")
				{
					ActivateSburb();
				}
				if (((NormalItem)to).captchaCode == "SW4PP3D!")
				{
					parent.screen.material.mainTexture = parent.hstexture;
					parent.screen.gameObject.SetActive(value: true);
				}
			}
			base.SetItemDirect(to);
		}

		public override bool CanRemoveItem()
		{
			Furniture componentInParent = parent.GetComponentInParent<Furniture>();
			if (!(componentInParent == null))
			{
				return componentInParent.AllowMovement;
			}
			return true;
		}

		public override bool RemoveItem()
		{
			NormalItem normalItem = (NormalItem)base.item;
			if (base.RemoveItem())
			{
				parent.screen.gameObject.SetActive(value: false);
				if (normalItem.captchaCode == "CD000000")
				{
					parent.screen.transform.parent.GetComponent<Interactable>().RemoveOption(parent.screen.transform.parent.gameObject.GetComponent<BuildModeAction>());
				}
				return true;
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
				image.material = image.transform.parent.GetComponent<Image>().material;
				return "CDs only";
			}
			base.item.ApplyToImage(image);
			image.color = Color.white;
			image.material = image.transform.parent.GetComponent<Image>().defaultMaterial;
			return base.item.GetItemName();
		}
	}

	public MeshRenderer screen;

	public Texture texture;

	public Texture hstexture;

	protected override void SetSlots()
	{
		base.ItemSlot.Set(new CDSlot(this));
	}
}
