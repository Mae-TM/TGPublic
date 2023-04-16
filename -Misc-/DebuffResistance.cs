using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class DebuffResistance : StatusEffect
{
	private readonly float reduction;

	public DebuffResistance(float duration, float reduction)
		: base(duration)
	{
		this.reduction = reduction;
	}

	public override void Begin(Attackable att)
	{
		att.StatusEffects.OnAffect += OnAffect;
	}

	public override void End(Attackable att)
	{
		att.StatusEffects.OnAffect -= OnAffect;
	}

	private void OnAffect(ref IStatusEffect effect)
	{
		if (effect is SlowEffect || effect is TauntEffect || effect is PoisonEffect || effect is BurningEffect || effect is RadiationEffect || effect is WeakenEffect)
		{
			((StatusEffect)effect).ReduceTime(reduction);
		}
	}
}
