using System;
using Quest.NET.Interfaces;

namespace Quest.NET;

public class QuestIdentifier : IQuestIdentifier
{
	private Guid _sourceId;

	private string _questId;

	private Guid _chainQuestId;

	public Guid SourceID => _sourceId;

	public string QuestID => _questId;

	public Guid ChainQuestID => _chainQuestId;

	public QuestIdentifier(string questId)
	{
		_questId = questId;
	}
}
