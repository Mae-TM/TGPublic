using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class EnterObjective : QuestObjective
{
	public EnterObjective(string title, string description, bool isBonus)
		: base(title, description, isBonus)
	{
	}

	public override ObjectiveStatus CheckProgress()
	{
		if (Player.player.HasEntered())
		{
			Status = ObjectiveStatus.Completed;
		}
		return Status;
	}
}
