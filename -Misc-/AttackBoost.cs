using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class AttackBoost : StatusEffect
{
	private static Sprite[] sprites;

	private float delta;

	public AttackBoost(float duration, float delta)
		: base(duration)
	{
		this.delta = delta;
	}

	public AttackBoost(AttackBoost orig, float factor)
		: base(orig.EndTime - Time.time)
	{
		delta = orig.delta * factor;
	}

	public override void Begin(Attackable att)
	{
		if (att is Attacking attacking)
		{
			delta = Mathf.Max(delta, 0f - attacking.AttackDamage);
			attacking.AttackDamage += delta;
		}
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Rage) };
		}
		StatusEffect.AddParticles(att, sprites, delta);
	}

	public override void End(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.AttackDamage -= delta;
		}
		StatusEffect.RemoveParticles(att, sprites, delta);
	}
}
