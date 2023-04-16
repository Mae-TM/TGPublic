using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class AfterNextHit : StatusEffect
{
	[ProtoIgnore]
	private readonly Attack.Handler action;

	[ProtoIgnore]
	private readonly Attack.Handler onHit;

	public AfterNextHit(float duration, Attack.Handler action, Attack.Handler onHit = null)
		: base(duration)
	{
		this.action = action;
		this.onHit = onHit;
	}

	public override bool OnAttack(Attack attack)
	{
		onHit?.Invoke(attack);
		return false;
	}

	public override bool AfterAttack(Attack attack)
	{
		action(attack);
		return true;
	}
}
