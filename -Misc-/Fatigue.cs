using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class Fatigue : StatusEffect
{
	private readonly float duration;

	private readonly float intensity;

	public Fatigue(float duration, float intensity)
		: base(duration)
	{
		this.duration = duration;
		this.intensity = intensity;
	}

	public override void Begin(Attackable att)
	{
		att.Affect(new SlowEffect(duration, intensity), stacking: false);
		att.HealthRegen -= 0.25f;
	}

	public override void End(Attackable att)
	{
		att.HealthRegen += 0.25f;
	}
}
