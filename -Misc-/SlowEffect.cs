using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class SlowEffect : StatusEffect
{
	private static Sprite[] slowSprites;

	private static Sprite[] speedSprites;

	private readonly float intensity;

	public SlowEffect(float duration, float intensity)
		: base(duration)
	{
		this.intensity = intensity;
	}

	public SlowEffect(SlowEffect orig, float factor = 1f)
		: base(orig.EndTime - Time.time)
	{
		intensity = orig.intensity * factor;
	}

	public override void Begin(Attackable att)
	{
		att.Speed /= intensity + 1f;
		if (intensity > 0f)
		{
			if (slowSprites == null)
			{
				slowSprites = Resources.LoadAll<Sprite>("Effect/Cold");
			}
			StatusEffect.AddParticles(att, slowSprites, intensity);
		}
		else if (intensity < 0f)
		{
			if (speedSprites == null)
			{
				speedSprites = new Sprite[1] { Classpect.GetIcon(Aspect.Breath) };
			}
			StatusEffect.AddParticles(att, speedSprites, 1f / (1f + intensity) - 1f);
		}
	}

	public override void End(Attackable att)
	{
		att.Speed *= intensity + 1f;
		if (intensity > 0f)
		{
			StatusEffect.RemoveParticles(att, slowSprites, intensity);
		}
		else
		{
			StatusEffect.RemoveParticles(att, speedSprites, 1f / (1f + intensity) - 1f);
		}
	}
}
