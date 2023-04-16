using Quest.NET.Enums;

namespace Quest.NET.Interfaces;

public class QuestObjective : IQuestObjective
{
	public virtual string Title { get; }

	public virtual string Description { get; }

	public virtual ObjectiveStatus Status { get; protected set; }

	public virtual bool IsBonus { get; }

	public virtual ObjectiveStatus CheckProgress()
	{
		return Status;
	}

	public QuestObjective(string title, string description, bool isBonus)
	{
		Title = title;
		Description = description;
		Status = ObjectiveStatus.InProgress;
		IsBonus = isBonus;
	}

	public virtual void Invoke(object arg)
	{
	}

	public byte[] Save()
	{
		QuestObjectiveData obj = default(QuestObjectiveData);
		obj.status = (byte)Status;
		return ProtobufHelpers.ProtoSerialize(obj);
	}

	public void Load(byte[] data)
	{
		Status = (ObjectiveStatus)ProtobufHelpers.ProtoDeserialize<QuestObjectiveData>(data).status;
	}
}
