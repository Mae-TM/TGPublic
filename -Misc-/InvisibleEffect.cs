using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class InvisibleEffect : StatusEffect
{
	private readonly float cost;

	private readonly float speedBoost;

	private readonly bool isPerfect;

	private Faction oldFaction;

	[ProtoIgnore]
	private SpriteRenderer[] sprites;

	public InvisibleEffect(float duration, float cost = 0f, float speedBoost = 0f, bool isPerfect = true)
		: base(duration)
	{
		this.cost = cost;
		this.speedBoost = speedBoost;
		this.isPerfect = isPerfect;
	}

	public override void Begin(Attackable att)
	{
		att.Speed *= speedBoost + 1f;
		oldFaction = att.Faction.Parent;
		att.Faction.Parent = null;
		if (isPerfect)
		{
			StrifeAI[] componentsInChildren = att.transform.parent.GetComponentsInChildren<StrifeAI>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ClearTarget(att);
			}
			foreach (StrifeAI item in att.Enemies.OfType<StrifeAI>())
			{
				item.ClearTarget(att);
			}
			att.LeaveStrife();
		}
		sprites = att.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		bool flag = isPerfect && att != Player.player;
		SpriteRenderer[] array = sprites;
		foreach (SpriteRenderer obj in array)
		{
			Color color = obj.color;
			color.a = (((bool)att == flag) ? 0f : (color.a / 5f));
			obj.color = color;
		}
	}

	public override float Update(Attackable att)
	{
		if (cost <= 0f || !(att is Attacking attacking))
		{
			return float.PositiveInfinity;
		}
		if (attacking.Vim < cost * 0.5f)
		{
			return -1f;
		}
		attacking.Vim -= cost * 0.5f;
		return 0.5f;
	}

	public override bool OnAttack(Attack attack)
	{
		return true;
	}

	public override void End(Attackable att)
	{
		att.Speed /= speedBoost + 1f;
		att.Faction.Parent = oldFaction;
		bool flag = isPerfect && att != Player.player;
		SpriteRenderer[] array = sprites;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			if (!(spriteRenderer == null))
			{
				Color color = spriteRenderer.color;
				color.a = (((bool)att == flag) ? 1f : (color.a * 5f));
				spriteRenderer.color = color;
			}
		}
	}
}
