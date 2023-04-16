using UnityEngine;

public class PickupItemAction : InteractableAction
{
	public string captchaCode;

	public Item targetItem;

	public bool objectIsItem = true;

	public bool consume = true;

	public ItemSlot notify;

	protected override Material Material => targetItem.GetMaterial();

	protected override Color Color => targetItem.GetColor();

	public void Start()
	{
		if (targetItem == null && !string.IsNullOrEmpty(captchaCode))
		{
			targetItem = captchaCode;
		}
		if (targetItem != null)
		{
			sprite = targetItem.sprite;
			desc = targetItem.GetItemName();
		}
		else
		{
			Debug.LogError("Target item not set for PickupItemAction!");
		}
	}

	public override void Execute()
	{
		if (Player.player.sylladex != null)
		{
			if (notify != null && !notify.Equals(null) && notify.item == targetItem)
			{
				if (!notify.RemoveItem())
				{
					return;
				}
				notify = null;
			}
			if (!Player.player.sylladex.AddItem(targetItem) || Equals(null))
			{
				return;
			}
			if (objectIsItem)
			{
				targetItem.ItemObject.CmdPickUp();
			}
			else if (consume)
			{
				Prototype component = GetComponent<Prototype>();
				if (component != null && targetItem.SceneObject.GetComponent<Prototype>() == null)
				{
					targetItem.SceneObject.AddComponent<Prototype>().protoName = component.protoName;
				}
				Object.Destroy(base.gameObject);
			}
			else
			{
				targetItem = targetItem.Copy();
			}
		}
		else
		{
			MonoBehaviour.print("No target sylladex set!");
		}
	}

	public void DragItem()
	{
		if (Player.player.sylladex.GetDragItem() != null)
		{
			return;
		}
		if (notify != null && !notify.Equals(null) && notify.item == targetItem)
		{
			if (!notify.RemoveItem())
			{
				return;
			}
			notify = null;
		}
		Player.player.sylladex.SetDragItem(targetItem);
		if (objectIsItem)
		{
			targetItem.ItemObject.CmdPickUp();
		}
		else if (consume)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			targetItem = targetItem.Copy();
		}
	}
}
