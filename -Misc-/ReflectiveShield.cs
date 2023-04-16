using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class ReflectiveShield : StatusEffect
{
	private static Sprite[] appear;

	private static Sprite[] idle;

	private static Sprite[] react;

	private readonly float factor;

	[ProtoIgnore]
	private AbilityAnimator shield;

	public ReflectiveShield(float duration, float factor)
		: base(duration)
	{
		this.factor = factor;
	}

	public override void Begin(Attackable att)
	{
		if (appear == null)
		{
			Sprite[] sourceArray = Resources.LoadAll<Sprite>("Bravery");
			appear = new Sprite[3];
			Array.Copy(sourceArray, 0, appear, 0, 3);
			idle = new Sprite[3];
			Array.Copy(sourceArray, 3, idle, 0, 3);
			react = new Sprite[3];
			Array.Copy(sourceArray, 6, react, 0, 3);
		}
		float magnitude = ModelUtility.GetBounds(att.gameObject).extents.magnitude;
		shield = AbilityAnimator.Make(appear, att.transform, magnitude, front: false, 0.08f);
		shield.loop = true;
		shield.SetEndEvent(SetIdleSprites);
	}

	private void SetIdleSprites()
	{
		shield.SetSprites(idle);
		shield.SetEndEvent(null);
	}

	public override bool OnAttacked(Attack attack)
	{
		if (attack.source == null)
		{
			return false;
		}
		attack.source.Damage(factor * attack.damage);
		shield.SetSprites(react);
		shield.SetEndEvent(SetIdleSprites);
		return false;
	}

	public override void End(Attackable att)
	{
		UnityEngine.Object.Destroy(shield.gameObject);
	}
}
