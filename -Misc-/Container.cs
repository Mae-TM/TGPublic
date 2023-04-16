using System.Linq;
using TheGenesisLib.Models;

public class Container : ItemAcceptorMono
{
	public string[] item;

	protected override void SetSlots()
	{
		if (item != null)
		{
			base.ItemSlot.Set(from it in ItemDownloader.Instance.GetItems(item)
				select new ItemSlot((NormalItem)it));
			item = null;
		}
	}
}
