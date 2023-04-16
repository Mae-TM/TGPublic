using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class BuildModeObjective : QuestObjective
{
	public BuildModeObjective(string title, string description, bool isBonus)
		: base(title, description, isBonus)
	{
	}

	public override ObjectiveStatus CheckProgress()
	{
		if (BuildExploreSwitcher.Instance.IsInBuildMode)
		{
			Status = ObjectiveStatus.Completed;
		}
		return Status;
	}
}
