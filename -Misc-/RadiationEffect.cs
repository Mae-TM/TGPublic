using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class RadiationEffect : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float radius;

	private readonly float intensity;

	private readonly float period;

	public RadiationEffect(float duration, float radius, float intensity, float period)
		: base(duration)
	{
		this.radius = radius;
		this.intensity = intensity;
		this.period = period;
	}

	public override float Update(Attackable att)
	{
		List<Attackable> list = new List<Attackable>();
		Collider[] array = Physics.OverlapSphere(att.transform.position, radius);
		for (int i = 0; i < array.Length; i++)
		{
			Attackable componentInParent = array[i].GetComponentInParent<Attackable>();
			if (!(componentInParent == null) && !list.Contains(componentInParent))
			{
				componentInParent.Damage(intensity);
				list.Add(componentInParent);
			}
		}
		return period;
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = Resources.LoadAll<Sprite>("Effect/Radiation");
		}
		StatusEffect.AddParticles(att, sprites, intensity / period);
	}

	public override void End(Attackable att)
	{
		StatusEffect.RemoveParticles(att, sprites, intensity / period);
	}
}
