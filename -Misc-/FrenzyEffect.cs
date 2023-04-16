using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class FrenzyEffect : StatusEffect
{
	private static Sprite[] sprites;

	private Faction oldFaction;

	public FrenzyEffect(float duration)
		: base(duration)
	{
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Rage) };
		}
		StatusEffect.AddParticles(att, sprites);
		att.LeaveStrife();
		oldFaction = att.Faction.Parent;
		att.Faction.Parent = "Chaos";
		if (att is StrifeAI strifeAI)
		{
			strifeAI.ClearTarget();
		}
	}

	public override void End(Attackable att)
	{
		att.Faction.Parent = oldFaction;
		StatusEffect.RemoveParticles(att, sprites);
	}
}
