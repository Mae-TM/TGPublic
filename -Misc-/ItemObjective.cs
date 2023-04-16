using System;
using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class ItemObjective : QuestObjective
{
	private readonly Item item;

	private readonly int requiredCount;

	private int lastCount;

	public ItemObjective(bool isBonus, Item item, int requiredCount = 1, string title = null, string description = null)
		: base(title ?? ((requiredCount <= 1) ? ("Collect 1 " + item.GetItemName()) : $"Collect {requiredCount} {item.GetItemName()}s"), description, isBonus)
	{
		this.item = item;
		this.requiredCount = Math.Max(1, requiredCount);
	}

	public override ObjectiveStatus CheckProgress()
	{
		int num = Player.player.sylladex.CountItem(item);
		if (lastCount != num)
		{
			lastCount = num;
			if (num >= requiredCount)
			{
				Status = ObjectiveStatus.Completed;
			}
			else
			{
				Status = ObjectiveStatus.Updated;
			}
		}
		return Status;
	}
}
