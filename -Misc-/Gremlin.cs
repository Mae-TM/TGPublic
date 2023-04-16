using System.Collections;
using UnityEngine;

public class Gremlin : Enemy
{
	private class LungeAbility : Ability
	{
		private const float MINDIST = 16f;

		private bool abort;

		public LungeAbility(Attacking caster)
			: base(caster, null, "Lunge", 4f, "Special", isOnHit: true)
		{
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (target != null)
			{
				return (target.transform.position - caster.transform.position).sqrMagnitude >= 16f;
			}
			return false;
		}

		protected override IEnumerable AfterAnimation(Attackable target, Vector3? position, float multiplier)
		{
			if (target == null)
			{
				yield break;
			}
			float speed = caster.Speed * 5f * multiplier;
			Gremlin gremlin = (Gremlin)caster;
			Rigidbody body = gremlin.body;
			Transform transform = caster.transform;
			Vector3 vector = gremlin.GetAttackPosition(target) - transform.position;
			float num = Mathf.Clamp(vector.magnitude / speed, 0.1f, 5f);
			Vector3 force = vector / num - Physics.gravity * num / 2f;
			Attacking.AddForce(body, force, ForceMode.VelocityChange);
			WaitForFixedUpdate wait = new WaitForFixedUpdate();
			do
			{
				if (target == null)
				{
					yield break;
				}
				Vector3 normalized = (gremlin.GetAttackPosition(target) - transform.position).normalized;
				Vector3 vector2 = Vector3.ProjectOnPlane(speed * normalized - body.velocity, transform.up);
				vector2.Normalize();
				body.AddForce(5f * speed * vector2, ForceMode.Acceleration);
				yield return wait;
			}
			while (!abort && !Mathf.Approximately(body.velocity.y, 0f));
			abort = false;
			body.velocity = Vector3.zero;
		}

		protected override bool OnHit(Attackable target, float multiplier = 1f)
		{
			abort = true;
			return caster.Attack(target, caster.AttackDamage * multiplier) > 0f;
		}
	}

	private class Invisible : Ability
	{
		public Invisible(Attacking caster)
			: base(caster, null, "Disillusion", 8f)
		{
			audio = Resources.Load<AudioClip>("Music/Abilities/Disillusion");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (caster.GetEffect<InvisibleEffect>() != null || caster.Vim < 0.25f)
			{
				return false;
			}
			caster.Affect(new InvisibleEffect(float.PositiveInfinity, 0.15f, 1f, isPerfect: false));
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.HealthMax = 25f;
		HealthRegen = 1f;
		if (Random.value > 0.4f)
		{
			healthDrop = 0;
		}
		abilities.Add(new LungeAbility(this));
		abilities.Add(new Invisible(this));
	}

	public override int GetCost()
	{
		return 2;
	}

	private void MirrorProcessed()
	{
	}
}
