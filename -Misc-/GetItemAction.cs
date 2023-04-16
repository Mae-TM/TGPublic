using UnityEngine;

public class GetItemAction : InteractableAction
{
	public string captchaCode;

	public Item targetItem;

	public int itemsLeft = -1;

	public void Start()
	{
		if (targetItem != null)
		{
			sprite = targetItem.sprite;
			desc = targetItem.GetItemName();
		}
		else if (captchaCode != null)
		{
			targetItem = captchaCode;
			sprite = targetItem.sprite;
			desc = targetItem.GetItemName();
		}
	}

	public override void Execute()
	{
		if ((bool)Player.player.sylladex)
		{
			Item item = ((itemsLeft != 1) ? targetItem.Copy() : targetItem);
			if (!Player.player.sylladex.AddItem(item) || itemsLeft <= 0)
			{
				return;
			}
			itemsLeft--;
			if (itemsLeft == 0)
			{
				Object.Destroy(this);
				Interactable component = base.gameObject.GetComponent<Interactable>();
				if ((bool)component)
				{
					component.RemoveOption(this);
				}
				else
				{
					MonoBehaviour.print("Couldn't find interactable");
				}
			}
		}
		else
		{
			MonoBehaviour.print("No target sylladex set!");
		}
	}
}
