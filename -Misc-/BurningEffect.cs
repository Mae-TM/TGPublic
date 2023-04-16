using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class BurningEffect : StatusEffect
{
	private static Sprite[] sprites;

	private readonly float duration;

	private readonly float intensity;

	private readonly float period;

	public BurningEffect(float duration, float intensity, float period)
		: base(duration)
	{
		this.duration = duration;
		this.intensity = intensity;
		this.period = period;
	}

	public BurningEffect(BurningEffect copy)
		: base(copy.duration)
	{
		duration = copy.duration;
		intensity = copy.intensity;
		period = copy.period;
	}

	public override float Update(Attackable att)
	{
		att.Damage(intensity);
		float num = Mathf.Max(Time.time - base.EndTime, duration);
		if (num == 0f)
		{
			return period;
		}
		Collider[] array = Physics.OverlapSphere(att.transform.position, 1.25f);
		for (int i = 0; i < array.Length; i++)
		{
			Attackable componentInParent = array[i].GetComponentInParent<Attackable>();
			if (componentInParent != null && componentInParent != att && componentInParent.GetEffect<BurningEffect>() == null)
			{
				componentInParent.Affect(new BurningEffect(num * num / duration, intensity, period));
			}
		}
		return period;
	}

	public override void Begin(Attackable att)
	{
		if (sprites == null)
		{
			sprites = Resources.LoadAll<Sprite>("Effect/Fire");
		}
		StatusEffect.AddParticles(att, sprites, intensity / period);
	}

	public override void End(Attackable att)
	{
		StatusEffect.RemoveParticles(att, sprites, intensity / period);
	}
}
