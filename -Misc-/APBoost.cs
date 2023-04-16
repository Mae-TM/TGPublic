using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class APBoost : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float delta;

	public APBoost(float duration, float delta)
		: base(duration)
	{
		this.delta = delta;
	}

	public override void Begin(Attackable att)
	{
		if (att is Player player)
		{
			player.AbilityPower += delta;
		}
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Space) };
		}
		StatusEffect.AddParticles(att, sprites, delta);
	}

	public override void End(Attackable att)
	{
		if (att is Player player)
		{
			player.AbilityPower -= delta;
		}
		StatusEffect.RemoveParticles(att, sprites, delta);
	}
}
