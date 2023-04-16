using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class AbilityEffectivenessBoost : StatusEffect
{
	private readonly float factor;

	public AbilityEffectivenessBoost(float duration, float factor)
		: base(duration)
	{
		this.factor = factor;
	}

	public override void Begin(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.abilityMultiplier *= factor;
		}
	}

	public override void End(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.abilityMultiplier /= factor;
		}
	}
}
