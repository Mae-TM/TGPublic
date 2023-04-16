using System.Text;
using Quest.NET.Interfaces;

public class ItemReward : IReward
{
	private readonly Item[] items;

	public ItemReward(Item[] items)
	{
		this.items = items;
	}

	public override void GrantReward()
	{
		base.GrantReward();
		Item[] array = items;
		foreach (Item item in array)
		{
			Player.player.sylladex.AddItem(item);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Item[] array = items;
		foreach (Item item in array)
		{
			stringBuilder.Append(item.GetItemName());
		}
		return stringBuilder.ToString();
	}
}
