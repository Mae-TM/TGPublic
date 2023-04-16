using System;

namespace Quest.NET.Interfaces;

public interface IQuestIdentifier
{
	Guid SourceID { get; }

	string QuestID { get; }

	Guid ChainQuestID { get; }
}
