using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
	private static GameObject hitMarker;

	private Func<Attackable, float, bool> onHit;

	private Func<bool> isActive;

	private float multiplier;

	private readonly ICollection<Attackable> attacked = new List<Attackable>();

	private void OnTriggerEnter(Collider collider)
	{
		if (onHit == null || (isActive != null && !isActive()))
		{
			return;
		}
		Attackable componentInParent = collider.GetComponentInParent<Attackable>();
		if (!componentInParent || base.transform.IsChildOf(componentInParent.transform))
		{
			collider.GetComponentInParent<IAttackTrigger>()?.Trigger();
		}
		else if (!attacked.Contains(componentInParent))
		{
			attacked.Add(componentInParent);
			if (onHit(componentInParent, multiplier))
			{
				ShowHitMarker(collider, base.transform.position);
			}
		}
	}

	public void Set(Func<Attackable, float, bool> onHit, Func<bool> isActive, float multiplier)
	{
		this.onHit = onHit;
		this.isActive = isActive;
		this.multiplier = multiplier;
		base.enabled = true;
		attacked.Clear();
	}

	public static void ShowHitMarker(Collider on, Vector3 from)
	{
		if (hitMarker == null)
		{
			hitMarker = Resources.Load<GameObject>("Hitmarker");
		}
		Vector3 vector = on.ClosestPointOnBounds(from);
		vector += Vector3.Project(on.ClosestPointOnBounds(MSPAOrthoController.main.transform.position) - vector, MSPAOrthoController.main.transform.forward);
		UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(hitMarker, vector, Quaternion.identity, on.transform), 0.25f);
	}
}
