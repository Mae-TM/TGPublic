using System.Collections;
using System.Collections.Generic;
using Quest.NET;
using Quest.NET.Enums;
using Quest.NET.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class QuestList : MonoBehaviour
{
	private readonly Dictionary<string, Quest.NET.Quest> quests = new Dictionary<string, Quest.NET.Quest>();

	private readonly Dictionary<Quest.NET.Quest, Button> questButtons = new Dictionary<Quest.NET.Quest, Button>();

	[SerializeField]
	private Button item;

	[SerializeField]
	private Text title;

	[SerializeField]
	private Text description;

	[SerializeField]
	private RectTransform objectivePrefab;

	[SerializeField]
	private RectTransform rewardPrefab;

	[SerializeField]
	private Animator notification;

	[SerializeField]
	private GameObject[] buttons;

	private Quest.NET.Quest selectedQuest;

	private QuestParser questParser;

	public IEnumerator UpdateQuests()
	{
		IEnumerator wait = new WaitForSecondsRealtime(0.5f);
		while (true)
		{
			yield return wait;
			RefreshQuests();
		}
	}

	public void RefreshQuests()
	{
		foreach (KeyValuePair<string, Quest.NET.Quest> quest in quests)
		{
			Quest.NET.Quest value = quest.Value;
			IEnumerable<string> enumerable = value.UpdateQuest();
			if (enumerable != null)
			{
				foreach (string item in enumerable)
				{
					AddQuest(item);
				}
			}
			if (value.Status != QuestStatus.Completed)
			{
				continue;
			}
			Object.Destroy(questButtons[value].gameObject);
			questButtons.Remove(value);
			quests.Remove(quest.Key);
			if (quests.Count == 0)
			{
				GameObject[] array = buttons;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
			}
			break;
		}
	}

	public void AddStartQuest()
	{
		Exile.SetAction(Exile.Action.start);
		AddQuest("Guardian");
	}

	public void AddEntryQuest()
	{
		if (Exile.SetAction(Exile.Action.furniture))
		{
			AddQuest("OpenCruxtruder");
		}
	}

	public bool HasQuest(string quest)
	{
		return quests.ContainsKey(quest);
	}

	public void InvokeQuest(string questName, object var)
	{
		if (quests.TryGetValue(questName, out var value))
		{
			value.Invoke(var);
		}
	}

	public bool AddQuest(string questName)
	{
		if (HasQuest(questName))
		{
			Debug.LogWarning("Attempt to add quest '" + questName + "' a second time.");
			return false;
		}
		if (questParser == null)
		{
			questParser = new QuestParser();
		}
		Quest.NET.Quest quest = questParser.Parse(questName);
		if (quest == null)
		{
			return false;
		}
		AddQuest(quest);
		GameObject[] array = buttons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		notification.Play("NotEnoughGrist");
		return true;
	}

	private void AddQuest(Quest.NET.Quest quest)
	{
		quests.Add(quest.Identifier.QuestID, quest);
		Button button = Object.Instantiate(item, item.transform.parent);
		button.GetComponentInChildren<Text>().text = quest.Text.Name;
		button.name = "Quest " + quest.Identifier;
		button.onClick.AddListener(delegate
		{
			FocusQuest(quest);
		});
		button.gameObject.SetActive(value: true);
		questButtons.Add(quest, button);
	}

	private void FocusQuest(Quest.NET.Quest quest)
	{
		selectedQuest = quest;
		title.text = quest.Text.Name;
		title.gameObject.SetActive(value: true);
		string descriptionSummary = quest.Text.DescriptionSummary;
		if (descriptionSummary != null)
		{
			description.text = descriptionSummary;
			description.gameObject.SetActive(value: true);
		}
		else
		{
			description.gameObject.SetActive(value: false);
		}
		foreach (Transform item in objectivePrefab.parent)
		{
			if (item != objectivePrefab)
			{
				Object.Destroy(item.gameObject);
			}
		}
		foreach (IQuestObjective objective in quest.Objectives)
		{
			RectTransform rectTransform = Object.Instantiate(objectivePrefab, objectivePrefab.parent);
			if (objective.Status == ObjectiveStatus.Completed)
			{
				rectTransform.GetChild(0).GetChild(0).gameObject.SetActive(value: true);
			}
			else if (objective.Status == ObjectiveStatus.Failed)
			{
				rectTransform.GetChild(0).GetChild(1).gameObject.SetActive(value: true);
			}
			rectTransform.GetChild(1).GetComponent<Text>().text = objective.Title;
			rectTransform.gameObject.SetActive(value: true);
		}
		foreach (Transform item2 in rewardPrefab.parent)
		{
			if (item2 != rewardPrefab)
			{
				Object.Destroy(item2.gameObject);
			}
		}
		foreach (IReward reward in quest.Rewards)
		{
			RectTransform rectTransform2 = Object.Instantiate(rewardPrefab, rewardPrefab.parent);
			rectTransform2.GetComponent<Text>().text = reward.ToString();
			rectTransform2.gameObject.SetActive(value: true);
		}
		foreach (Button value in questButtons.Values)
		{
			value.interactable = true;
		}
		questButtons[quest].interactable = false;
	}

	private void OnEnable()
	{
		if (selectedQuest != null && selectedQuest.Status != QuestStatus.Completed)
		{
			return;
		}
		Dictionary<string, Quest.NET.Quest>.Enumerator enumerator = quests.GetEnumerator();
		if (enumerator.MoveNext())
		{
			FocusQuest(enumerator.Current.Value);
		}
		else
		{
			title.text = "Out of quests!";
			description.gameObject.SetActive(value: false);
			foreach (Transform item in objectivePrefab.parent)
			{
				if (item != objectivePrefab)
				{
					Object.Destroy(item.gameObject);
				}
			}
			foreach (Transform item2 in rewardPrefab.parent)
			{
				if (item2 != rewardPrefab)
				{
					Object.Destroy(item2.gameObject);
				}
			}
			selectedQuest = null;
		}
		enumerator.Dispose();
	}

	public QuestData[] Save()
	{
		QuestData[] array = new QuestData[quests.Count];
		int num = 0;
		foreach (KeyValuePair<string, Quest.NET.Quest> quest in quests)
		{
			array[num] = quest.Value.Save();
			num++;
		}
		return array;
	}

	public void Load(QuestData[] data)
	{
		if (questParser == null)
		{
			questParser = new QuestParser();
		}
		if (data == null)
		{
			return;
		}
		int num = data.Length;
		if (num != 0)
		{
			for (int i = 0; i < num; i++)
			{
				Quest.NET.Quest quest = questParser.Parse(data[i].questId);
				quest.Load(data[i]);
				AddQuest(quest);
			}
			GameObject[] array = buttons;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: true);
			}
		}
	}
}
