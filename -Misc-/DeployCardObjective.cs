using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class DeployCardObjective : QuestObjective
{
	public DeployCardObjective(string title, string description, bool isBonus)
		: base(title, description, isBonus)
	{
	}

	public override ObjectiveStatus CheckProgress()
	{
		House house = BuildExploreSwitcher.Instance.houseBuilder.House;
		if (house != null && house.HasPrepunchedCard)
		{
			Status = ObjectiveStatus.Completed;
		}
		return Status;
	}
}
