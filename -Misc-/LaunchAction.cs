using UnityEngine;

public class LaunchAction : InteractableAction
{
	private AudioClip clip;

	private int active;

	private void Awake()
	{
		active = 0;
		sprite = Resources.Load<Sprite>("Launch");
		desc = "Launch";
	}

	public override void Execute()
	{
		active = 1;
	}

	private void OnTriggerStay(Collider other)
	{
		if (active == 1)
		{
			Rigidbody attachedRigidbody = other.attachedRigidbody;
			if ((bool)attachedRigidbody)
			{
				Attacking.AddForce(attachedRigidbody, new Vector3(0f, 40f, 0f), ForceMode.VelocityChange);
				active = 0;
			}
		}
	}
}
