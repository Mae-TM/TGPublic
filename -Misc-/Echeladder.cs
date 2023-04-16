using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RandomExtensions;
using UnityEngine;
using UnityEngine.UI;

public class Echeladder : MonoBehaviour
{
	public readonly struct Change
	{
		public readonly float healthMax;

		public readonly float healthRegen;

		public readonly float offense;

		public readonly float strength;

		public readonly float boonBucks;

		public readonly float vimMax;

		public readonly float vimRegen;

		public readonly float speed;

		public readonly float defense;

		public readonly float abilityPower;

		public readonly Attacking.Ability ability;

		public Change(Player player, Attacking.Ability ability = null)
		{
			healthMax = player.HealthMax;
			healthRegen = player.HealthRegen;
			offense = player.Offense;
			strength = player.Strength;
			boonBucks = player.boonBucks;
			vimMax = player.VimMax;
			vimRegen = player.VimRegen;
			speed = player.Speed;
			defense = player.Defense;
			abilityPower = player.AbilityPower;
			this.ability = ability;
		}

		public Change(Change from, Change to, float t)
		{
			healthMax = Mathf.Round(Mathf.Lerp(from.healthMax, to.healthMax, t));
			healthRegen = Mathf.Lerp(from.healthRegen, to.healthRegen, t);
			offense = Mathf.Lerp(from.offense, to.offense, t);
			strength = Mathf.Lerp(from.strength, to.strength, t);
			boonBucks = Mathf.Round(Mathf.Lerp(from.boonBucks, to.boonBucks, t));
			vimMax = Mathf.Lerp(from.vimMax, to.vimMax, t);
			vimRegen = Mathf.Lerp(from.vimRegen, to.vimRegen, t);
			speed = Mathf.Lerp(from.speed, to.speed, t);
			defense = Mathf.Lerp(from.defense, to.defense, t);
			abilityPower = Mathf.Lerp(from.abilityPower, to.abilityPower, t);
			ability = null;
		}
	}

	private const int RUNG_COUNT = 20;

	[SerializeField]
	private AudioClip levelUpMusic;

	private Transform[] rungs;

	private uint shownRung;

	private Text healthText;

	private Text offenseText;

	private Text strengthText;

	private Text boonBucksText;

	private Text vimText;

	private Text speedText;

	private Text defenseText;

	private Text abilityPowerText;

	private readonly string[] rungColor = new string[40]
	{
		"#4fd400", "#fdff2b", "#fdff2b", "#ff942b", "#ff2b8f", "#000000", "#24d9d7", "#008ea0", "#a120ac", "#8aff33",
		"#ffae00", "#f80000", "#a8ffa8", "#ffb8af", "#345aff", "#00fff0", "#dfbb6c", "#ff0000", "#b1ffff", "#595959",
		"#f12b26", "#ffffff", "#d885d9", "#6c00ff", "#e4ff00", "#6d9a00", "#3dff17", "#00970b", "#c6ff17", "#01948b",
		"#d6d6d6", "#b101af", "#751b6e", "#ffffff", "#b4afef", "#1200bd", "#d80000", "#ff9ca5", "#dec69b", "#008a2d"
	};

	private void Awake()
	{
		if (rungs == null)
		{
			MakeRungs();
		}
		healthText = base.transform.Find("Gel").GetChild(0).GetComponent<Text>();
		offenseText = base.transform.Find("Offense").GetChild(0).GetComponent<Text>();
		strengthText = base.transform.Find("Speedboost").GetChild(0).GetComponent<Text>();
		boonBucksText = base.transform.Find("Porkhollow").GetChild(1).GetComponent<Text>();
		vimText = base.transform.Find("Vim").GetChild(0).GetComponent<Text>();
		speedText = base.transform.Find("Speed").GetChild(0).GetComponent<Text>();
		defenseText = base.transform.Find("Defense").GetChild(0).GetComponent<Text>();
		abilityPowerText = base.transform.Find("Aspect").GetChild(0).GetComponent<Text>();
	}

	private static Stack<string> GetEcheladderOptions(string path)
	{
		string[] array = StreamingAssets.ReadAllLines(path);
		array.Shuffle();
		return new Stack<string>(array);
	}

	private static IEnumerable<string> GenerateRungs()
	{
		SessionRandom.Seed(Player.player.GetID());
		Classpect classpect = Player.player.classpect;
		Stack<string>[] prefixes = new Stack<string>[3]
		{
			GetEcheladderOptions("echeladder1.txt"),
			GetEcheladderOptions($"Classes/{classpect.role}/echeladder1.txt"),
			GetEcheladderOptions($"Aspects/{classpect.aspect}/echeladder1.txt")
		};
		Stack<string>[] suffixes = new Stack<string>[3]
		{
			GetEcheladderOptions("echeladder2.txt"),
			GetEcheladderOptions($"Classes/{classpect.role}/echeladder2.txt"),
			GetEcheladderOptions($"Aspects/{classpect.aspect}/echeladder2.txt")
		};
		int i = 0;
		while (i < 20)
		{
			int num;
			if (Random.Range(0, 100) >= i * 4 && prefixes[0].Count != 0)
			{
				num = 0;
			}
			else
			{
				int minInclusive = -prefixes[1].Count * suffixes[2].Count;
				int maxExclusive = prefixes[2].Count * suffixes[1].Count;
				num = ((Random.Range(minInclusive, maxExclusive) < 0) ? 1 : 2);
			}
			int num2;
			if (Random.Range(0, 100) >= i * 4 && suffixes[0].Count != 0)
			{
				num2 = 0;
			}
			else if (num != 0)
			{
				num2 = 3 - num;
			}
			else
			{
				int minInclusive2 = -suffixes[1].Count;
				int count = suffixes[2].Count;
				num2 = ((Random.Range(minInclusive2, count) < 0) ? 1 : 2);
			}
			if (prefixes[num].Count == 0)
			{
				num = 0;
			}
			if (suffixes[num2].Count == 0)
			{
				num2 = 0;
			}
			string text = prefixes[num].Pop();
			string text2 = suffixes[num2].Pop();
			if (text.StartsWith("#-"))
			{
				yield return text2 + text.Substring(1);
			}
			else if (text.StartsWith("#"))
			{
				yield return text2 + " " + text.Substring(1);
			}
			else
			{
				yield return text + " " + text2;
			}
			int num3 = i + 1;
			i = num3;
		}
	}

	private void MakeRungs()
	{
		RectTransform content = (RectTransform)base.transform.Find("Echeladder").GetChild(0).GetChild(0);
		content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 400f);
		Transform rungTemplate = content.GetChild(0);
		rungs = GenerateRungs().Select(delegate(string rungName, int index)
		{
			Transform obj = Object.Instantiate(rungTemplate, content);
			obj.name = rungName;
			obj.transform.localPosition = new Vector3(0f, 20 * index, 0f);
			obj.transform.GetChild(1).GetComponent<Text>().text = rungName;
			return obj;
		}).ToArray();
		Object.Destroy(rungTemplate.gameObject);
		ShowLevel(1u);
	}

	private void ShowStats(Change change)
	{
		healthText.text = Sylladex.MetricFormat(change.healthMax) + " +" + Sylladex.MetricFormat(change.healthRegen) + "/s";
		offenseText.text = Sylladex.MetricFormat(change.offense);
		strengthText.text = Sylladex.MetricFormat(change.strength);
		boonBucksText.text = Sylladex.MetricFormat(change.boonBucks);
		vimText.text = Sylladex.MetricFormat(change.vimMax) + " +" + Sylladex.MetricFormat(change.vimRegen) + "/s";
		speedText.text = Sylladex.MetricFormat(change.speed);
		defenseText.text = Sylladex.MetricFormat(change.defense);
		abilityPowerText.text = Sylladex.MetricFormat(change.abilityPower);
	}

	private void OnEnable()
	{
		Change change = new Change(Player.player);
		uint level = Player.player.Level;
		ICollection<Change> collection = Player.player.LevelUp();
		if (collection == null)
		{
			ShowStats(change);
			ShowLevel(level);
		}
		else
		{
			ShowChanges(level, change, collection);
		}
	}

	private void OnDisable()
	{
		base.transform.Find("Ability").gameObject.SetActive(value: false);
	}

	public void ShowChanges(uint fromLevel, Change first, ICollection<Change> changes)
	{
		StartCoroutine(ShowChangesRoutine(fromLevel, first, changes));
	}

	private IEnumerator ShowChangesRoutine(uint fromLevel, Change first, ICollection<Change> changes)
	{
		BackgroundMusic.instance.PlayEvent(levelUpMusic, loop: false);
		float duration = 2f / (float)changes.Count;
		Text abilityText = base.transform.Find("Ability").GetChild(0).GetComponent<Text>();
		Change prev = first;
		foreach (Change change in changes)
		{
			if (change.ability != null)
			{
				abilityText.transform.parent.gameObject.SetActive(value: true);
				string text = ColorUtility.ToHtmlStringRGB(change.ability.color);
				abilityText.text = "Learned <color=#" + text + ">" + change.ability.name.ToUpper() + "</color>";
			}
			float startTime = Time.time;
			float endTime = startTime + duration;
			while (Time.time <= endTime)
			{
				ShowStats(new Change(prev, change, (Time.time - startTime) / duration));
				yield return null;
			}
			prev = change;
			Echeladder echeladder = this;
			uint num = fromLevel + 1;
			fromLevel = num;
			echeladder.ShowLevel(num);
		}
		ShowStats(prev);
	}

	private void ShowLevel(uint level)
	{
		if (rungs == null)
		{
			MakeRungs();
		}
		for (uint num = shownRung; num < level && num < rungs.Length; num++)
		{
			if (num * 2 >= rungColor.Length || !ColorUtility.TryParseHtmlString(rungColor[num * 2], out var color))
			{
				color = new Color(0f, 0.5f, 0f);
			}
			rungs[num].GetChild(0).GetComponent<Image>().color = color;
			if (num * 2 + 1 >= rungColor.Length || !ColorUtility.TryParseHtmlString(rungColor[num * 2 + 1], out color))
			{
				color = new Color(1f, 1f, 0f);
			}
			rungs[num].GetChild(1).GetComponent<Text>().color = color;
		}
		shownRung = level;
	}
}
