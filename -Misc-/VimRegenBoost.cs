using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class VimRegenBoost : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float increment;

	public VimRegenBoost(float duration, float increment)
		: base(duration)
	{
		this.increment = increment;
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Mind) };
		}
		StatusEffect.AddParticles(att, sprites, increment);
		if (att is Attacking attacking)
		{
			attacking.VimRegen += increment;
		}
	}

	public override void End(Attackable att)
	{
		StatusEffect.RemoveParticles(att, sprites, increment);
		if (att is Attacking attacking)
		{
			attacking.VimRegen -= increment;
		}
	}
}
