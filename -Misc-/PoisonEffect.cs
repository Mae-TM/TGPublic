using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class PoisonEffect : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float intensity;

	private readonly float period;

	public PoisonEffect(float duration, float intensity, float period)
		: base(duration)
	{
		this.intensity = intensity;
		this.period = period;
	}

	public override float Update(Attackable att)
	{
		att.Damage(intensity);
		return period;
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = Resources.LoadAll<Sprite>("Effect/Poison");
		}
		StatusEffect.AddParticles(att, sprites, intensity / period);
	}

	public override void End(Attackable att)
	{
		StatusEffect.RemoveParticles(att, sprites, intensity / period);
	}
}
