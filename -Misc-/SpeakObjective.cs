using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class SpeakObjective : QuestObjective
{
	public SpeakObjective(bool isBonus, string title = null, string description = null)
		: base(title ?? "Report back", description, isBonus)
	{
	}

	public override void Invoke(object arg)
	{
		if (arg is SpeakAction speakAction)
		{
			speakAction.AddListener(OnSpeak);
		}
	}

	private void OnSpeak()
	{
		Status = ObjectiveStatus.Completed;
	}
}
