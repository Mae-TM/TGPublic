using Quest.NET.Enums;

namespace Quest.NET.Interfaces;

public interface IQuestObjective
{
	string Title { get; }

	string Description { get; }

	ObjectiveStatus Status { get; }

	bool IsBonus { get; }

	ObjectiveStatus CheckProgress();

	byte[] Save();

	void Load(byte[] data);

	void Invoke(object arg);
}
