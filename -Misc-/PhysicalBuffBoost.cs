using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class PhysicalBuffBoost : StatusEffect
{
	private readonly float factor;

	public PhysicalBuffBoost(float duration, float factor)
		: base(duration)
	{
		this.factor = factor;
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
		IStatusEffect statusEffect = effect;
		IStatusEffect statusEffect2 = ((statusEffect is SlowEffect orig) ? new SlowEffect(orig, factor) : ((statusEffect is WeakenEffect orig2) ? new WeakenEffect(orig2, factor) : ((statusEffect is TimeEffect orig3) ? new TimeEffect(orig3, factor) : ((statusEffect is AttackBoost orig4) ? new AttackBoost(orig4, factor) : ((!(statusEffect is AttackSpeedBoost orig5)) ? effect : new AttackSpeedBoost(orig5, factor))))));
		effect = statusEffect2;
	}
}
