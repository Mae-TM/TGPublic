using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class OnNextHit : StatusEffect
{
	[ProtoIgnore]
	private readonly Attack.Handler action;

	public OnNextHit(float duration, Attack.Handler action)
		: base(duration)
	{
		this.action = action;
	}

	public override bool OnAttack(Attack attack)
	{
		action(attack);
		return true;
	}
}
