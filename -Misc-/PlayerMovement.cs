using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
	private Player owner;

	private CharacterController controller;

	private Rigidbody body;

	private DisplacementMapFlat planet;

	private Vector3 moveDirection;

	[SerializeField]
	private float terminalVelocity = 10f;

	public float jumpSpeed;

	private bool flying;

	private const float groundingDistance = 0.5f;

	private bool isGrounding;

	private const float groundingGravityScale = 5f;

	private float gravity = Physics.gravity.y;

	private const float m_MaxGravityDelta = 10f;

	private float verticalVelocity;

	private float initialJumpVelocity;

	private bool didJump;

	private float airTime;

	private float fallTime;

	private void Awake()
	{
		owner = GetComponent<Player>();
		body = GetComponent<Rigidbody>();
		controller = GetComponent<CharacterController>();
	}

	private void OnEnable()
	{
		controller.enabled = true;
	}

	private void OnDisable()
	{
		controller.enabled = false;
	}

	private void OnControllerColliderHit(ControllerColliderHit collision)
	{
		AutoPickup component = collision.gameObject.GetComponent<AutoPickup>();
		if (component == null && collision.transform.parent != null)
		{
			component = collision.transform.parent.GetComponent<AutoPickup>();
		}
		if (component != null)
		{
			component.Pickup(owner);
			return;
		}
		if (collision.rigidbody != null)
		{
			Bullet component2 = collision.rigidbody.GetComponent<Bullet>();
			if (component2 != null)
			{
				component2.Collide(owner, owner.Collider, collision.point, collision.normal);
			}
		}
		if (!(owner.sylladex == null) && owner.sylladex.AutoPickup)
		{
			PickupItemAction componentInParent = collision.gameObject.GetComponentInParent<PickupItemAction>();
			if (componentInParent != null && componentInParent.enabled)
			{
				componentInParent.Execute();
			}
		}
	}

	public void MoveChar(float hor, float ver)
	{
		if (base.enabled)
		{
			moveDirection.x = hor * owner.Speed;
			moveDirection.z = ver * owner.Speed;
		}
	}

	public void Move(Vector3 move)
	{
		moveDirection += move;
	}

	private void OnTransformParentChanged()
	{
		if ((bool)base.transform.parent)
		{
			planet = base.transform.parent.parent.GetComponent<DisplacementMapFlat>();
		}
	}

	private void FixedUpdate()
	{
		if (body.isKinematic)
		{
			return;
		}
		if (controller.enabled)
		{
			controller.enabled = false;
			return;
		}
		Vector3 velocity = body.velocity;
		if (!(velocity.y > 0f) && !(velocity.sqrMagnitude > 50f) && body.SweepTest(-Vector3.up, out var _, 0.1f))
		{
			controller.enabled = true;
			body.isKinematic = true;
			body.useGravity = false;
		}
	}

	public void Update()
	{
		if (!body.isKinematic)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (owner.GetArmor(i) != null && owner.GetArmor(i).HasTag(NormalItem.Tag.Trickster))
			{
				moveDirection.x += (Mathf.PerlinNoise(Time.time, 42f) * 2f - 1f) * owner.Speed;
				moveDirection.z += (Mathf.PerlinNoise(Time.time, 21f) * 2f - 1f) * owner.Speed;
				break;
			}
		}
		Vector3 vector = AerialMovement(Time.deltaTime);
		if ((controller.Move(base.transform.TransformDirection(GetOceanFactor() * moveDirection + vector) * Time.deltaTime) & CollisionFlags.Above) == CollisionFlags.Above)
		{
			verticalVelocity = 0f;
			initialJumpVelocity = 0f;
		}
		moveDirection.Set(0f, 0f, 0f);
		if (planet != null)
		{
			Vector3 localPosition = base.transform.localPosition;
			bool flag = false;
			if (Mathf.Abs(localPosition.x) > 475f)
			{
				localPosition.x = Mathf.Repeat(localPosition.x + 475f, 950f) - 475f;
				flag = true;
			}
			if (Mathf.Abs(localPosition.z) > 475f)
			{
				localPosition.z = Mathf.Repeat(localPosition.z + 475f, 950f) - 475f;
				flag = true;
			}
			if (flag)
			{
				base.transform.localPosition = localPosition;
			}
		}
	}

	private float GetOceanFactor()
	{
		if (planet == null)
		{
			return 1f;
		}
		float num = planet.SeaY - base.transform.localPosition.y;
		return Mathf.Clamp01(1f - num);
	}

	private Vector3 AerialMovement(float deltaTime)
	{
		airTime += deltaTime;
		CalculateGravity(deltaTime);
		float b = (isGrounding ? float.NegativeInfinity : (0f - terminalVelocity));
		if (verticalVelocity >= 0f)
		{
			verticalVelocity = Mathf.Max(initialJumpVelocity + gravity * airTime, b);
		}
		float a = fallTime;
		if (verticalVelocity < 0f)
		{
			verticalVelocity = Mathf.Max(gravity * fallTime, b);
			fallTime += deltaTime;
			if (IsGrounded())
			{
				initialJumpVelocity = 0f;
				if (Mathf.Abs(airTime - deltaTime) > Mathf.Epsilon)
				{
					didJump = false;
					isGrounding = false;
					owner.Damage(CalculateFallDamage(fallTime));
				}
				fallTime = 0f;
				airTime = 0f;
				verticalVelocity = -2f;
			}
		}
		if (Mathf.Approximately(a, 0f) && fallTime > Mathf.Epsilon && IsInGroundingDistance())
		{
			isGrounding = true;
		}
		return new Vector3(0f, verticalVelocity, 0f);
	}

	private float CalculateFallDamage(float fallTime)
	{
		float num = 1f;
		float num2 = 5.5f;
		float num3 = Mathf.Clamp(fallTime - num, 0f, float.MaxValue);
		if (num3 > 0f)
		{
			return Mathf.Clamp(Mathf.Pow(1.3f * num3 * num2, 1.5f), 0f, float.MaxValue);
		}
		return 0f;
	}

	private void CalculateGravity(float deltaTime)
	{
		float num;
		if (verticalVelocity < 0f)
		{
			num = 1f;
			if (!didJump && isGrounding)
			{
				num *= 5f;
				gravity = num * Physics.gravity.y;
				return;
			}
		}
		else
		{
			num = 1f;
		}
		float b = num * Physics.gravity.y;
		gravity = Mathf.Lerp(gravity, b, deltaTime * 10f);
	}

	private bool IsInGroundingDistance()
	{
		Bounds bounds = controller.bounds;
		if (Physics.Raycast(new Ray(bounds.center + Vector3.down * bounds.extents.y, -Vector3.up), out var hitInfo, 0.5f))
		{
			return hitInfo.distance <= 0.5f;
		}
		return false;
	}

	public void Jump()
	{
		if (IsGrounded() || flying)
		{
			verticalVelocity = (initialJumpVelocity = jumpSpeed);
			didJump = true;
			if (flying)
			{
				fallTime = 0f;
				airTime = 0f;
			}
		}
	}

	public bool IsGrounded()
	{
		if (body.isKinematic)
		{
			return controller.isGrounded;
		}
		return false;
	}

	public void ToggleFly()
	{
		flying = !flying;
	}
}
