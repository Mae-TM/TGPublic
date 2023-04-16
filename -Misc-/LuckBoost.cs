using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class LuckBoost : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float delta;

	public LuckBoost(float duration, float delta)
		: base(duration)
	{
		this.delta = delta;
	}

	public override void Begin(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.luck += delta;
		}
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Light) };
		}
		StatusEffect.AddParticles(att, sprites, delta);
	}

	public override void End(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.luck -= delta;
		}
		StatusEffect.RemoveParticles(att, sprites, delta);
	}
}
