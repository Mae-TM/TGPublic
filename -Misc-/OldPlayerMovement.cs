using UnityEngine;

public class OldPlayerMovement : MonoBehaviour
{
	public Transform respectedCamera;

	public float movementSpeed = 4f;

	private Vector3 playerMovement;

	private Animator playerAnimator;

	private SpriteRenderer playerSprite;

	public static bool XFlipped;

	private void Awake()
	{
		playerAnimator = GetComponent<Animator>();
		playerSprite = GetComponent<SpriteRenderer>();
	}

	private void FixedUpdate()
	{
		if (KeyboardControl.IsKeyboardBlocked())
		{
			return;
		}
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		if (respectedCamera != null)
		{
			Vector3 normalized = Vector3.Scale(respectedCamera.forward, new Vector3(1f, 0f, 1f)).normalized;
			playerMovement = axis2 * normalized + axis * respectedCamera.right;
		}
		else
		{
			playerMovement = axis2 * Vector3.forward + axis * Vector3.right;
		}
		if (playerMovement.magnitude > 1f)
		{
			playerMovement /= playerMovement.magnitude;
		}
		if (playerSprite != null)
		{
			if (respectedCamera != null)
			{
				Vector3 eulerAngles = playerSprite.transform.rotation.eulerAngles;
				eulerAngles.y = respectedCamera.eulerAngles.y;
				playerSprite.transform.rotation = Quaternion.Euler(eulerAngles);
			}
			if (axis > 0f)
			{
				playerSprite.flipX = false;
				Debug.Log("right");
			}
			else if (axis < 0f)
			{
				playerSprite.flipX = true;
			}
		}
		if (playerAnimator != null)
		{
			playerAnimator.SetFloat("Speed", playerMovement.magnitude);
			if (axis2 < 0f)
			{
				playerAnimator.SetBool("FrontFacing", value: true);
			}
			else if (axis2 > 0f)
			{
				playerAnimator.SetBool("FrontFacing", value: false);
			}
		}
		playerMovement *= movementSpeed;
		base.transform.position += playerMovement * Time.deltaTime;
		Debug.DrawRay(base.transform.position, playerMovement, Color.cyan);
	}
}
