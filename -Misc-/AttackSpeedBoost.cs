using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class AttackSpeedBoost : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float factor;

	public AttackSpeedBoost(float duration, float factor)
		: base(duration)
	{
		this.factor = factor;
	}

	public AttackSpeedBoost(AttackSpeedBoost orig, float factor)
		: base(orig.EndTime - Time.time)
	{
		this.factor = orig.factor * factor;
	}

	public override void Begin(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.AttackSpeed *= factor;
		}
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Time) };
		}
		StatusEffect.AddParticles(att, sprites, Mathf.Log(factor, 1.5f));
	}

	public override void End(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.AttackSpeed /= factor;
		}
		StatusEffect.RemoveParticles(att, sprites, Mathf.Log(factor, 1.5f));
	}
}
