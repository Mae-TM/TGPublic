using UnityEngine;

public class Ogre : Enemy
{
	protected override void Start()
	{
		base.HealthMax = 75f;
		HealthRegen = 1f;
		base.Defense = 1f;
		if (Random.value > 0.8f)
		{
			healthDrop = 0;
		}
		base.OnAttack += OnHit;
		base.Start();
	}

	private void OnHit(Attack attack)
	{
		if (attack.target.TryGetComponent<Rigidbody>(out var component))
		{
			Vector3 force = 5f * (attack.target.transform.position - base.transform.position).normalized;
			Attacking.AddForce(component, force, ForceMode.Impulse);
		}
	}

	public override int GetCost()
	{
		return 8;
	}

	private void MirrorProcessed()
	{
	}
}
