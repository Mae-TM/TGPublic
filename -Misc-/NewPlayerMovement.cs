using UnityEngine;

public class NewPlayerMovement : MonoBehaviour
{
	private Attackable owner;

	private CharacterController characterControl;

	private Vector3 moveDirection;

	public float gravityForce;

	public float jumpSpeed;

	public bool flying;

	private void Start()
	{
		characterControl = GetComponent<CharacterController>();
		owner = GetComponent<Attackable>();
	}

	public void MoveChar(float _horizontal, float _vertical)
	{
		float z = _vertical * owner.Speed;
		float x = _horizontal * owner.Speed;
		moveDirection.x = x;
		moveDirection.z = z;
	}

	public void SendMove()
	{
		if (!(characterControl == null))
		{
			characterControl.enabled = false;
			Planet componentInParent = GetComponentInParent<Planet>();
			if (componentInParent != null)
			{
				base.transform.rotation = Quaternion.FromToRotation(base.transform.up, (base.transform.position - componentInParent.transform.position).normalized) * base.transform.rotation;
				Camera.main.transform.eulerAngles = new Vector3(0f, 0f, base.transform.eulerAngles.z);
			}
			if (Physics.Raycast(base.transform.position, -base.transform.up, 1.2f))
			{
				moveDirection.y = Mathf.Max(moveDirection.y, 0f);
			}
			else
			{
				moveDirection.y -= gravityForce * Time.deltaTime;
			}
			GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + base.transform.TransformDirection(moveDirection) * Time.deltaTime);
		}
	}

	public void Jump()
	{
		if (characterControl.isGrounded || flying)
		{
			moveDirection.y = jumpSpeed;
		}
	}

	public void ToggleFly()
	{
		flying = !flying;
	}
}
