using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SpeakAction : InteractableAction
{
	private string file;

	private Material material;

	private Image questIcon;

	private readonly List<string> quests = new List<string>();

	private static QuestList questList;

	private bool isReceivingAcceptorEvent;

	private static QuestList QuestList
	{
		get
		{
			if (!questList)
			{
				questList = (Player.player ? Player.player.sylladex : UnityEngine.Object.FindObjectOfType<Sylladex>(includeInactive: true)).quests;
			}
			return questList;
		}
	}

	private event Action OnSpeak;

	private void Start()
	{
		desc = "Address";
		sprite = Resources.Load<Sprite>("Chummy");
	}

	public bool Set(string to, Material mat)
	{
		file = Dialogue.GetDialoguePath(to);
		if (file != null)
		{
			material = mat;
			return true;
		}
		GetComponent<Interactable>().RemoveOption(this);
		UnityEngine.Object.DestroyImmediate(this);
		return false;
	}

	public void AddListener(Action listener)
	{
		OnSpeak += listener;
		if ((bool)questIcon)
		{
			questIcon.enabled = true;
			return;
		}
		IOverhead componentInChildren = GetComponentInChildren<IOverhead>();
		if (componentInChildren != null)
		{
			questIcon = new GameObject("Quest Icon").AddComponent<Image>();
			componentInChildren.ShowAbove(questIcon.rectTransform);
			questIcon.sprite = Resources.Load<Sprite>("MISSION_CAN_BE_FINISHED");
		}
	}

	public override void Execute()
	{
		this.OnSpeak?.Invoke();
		this.OnSpeak = null;
		if ((bool)questIcon)
		{
			questIcon.enabled = false;
		}
		if (TryGetComponent<Consort>(out var _))
		{
			Dialogue.SetVariable("consort_met");
		}
		Dialogue.OnDone += DialogueDone;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string quest in quests)
		{
			hashSet.Add(QuestList.HasQuest(quest) ? ("Quest " + quest) : ("Done " + quest));
		}
		Dialogue.StartDialogue(file, material, base.transform.Find("SpriteHolder"), hashSet);
	}

	private void AddQuest(string quest)
	{
		quests.Add(quest);
		QuestList.InvokeQuest(quest, this);
		if (!isReceivingAcceptorEvent && TryGetComponent<ItemAcceptorCallback>(out var component))
		{
			isReceivingAcceptorEvent = true;
			component.OnReceiveItem += delegate
			{
				QuestList.RefreshQuests();
				Execute();
			};
		}
	}

	private void DialogueDone(ISet<string> vars)
	{
		foreach (string var in vars)
		{
			if (var.StartsWith("Quest "))
			{
				string text = var.Substring("Quest ".Length);
				if (!quests.Contains(text) && QuestList.AddQuest(text))
				{
					AddQuest(text);
				}
			}
		}
	}

	public void Save(Stream stream)
	{
		foreach (string quest in quests)
		{
			HouseLoader.writeString(quest, stream);
		}
		HouseLoader.writeString(string.Empty, stream);
	}

	public IEnumerable<string> Save()
	{
		return quests;
	}

	public void Load(IEnumerable<string> data)
	{
		if (data == null)
		{
			return;
		}
		foreach (string datum in data)
		{
			AddQuest(datum);
		}
	}
}
