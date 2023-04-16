using System.Collections;
using UnityEngine;

public class Giclops : Enemy
{
	private class StompAbility : Ability
	{
		public StompAbility(Giclops caster)
			: base(caster, null, "Stomp", 4f, "Special")
		{
			vimcost = 1f;
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (target == null)
			{
				return true;
			}
			float sqrMagnitude = (target.transform.position - caster.transform.position).sqrMagnitude;
			float num = multiplier * 4f;
			return sqrMagnitude <= num * num;
		}

		protected override IEnumerable AfterAnimation(Attackable target, Vector3? position, float multiplier)
		{
			float radius = multiplier * 4f;
			ParticleCollision.Add(ParticleCollision.Ring(caster.gameObject, Grist.GetColor(caster.GristType), 0.25f, radius), delegate(Attackable att)
			{
				if (caster.IsValidTarget(att))
				{
					att.Damage(5f, caster);
					if (caster.TryGetAttackTag(out var tag, ignoreHasAttack: true))
					{
						NormalItem.TagHit(tag, 5f, caster, att);
					}
					else
					{
						att.Affect(new SlowEffect(2f, 1f));
					}
				}
			});
			yield break;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.HealthMax = 100f;
		HealthRegen = 2f;
		base.Defense = 2f;
		if (Random.value > 0.7f)
		{
			healthDrop = 0;
		}
		abilities.Add(new StompAbility(this));
	}

	public override int GetCost()
	{
		return 12;
	}

	private void MirrorProcessed()
	{
	}
}
