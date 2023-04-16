public class Bin : ItemAcceptorMono
{
	protected override void SetSlots()
	{
		base.ItemSlot.Set();
	}

	public override bool AcceptItem(Item item)
	{
		if (item.IsEntry)
		{
			return false;
		}
		item.Destroy();
		return true;
	}

	protected override bool CanOpen()
	{
		return false;
	}

	protected override void Opened()
	{
		Closed();
		ClickOpen.active = null;
	}
}
