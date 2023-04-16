using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class TauntEffect : StatusEffect
{
	private static Sprite[] sprites;

	private readonly Attacking target;

	public TauntEffect(float duration, Attacking target)
		: base(duration)
	{
		this.target = target;
	}

	public override float Update(Attackable att)
	{
		att.Damage(0f, target);
		return 0f;
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = Resources.LoadAll<Sprite>("Effect/Menacing");
		}
		StatusEffect.AddParticles(att, sprites);
	}

	public override void End(Attackable att)
	{
		StatusEffect.RemoveParticles(att, sprites);
	}
}
