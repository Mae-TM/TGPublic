using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class Sylladex : MonoBehaviour
{
	public Specibus strifeSpecibus;

	public QuestList quests;

	private Modus captchaModus;

	[SerializeField]
	private GameObject modusObject;

	[SerializeField]
	private AbilityButton abilityButtonTemplate;

	private readonly List<AbilityButton> abilityButton = new List<AbilityButton>();

	[SerializeField]
	private RectTransform itemView;

	public Image modusIcon;

	public RectTransform modusSettings;

	[SerializeField]
	private ModusPicker modusPicker;

	[SerializeField]
	private GameObject strifeText;

	[SerializeField]
	private DragCard dragCard;

	private AudioSource soundEffect;

	[SerializeField]
	private AudioClip clipAccept;

	[SerializeField]
	private AudioClip clipRemove;

	[SerializeField]
	private AudioClip clipEject;

	[SerializeField]
	private AudioClip clipReject;

	[SerializeField]
	private AudioClip clipSwitch;

	[SerializeField]
	private AudioClip clipChange;

	private static List<string> modi = new List<string> { "Queue", "Stack", "Array", "Hashmap", "Tree" };

	private string modusName;

	private string playerName;

	private static char[] metrics = new char[5] { 'k', 'M', 'G', 'T', 'P' };

	private readonly string[] illegalNames = new string[11]
	{
		"smell", "poop", "broad", "insufferable", "prick", "priick", "stink", "butt", "penis", "bepis",
		"scunthorpe"
	};

	public string PlayerName
	{
		get
		{
			return playerName;
		}
		private set
		{
			playerName = value;
			UnityEngine.Object.Destroy(Player.Ui.Find("NameInput").gameObject);
			BuildExploreSwitcher.Instance.SwitchToExplore();
			GlobalChat.SetupChat();
		}
	}

	public bool AutoPickup
	{
		get
		{
			if (captchaModus != null)
			{
				return captchaModus.AutoPickup;
			}
			return false;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(quests.UpdateQuests());
	}

	private void Start()
	{
		strifeSpecibus.sylladex = this;
		abilityButton[0].isRepeating = true;
		abilityButton[1].requiresMoving = false;
		abilityButton[2].requiresMoving = true;
		dragCard.reject = delegate(Item item, bool playSound)
		{
			if (playSound)
			{
				PlayRejectCard();
			}
			ThrowItem(item);
		};
		foreach (string item in modi)
		{
			modusPicker.AddModus(item);
		}
		modusPicker.OnPickModus += delegate(string picked)
		{
			SetModus(picked, playSound: true);
		};
		if (captchaModus == null)
		{
			SetModus(ModusPickerComponent.Modus);
		}
		if (BuildExploreSwitcher.cheatMode)
		{
			PlayerName = "Builder";
		}
		if (TutorialDirector.isTutorial)
		{
			PlayerName = "John Egbert";
		}
		if (string.IsNullOrEmpty(PlayerName))
		{
			KeyboardControl.Block();
			Player.Ui.Find("NameInput").GetComponentInChildren<InputField>().textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
		}
	}

	public void RefreshAbilityButtons(IEnumerable<Attacking.Ability> abilities, Action<int, Attackable, Vector3?> onExecute)
	{
		int i = 0;
		foreach (Attacking.Ability ability in abilities)
		{
			if (i >= abilityButton.Count)
			{
				abilityButton.Add(UnityEngine.Object.Instantiate(abilityButtonTemplate, abilityButtonTemplate.transform.parent));
				int index = i;
				abilityButton[i].OnExecute += delegate(Attackable target, Vector3? position)
				{
					onExecute(index, target, position);
				};
				abilityButton[i].gameObject.SetActive(value: true);
			}
			abilityButton[i].SetBinding(KeyboardControl.GetAbilityBinding(i));
			abilityButton[i].SetAbility(ability);
			i++;
		}
		for (; i < abilityButton.Count; i++)
		{
			abilityButton[i].UnsetAbility();
		}
	}

	public void SetBasicAttack(string name, Color color)
	{
		abilityButton[0].SetAppearance(name, color);
	}

	public Modus GetCaptchaModus()
	{
		return captchaModus;
	}

	public void SetModus(string modus, bool playSound = false)
	{
		switch (modus)
		{
		case "Stack":
			captchaModus = modusObject.AddComponent<FILOModus>();
			break;
		case "Array":
			captchaModus = modusObject.AddComponent<ArrayModus>();
			break;
		case "Hashmap":
			captchaModus = modusObject.AddComponent<HashmapModus>();
			break;
		case "Tree":
			captchaModus = modusObject.AddComponent<TreeModus>();
			break;
		default:
			captchaModus = modusObject.AddComponent<FIFOModus>();
			break;
		}
		modusName = modus;
		if (playSound)
		{
			PlaySoundEffect(clipSwitch);
		}
	}

	public void OpenModusSettings()
	{
		captchaModus.OpenSettings();
	}

	public void ActivateGameObject(GameObject todo)
	{
		todo.SetActive(value: true);
	}

	private void Update()
	{
		if (GetDragItem() == null && KeyboardControl.IsItemAction && Input.GetMouseButtonDown(0) && !KeyboardControl.IsMouseBlocked())
		{
			Player.player.GetComponent<PlayerController>().FaceMouse();
			PickupItemAction pickupItemAction = FindNearestPickup(6f);
			if ((object)pickupItemAction != null && pickupItemAction.TryGetComponent<Rigidbody>(out var component) && component.velocity.sqrMagnitude < 1f)
			{
				pickupItemAction.Execute();
			}
		}
	}

	public void OnStrifeStart(IEnumerable<Attackable> enemies)
	{
		strifeText.SetActive(value: true);
		StartCoroutine(DisableAfterTime(1f, strifeText));
		AudioClip audioClip = enemies.Select((Attackable enemy) => enemy.StrifeClip).FirstOrDefault((AudioClip strifeClip) => strifeClip != null);
		if ((object)audioClip == null)
		{
			audioClip = Player.player.RegionChild.Region.StrifeMusic;
			if ((object)audioClip == null)
			{
				return;
			}
		}
		BackgroundMusic.instance.PlayEvent(audioClip, loop: true, 1f);
	}

	private IEnumerator DisableAfterTime(float seconds, GameObject gameObject)
	{
		yield return new WaitForSeconds(seconds);
		gameObject.SetActive(value: false);
	}

	public void OnStrifeEnd()
	{
		BackgroundMusic.instance.ResumeNormal(1f);
	}

	public void SetDragItem(Item item, Material mat = null)
	{
		if ((!KeyboardControl.IsQuickAction || !Player.player.AcceptItem(item)) && (!KeyboardControl.IsItemAction || !captchaModus.AcceptItem(item)))
		{
			if (mat == null)
			{
				mat = modusSettings.GetComponent<Image>().material;
			}
			dragCard.SetItem(item, mat);
		}
	}

	public bool AddItem(Item item)
	{
		if (KeyboardControl.IsQuickAction)
		{
			return Player.player.AcceptItem(item);
		}
		if (captchaModus != null)
		{
			return captchaModus.AcceptItem(item);
		}
		return false;
	}

	public bool AcceptItem(Item item)
	{
		if (strifeSpecibus.AcceptItem(item))
		{
			return true;
		}
		if (captchaModus != null)
		{
			return captchaModus.AcceptItem(item);
		}
		return false;
	}

	public int CountItem(Item item)
	{
		int num = (item.Equals(GetDragItem()) ? 1 : 0);
		if (captchaModus != null)
		{
			num += captchaModus.CountItem(item);
		}
		return num;
	}

	public Item GetDragItem()
	{
		return dragCard.GetItem();
	}

	public PickupItemAction FindNearestPickup(float dist = float.PositiveInfinity)
	{
		dist *= dist;
		PickupItemAction result = null;
		Transform transform = Player.player.transform;
		PickupItemAction[] componentsInChildren = transform.parent.GetComponentsInChildren<PickupItemAction>();
		foreach (PickupItemAction pickupItemAction in componentsInChildren)
		{
			if (pickupItemAction.enabled)
			{
				float sqrMagnitude = (transform.position - pickupItemAction.transform.position).sqrMagnitude;
				if (sqrMagnitude < dist)
				{
					dist = sqrMagnitude;
					result = pickupItemAction;
				}
			}
		}
		return result;
	}

	public void OpenItemView(Item item)
	{
		itemView.gameObject.SetActive(value: true);
		Text[] componentsInChildren = itemView.GetComponentsInChildren<Text>();
		componentsInChildren[0].text = item.GetItemName();
		componentsInChildren[1].text = string.Empty;
		if (item is NormalItem normalItem)
		{
			if (normalItem.weaponKind.Length == 1)
			{
				componentsInChildren[1].text = normalItem.weaponKind[0].ToString() + "kind";
			}
			else if (normalItem.armor != ArmorKind.None)
			{
				componentsInChildren[1].text = normalItem.armor.ToString();
			}
		}
		componentsInChildren[2].text = item.description;
	}

	public void ThrowItem(Item item)
	{
		PlaySoundEffect(clipRemove);
		if (item.SceneObject != null)
		{
			Vector3 forward = Player.player.sync.GetForward(local: false);
			Vector3 spawnPos = ModelUtility.GetSpawnPos(item.SceneObject.transform, Player.player, forward);
			item.PutDown(Player.player.RegionChild.Area, spawnPos);
		}
		else
		{
			Debug.LogError("No gameObject set, item lost.");
		}
	}

	public void EjectItems()
	{
		if (!(captchaModus != null))
		{
			return;
		}
		captchaModus.Eject();
		SetModus(modusName);
		Item[] array = strifeSpecibus.EjectWeapons(new Item[1] { strifeSpecibus.GetActive() });
		foreach (Item item in array)
		{
			if (item != null)
			{
				ThrowItem(item);
			}
		}
	}

	public SylladexData Save()
	{
		SylladexData result = default(SylladexData);
		result.modus = modusName;
		result.modusData = captchaModus.Save();
		result.specibus = strifeSpecibus.Save();
		result.quests = quests.Save();
		result.characterName = PlayerName;
		return result;
	}

	public void Load(SylladexData data)
	{
		SetModus(data.modus);
		captchaModus.Load(data.modusData);
		strifeSpecibus.Load(data.specibus);
		quests.Load(data.quests);
		PlayerName = data.characterName;
	}

	public static string MetricFormat(float a)
	{
		int num = -1;
		while (a >= 1000f && num + 1 < metrics.Length)
		{
			num++;
			a /= 1000f;
		}
		a = (float)Math.Round(a, 1);
		if (num != -1)
		{
			return a.ToString() + metrics[num];
		}
		return a.ToString();
	}

	public void PlaySoundEffect(AudioClip clip)
	{
		if (soundEffect == null && !TryGetComponent<AudioSource>(out soundEffect))
		{
			soundEffect = base.gameObject.AddComponent<AudioSource>();
		}
		soundEffect.clip = clip;
		soundEffect.Play();
	}

	public void PlayAcceptCard()
	{
		PlaySoundEffect(clipAccept);
	}

	public void PlayRejectCard()
	{
		PlaySoundEffect(clipReject);
	}

	public void PlayEjectCard()
	{
		PlaySoundEffect(clipEject);
	}

	public bool AddCaptchaCard(int cardCount)
	{
		if (captchaModus != null)
		{
			captchaModus.ItemCapacity += cardCount;
			return true;
		}
		return false;
	}

	public void AddModus(string modus)
	{
		modi.Add(modus);
		modusPicker.AddModus(modus);
	}

	public bool HasModus(string modus)
	{
		return modi.Contains(modus);
	}

	public string GetModus()
	{
		return modusName;
	}

	public static List<string> GetModi()
	{
		return modi;
	}

	public void EjectModus()
	{
		modusSettings.gameObject.SetActive(value: false);
		captchaModus.Eject();
		PlaySoundEffect(clipChange);
		modusPicker.gameObject.SetActive(value: true);
		captchaModus = null;
	}

	public Material GetMaterial()
	{
		return quests.transform.parent.GetComponent<Image>().material;
	}

	private bool IsGoodPlayerName(string name)
	{
		if (name.Length == 0)
		{
			return false;
		}
		string[] array = illegalNames;
		foreach (string value in array)
		{
			if (name.Contains(value))
			{
				return false;
			}
		}
		if (PrettyFilter.IsNotPretty(name))
		{
			return false;
		}
		return true;
	}

	public void SetName(InputField input)
	{
		string text = input.text.ToLower();
		if (IsGoodPlayerName(text))
		{
			input.textComponent.transform.GetChild(0).gameObject.SetActive(value: true);
			StartCoroutine(AcceptRejectName(input, accept: true));
		}
		else
		{
			input.textComponent.enabled = false;
			input.transform.GetChild(2).gameObject.SetActive(value: true);
			StartCoroutine(AcceptRejectName(input, accept: false));
		}
	}

	private IEnumerator AcceptRejectName(InputField input, bool accept)
	{
		yield return new WaitForSeconds(1f);
		if (accept)
		{
			KeyboardControl.Unblock();
			PlayerName = input.text;
			quests.AddStartQuest();
		}
		else
		{
			input.text = "";
			input.textComponent.enabled = true;
			input.transform.GetChild(2).gameObject.SetActive(value: false);
		}
	}
}
