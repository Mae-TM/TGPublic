using UnityEngine;
using UnityEngine.UI;

public class Poster : ItemAcceptorMono
{
	private class PosterSlot : ItemSlot
	{
		private readonly Poster poster;

		public PosterSlot(Poster poster)
			: base(null)
		{
			this.poster = poster;
		}

		public override bool CanAcceptItem(Item newItem)
		{
			return newItem.IsPrefab("Rolled Poster");
		}

		public override bool AcceptItem(Item newItem)
		{
			if (!base.AcceptItem(newItem))
			{
				return false;
			}
			SetSprite();
			return true;
		}

		public override bool RemoveItem()
		{
			RemoveSprite();
			return base.RemoveItem();
		}

		public override void SetItemDirect(Item to)
		{
			if (base.item != null)
			{
				RemoveSprite();
			}
			base.SetItemDirect(to);
			if (base.item != null)
			{
				SetSprite();
			}
		}

		private void SetSprite()
		{
			poster.nail.SetActive(value: false);
			poster.sprite.material.mainTexture = base.item.sprite.texture;
			poster.sprite.enabled = true;
			Bounds bounds = base.item.sprite.bounds;
			Transform transform = poster.sprite.transform;
			transform.localPosition = new Vector3(0f, 0f - bounds.extents.y, (0f - Random.value) / 128f) - bounds.center;
			transform.localScale = bounds.size;
			poster.trigger.enabled = true;
		}

		private void RemoveSprite()
		{
			poster.sprite.enabled = false;
			poster.trigger.enabled = false;
			poster.nail.SetActive(value: true);
		}

		public override string VisualUpdate(Image image)
		{
			return base.VisualUpdate(image) ?? "Posters only";
		}
	}

	[SerializeField]
	private MeshRenderer sprite;

	[SerializeField]
	private Collider trigger;

	[SerializeField]
	private GameObject nail;

	protected override void SetSlots()
	{
		base.ItemSlot.Set(new PosterSlot(this));
	}
}
