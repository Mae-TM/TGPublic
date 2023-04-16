using UnityEngine;

public class RemoveItemAction : SyncedInteractableAction
{
	public string captchaCode;

	public string newFurniture;

	private Item item;

	private void Start()
	{
		item = captchaCode;
		sprite = item.sprite;
		desc = "Remove " + item.GetItemName();
	}

	protected override bool LocalExecute()
	{
		return Player.player.sylladex.AddItem(item);
	}

	protected override void ServerExecute(Player player)
	{
		Furniture.Make(newFurniture, GetComponent<Furniture>());
		Object.Destroy(base.gameObject);
	}
}
