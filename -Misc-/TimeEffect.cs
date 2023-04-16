using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class TimeEffect : StatusEffect
{
	private static readonly Sprite[] gear = Resources.LoadAll<Sprite>("Gear");

	private readonly float factor;

	[ProtoIgnore]
	private AbilityAnimator animator;

	public TimeEffect(float duration, float factor)
		: base(duration)
	{
		this.factor = factor;
	}

	public TimeEffect(TimeEffect orig, float factor)
		: base(orig.EndTime - Time.time)
	{
		this.factor = orig.factor * factor;
	}

	public override void Begin(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.AttackSpeed *= factor;
		}
		att.Speed *= factor;
		animator = AbilityAnimator.Make(gear, att.transform, 0.5f, front: false, 0.1f / factor, top: true);
		animator.loop = true;
	}

	public override void End(Attackable att)
	{
		if (att is Attacking attacking)
		{
			attacking.AttackSpeed /= factor;
		}
		att.Speed /= factor;
		Object.Destroy(animator.gameObject);
	}
}
