using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class Faction : SubFaction
{
	private struct JsonFaction
	{
		public string parent;

		public string[] enemies;

		public bool? isAlliance;

		public string color;

		public string gelColor;
	}

	public class Member : SubFaction
	{
		public delegate void OnParentChangeHandler(Faction from, Faction to);

		private readonly Attackable member;

		private Faction parent;

		public Faction Parent
		{
			get
			{
				return parent;
			}
			set
			{
				if (parent != value)
				{
					Disable();
					this.OnParentChange?.Invoke(parent, value);
					parent = value;
					Enable();
				}
			}
		}

		public IEnumerable<Attackable> All => Faction.All;

		public event OnParentChangeHandler OnParentChange;

		public void Enable()
		{
			parent?.Add(this);
		}

		public void Disable()
		{
			parent?.Remove(this);
		}

		public Member(Attackable member)
		{
			this.member = member;
		}

		protected override IEnumerable<Faction> GetAscendants()
		{
			return Parent?.GetAscendants() ?? Enumerable.Empty<Faction>();
		}

		public override IEnumerable<Attackable> GetMembers(SubFaction exclude = null)
		{
			yield return member;
		}

		public override IEnumerable<Attackable> GetEnemies()
		{
			return Parent?.GetEnemies() ?? Enumerable.Empty<Attackable>();
		}
	}

	private static readonly IDictionary<string, Faction> factions = new Dictionary<string, Faction>();

	private static readonly Faction root = "All";

	private readonly Faction parent;

	private readonly ISet<SubFaction> children = new HashSet<SubFaction>();

	private readonly Faction[] enemies;

	public readonly bool isAlliance;

	public readonly Color color;

	public readonly Color gelColor;

	public static IEnumerable<Attackable> All => root.GetMembers();

	public override string ToString()
	{
		return factions.FirstOrDefault((KeyValuePair<string, Faction> pair) => pair.Value == this).Key;
	}

	public static Faction Parse(string name)
	{
		return name;
	}

	public static implicit operator Faction(string name)
	{
		if (name == null)
		{
			return null;
		}
		if (!factions.TryGetValue(name, out var value))
		{
			return LoadFaction(name);
		}
		return value;
	}

	private static Faction LoadFaction(string name)
	{
		if (!StreamingAssets.TryGetFile("Factions/" + name + ".json", out var path))
		{
			return null;
		}
		using StreamReader reader = File.OpenText(path);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		JsonFaction json = JsonSerializer.CreateDefault().Deserialize<JsonFaction>(reader2);
		Faction faction = new Faction(json);
		factions.Add(name, faction);
		faction.Init(json);
		return faction;
	}

	private Faction(JsonFaction json)
	{
		string[] array = json.enemies;
		enemies = new Faction[(array != null) ? array.Length : 0];
		isAlliance = json.isAlliance ?? true;
		string text = json.parent;
		parent = ((text != null) ? ((Faction)text) : root);
		if (!ColorUtility.TryParseHtmlString(json.color, out color))
		{
			color = parent?.color ?? Color.white;
		}
		if (!ColorUtility.TryParseHtmlString(json.gelColor, out gelColor))
		{
			gelColor = parent?.gelColor ?? Color.white;
		}
	}

	private void Init(JsonFaction json)
	{
		for (int i = 0; i < enemies.Length; i++)
		{
			enemies[i] = json.enemies[i];
		}
	}

	private void Add(SubFaction faction)
	{
		children.Add(faction);
	}

	private void Remove(SubFaction faction)
	{
		children.Remove(faction);
	}

	protected override IEnumerable<Faction> GetAscendants()
	{
		for (Faction faction = this; faction != root; faction = faction.parent)
		{
			yield return faction;
		}
	}

	public override IEnumerable<Attackable> GetMembers(SubFaction exclude = null)
	{
		return children.Where((SubFaction faction) => faction != exclude).SelectMany((SubFaction faction) => faction.GetMembers(exclude));
	}

	public override IEnumerable<Attackable> GetEnemies()
	{
		SubFaction alliance = GetAlliance();
		return GetAscendants().SelectMany((Faction faction) => faction.enemies).SelectMany((Faction faction) => faction.GetMembers(alliance));
	}
}
