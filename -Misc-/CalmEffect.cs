using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class CalmEffect : StatusEffect
{
	private static Sprite[] sprites;

	public CalmEffect(float duration)
		: base(duration)
	{
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = new Sprite[1] { Classpect.GetIcon(Aspect.Rage) };
		}
		StatusEffect.AddParticles(att, sprites);
		att.LeaveStrife();
		if (att is StrifeAI strifeAI)
		{
			strifeAI.ClearTarget();
			strifeAI.ApplyCalm();
		}
	}

	public override void End(Attackable att)
	{
		if (att is StrifeAI strifeAI)
		{
			strifeAI.RemoveCalm();
		}
		StatusEffect.RemoveParticles(att, sprites);
	}
}
