using System.Collections.Generic;
using System.Linq;
using Quest.NET.Enums;
using Quest.NET.Interfaces;
using Quest.NET.Utils;

namespace Quest.NET;

public class Quest
{
	public delegate void OnCompletionHandler(Quest q);

	public delegate void OnFailedHandler(Quest q);

	public delegate void OnUpdateHandler(Quest q);

	public delegate void OnInvokeHandler(Quest q, object arg);

	private IQuestText _text;

	private IQuestIdentifier _identifier;

	private List<IQuestObjective> _objectives;

	private List<IReward> _rewards;

	private List<string> _next;

	private QuestStatus _status;

	private List<IQuestObjective> _specialObjectives;

	public IQuestText Text => _text;

	public IQuestIdentifier Identifier => _identifier;

	public List<IQuestObjective> Objectives => _objectives;

	public List<IReward> Rewards => _rewards;

	public QuestStatus Status => _status;

	public event OnCompletionHandler OnQuestCompletion;

	public event OnFailedHandler OnQuestFail;

	public event OnUpdateHandler OnQuestUpdate;

	public event OnInvokeHandler OnQuestInvoke;

	public Quest()
	{
		OnQuestCompletion += delegate
		{
		};
		OnQuestFail += delegate
		{
		};
		OnQuestUpdate += delegate
		{
		};
	}

	public Quest(IQuestText text, IQuestIdentifier identifier, List<IQuestObjective> objectives, List<IReward> rewards, List<string> next = null, List<IQuestObjective> specialObjectives = null)
		: this()
	{
		_text = text;
		_identifier = identifier;
		_objectives = objectives;
		_rewards = rewards;
		_next = next;
		_status = QuestStatus.InProgress;
		_specialObjectives = specialObjectives;
	}

	public QuestStatus CheckCompletion()
	{
		if (_objectives.Any((IQuestObjective o) => !o.IsBonus && o.Status == ObjectiveStatus.Failed))
		{
			return QuestStatus.Failed;
		}
		IEnumerable<ObjectiveStatus> second = _objectives.Select((IQuestObjective o) => o.Status);
		if (_specialObjectives != null && _specialObjectives.Select((IQuestObjective specialObjective) => specialObjective.CheckProgress()).Any((ObjectiveStatus progress) => progress == ObjectiveStatus.Completed))
		{
			return QuestStatus.Completed;
		}
		List<ObjectiveStatus> list = (from o in _objectives
			where o.Status != ObjectiveStatus.Completed && !o.IsBonus
			select o.CheckProgress()).ToList();
		if (list.All((ObjectiveStatus s) => s == ObjectiveStatus.Completed))
		{
			return QuestStatus.Completed;
		}
		if (list.Any((ObjectiveStatus s) => s == ObjectiveStatus.Updated) || list.HasChanged(second) != 0)
		{
			return QuestStatus.Updated;
		}
		return QuestStatus.InProgress;
	}

	public IEnumerable<string> UpdateQuest()
	{
		_status = CheckCompletion();
		switch (_status)
		{
		case QuestStatus.Completed:
			this.OnQuestCompletion(this);
			_rewards.GrantRewards();
			return _next;
		case QuestStatus.Failed:
			this.OnQuestFail(this);
			break;
		case QuestStatus.Updated:
			this.OnQuestUpdate(this);
			break;
		}
		return null;
	}

	public void Invoke(object arg)
	{
		this.OnQuestInvoke?.Invoke(this, arg);
		foreach (IQuestObjective objective in _objectives)
		{
			objective.Invoke(arg);
		}
	}

	public QuestData Save()
	{
		QuestData result = default(QuestData);
		result.questId = _identifier.QuestID;
		result.status = _status;
		result.objectives = _objectives.Select((IQuestObjective o) => o.Save()).ToList();
		return result;
	}

	public void Load(QuestData data)
	{
		_status = data.status;
		_identifier = new QuestIdentifier(data.questId);
		for (int i = 0; i < _objectives.Count; i++)
		{
			_objectives[i].Load(data.objectives[i]);
		}
	}
}
