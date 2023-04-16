using System;
using UnityEngine;

public class TumbleweedBehaviour : MonoBehaviour
{
	private Rigidbody body;

	private float orientation;

	private bool isActive;

	private void Awake()
	{
		orientation = UnityEngine.Random.Range(0f, 360f);
		base.transform.Rotate(0f, orientation, 0f);
		body = GetComponent<Rigidbody>();
	}

	private Vector3 GetVelocity()
	{
		float x = Mathf.Sin((orientation - 180f) / 360f * ((float)Math.PI * 2f));
		float z = Mathf.Sin((orientation - 90f) / 360f * ((float)Math.PI * 2f));
		return 2.5f * new Vector3(x, 0f, z);
	}

	private void FixedUpdate()
	{
		if (isActive)
		{
			body.MovePosition(body.position + GetVelocity() * Time.fixedDeltaTime);
		}
	}

	private void OnEnable()
	{
		Animator componentInChildren = GetComponentInChildren<Animator>();
		CapsuleCollider componentInChildren2 = GetComponentInChildren<CapsuleCollider>();
		Vector3 center = componentInChildren2.center;
		if (GetComponentInParent<Planet>() == null)
		{
			isActive = false;
			componentInChildren.speed = 0f;
			componentInChildren2.height = 0.2f;
			center.y = 0.13f;
		}
		else
		{
			isActive = true;
			componentInChildren.speed = 1f;
			componentInChildren2.height = 0.6f;
			center.y = 0.33f;
		}
		componentInChildren2.center = center;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (isActive)
		{
			ContactPoint contact = collision.GetContact(0);
			if (!(contact.normal.y > 0.8f))
			{
				Vector3 velocity = GetVelocity();
				float num = Vector3.Angle(Vector3.Reflect(velocity, contact.normal).normalized, velocity);
				orientation += num;
				base.transform.Rotate(0f, num, 0f);
			}
		}
	}
}
