using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class HealthRegenBoost : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float increment;

	public HealthRegenBoost(float duration, float increment)
		: base(duration)
	{
		this.increment = increment;
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Life) };
		}
		StatusEffect.AddParticles(att, sprites, increment);
		att.HealthRegen += increment;
	}

	public override void End(Attackable att)
	{
		StatusEffect.RemoveParticles(att, sprites, increment);
		att.HealthRegen -= increment;
	}
}
