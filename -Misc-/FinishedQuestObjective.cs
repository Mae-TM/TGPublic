using Quest.NET;
using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class FinishedQuestObjective : QuestObjective
{
	public global::Quest.NET.Quest Quest { get; }

	public FinishedQuestObjective(string title, string description, bool isBonus, global::Quest.NET.Quest quest)
		: base(title, description, isBonus)
	{
		Quest = quest;
	}

	public override ObjectiveStatus CheckProgress()
	{
		QuestStatus status = Quest.Status;
		ObjectiveStatus objectiveStatus;
		switch (Quest.CheckCompletion())
		{
		case QuestStatus.Failed:
			objectiveStatus = ObjectiveStatus.Failed;
			break;
		case QuestStatus.Completed:
			objectiveStatus = ObjectiveStatus.Completed;
			break;
		case QuestStatus.InProgress:
			if (status == QuestStatus.Updated)
			{
				goto IL_0045;
			}
			goto default;
		case QuestStatus.Updated:
			if (status == QuestStatus.InProgress || status == QuestStatus.Updated)
			{
				goto IL_0045;
			}
			goto default;
		default:
			{
				objectiveStatus = ObjectiveStatus.InProgress;
				break;
			}
			IL_0045:
			objectiveStatus = ObjectiveStatus.Updated;
			break;
		}
		Status = objectiveStatus;
		return objectiveStatus;
	}
}
