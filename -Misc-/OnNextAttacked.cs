using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class OnNextAttacked : StatusEffect
{
	private readonly PBColor? color;

	[ProtoIgnore]
	private readonly Attack.Handler action;

	[ProtoIgnore]
	private GameObject shield;

	public OnNextAttacked(float duration, Attack.Handler action, PBColor? color = null)
		: base(duration)
	{
		this.color = color;
		this.action = action;
	}

	public override void Begin(Attackable att)
	{
		if (color.HasValue)
		{
			SpriteRenderer prefab = Resources.Load<SpriteRenderer>("Shield");
			shield = StatusEffect.MakeShield(att, prefab, color.Value).gameObject;
		}
	}

	public override void End(Attackable att)
	{
		if (shield != null)
		{
			Object.Destroy(shield);
		}
	}

	public override bool OnAttacked(Attack attack)
	{
		if (attack.target == null)
		{
			return false;
		}
		action(attack);
		return true;
	}
}
