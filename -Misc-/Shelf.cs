using Mirror;
using UnityEngine;

public class Shelf : ItemAcceptorMono
{
	public class ShelfSlot : ItemSlot
	{
		private readonly Transform worldParent;

		public ShelfSlot(Transform worldParent)
			: base(null)
		{
			this.worldParent = worldParent;
		}

		public override bool AcceptItem(Item newItem)
		{
			if (base.AcceptItem(newItem))
			{
				SetSceneObject(newItem.SceneObject);
				return true;
			}
			return false;
		}

		public override bool RemoveItem()
		{
			RemoveSceneObject();
			return base.RemoveItem();
		}

		public override void SetItemDirect(Item to)
		{
			if (base.item != null)
			{
				RemoveSceneObject();
			}
			if (to != null)
			{
				SetSceneObject(to.SceneObject);
			}
			base.SetItemDirect(to);
		}

		protected void SetSceneObject(GameObject obj)
		{
			obj.GetComponent<PickupItemAction>().notify = this;
			obj.GetComponent<Rigidbody>().isKinematic = true;
			obj.GetComponent<RegionChild>().enabled = false;
			obj.GetComponent<NetworkTransform>().enabled = false;
			Visibility.SetParent(obj.transform, worldParent);
			obj.SetActive(value: true);
			SetLocalOrientation(obj);
		}

		protected virtual void SetLocalOrientation(GameObject obj)
		{
			obj.transform.position = Vector3.zero;
			obj.transform.rotation = Quaternion.identity;
			obj.transform.localPosition = -ModelUtility.GetBottom(obj);
		}

		protected void RemoveSceneObject()
		{
			if (base.item != null && base.item.SceneObject != null)
			{
				base.item.SceneObject.SetActive(value: false);
				base.item.SceneObject.GetComponent<PickupItemAction>().notify = null;
				base.item.SceneObject.GetComponent<RegionChild>().enabled = true;
				base.item.SceneObject.GetComponent<Rigidbody>().isKinematic = false;
				base.item.SceneObject.GetComponent<NetworkTransform>().enabled = true;
			}
		}
	}

	public Transform position;

	protected override void SetSlots()
	{
		base.ItemSlot.Set(new ShelfSlot(position));
	}
}
