using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class WeakenEffect : StatusEffect
{
	private static Sprite[] sprites;

	private static Sprite[] negSprites;

	private readonly float slow;

	private readonly float defense;

	public WeakenEffect(float duration, float slow, float defense)
		: base(duration)
	{
		this.slow = slow;
		this.defense = defense;
	}

	public WeakenEffect(WeakenEffect orig, float factor = 1f)
		: base(orig.EndTime - Time.time)
	{
		slow = orig.slow * factor;
		defense = orig.defense * factor;
	}

	public override void Begin(Attackable att)
	{
		att.Speed /= slow + 1f;
		att.Defense += 0f - defense;
		if (slow + defense >= 0f)
		{
			if (sprites == null)
			{
				sprites = Resources.LoadAll<Sprite>("Effect/Poison");
			}
			StatusEffect.AddParticles(att, sprites, slow + defense);
			return;
		}
		if (negSprites == null)
		{
			negSprites = new Sprite[1] { Classpect.GetIcon(Aspect.Heart) };
		}
		StatusEffect.AddParticles(att, negSprites, 0f - slow - defense);
	}

	public override void End(Attackable att)
	{
		att.Speed *= slow + 1f;
		att.Defense += defense;
		if (slow + defense >= 0f)
		{
			StatusEffect.RemoveParticles(att, sprites, slow + defense);
		}
		else
		{
			StatusEffect.RemoveParticles(att, negSprites, 0f - slow - defense);
		}
	}
}
