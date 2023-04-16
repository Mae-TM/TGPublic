using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StrifeAI : Attacking
{
	private new class BasicAttack : Attacking.BasicAttack
	{
		private readonly float attackDistance;

		private readonly float precisionFactor;

		public BasicAttack(StrifeAI caster)
			: base(caster, "Attack")
		{
			attackDistance = caster.attackDistance;
			precisionFactor = caster.precisionFactor;
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!NetworkServer.active)
			{
				return base.Effect(target, position, multiplier);
			}
			if (target == null)
			{
				return false;
			}
			Vector3 position2 = caster.transform.position;
			Vector3 cameraDirection = GetCameraDirection(target.transform.position, position2);
			Vector3 vector = target.Collider.ClosestPoint(position2);
			vector.y = position2.y;
			float num = Mathf.Min(attackDistance, Vector3.Dot(position2 - vector, cameraDirection));
			if ((vector + cameraDirection * num - position2).sqrMagnitude > precisionFactor * precisionFactor)
			{
				return false;
			}
			return base.Effect(target, position, multiplier);
		}
	}

	public const float CHASE_RANGE = 20f;

	public const float FIND_RANGE = 10f;

	private static readonly int speedHash = Animator.StringToHash("speed");

	private static readonly int forwardHash = Animator.StringToHash("forward");

	private static readonly int fallingHash = Animator.StringToHash("falling");

	[SerializeField]
	protected AudioClip clip;

	[SerializeField]
	protected float attackDistance = 1f;

	[SerializeField]
	protected float precisionFactor = 0.1f;

	[SerializeField]
	private bool walkAway = true;

	protected Attackable target;

	protected NavMeshAgent navAgent;

	protected Rigidbody body;

	protected static Transform cam;

	protected bool flee;

	public bool pacifist;

	private float lastY;

	public override AudioClip StrifeClip => clip;

	public override float Speed
	{
		set
		{
			navAgent.speed *= value / Speed;
			base.Speed = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		abilities.Add(new BasicAttack(this));
		navAgent = GetComponent<NavMeshAgent>();
	}

	protected virtual void Start()
	{
		int num = Mathf.RoundToInt(AttackDamage * AttackSpeed) * 6 + (int)base.netId % 6;
		num = Mathf.Clamp(100 - num, 0, 99);
		Speed = navAgent.speed;
		navAgent.avoidancePriority = num;
		LocalNavMeshBuilder component = base.transform.root.GetComponent<LocalNavMeshBuilder>();
		if (component != null)
		{
			component.EnableOnMeshBuilt(navAgent);
		}
		else
		{
			navAgent.enabled = true;
		}
		navAgent.updatePosition = base.isServer;
		body = GetComponent<Rigidbody>();
		if (cam == null)
		{
			cam = MSPAOrthoController.main.transform;
		}
		lastY = base.transform.position.y;
		base.OnHurt += OnDamage;
		base.OnAttack += OnHit;
	}

	private void OnTransformParentChanged()
	{
		flee = false;
	}

	protected void Update()
	{
		PreventFloorWarping();
		bool flag = HandleFalling();
		animator.SetBool(fallingHash, flag);
		if (flag)
		{
			return;
		}
		bool isAttacking = !abilities.TrueForAll((Ability a) => !a.IsPlaying());
		UpdateAnimation(isAttacking);
		if (navAgent.isOnNavMesh)
		{
			if (navAgent.remainingDistance <= ((target == null) ? 1f : precisionFactor))
			{
				flee = false;
				navAgent.isStopped = true;
			}
			else
			{
				navAgent.isStopped = false;
			}
			if (!pacifist && !flee)
			{
				HandleStrife(isAttacking);
			}
			if (target == null && !flee)
			{
				HandleIdle();
			}
			else
			{
				navAgent.speed = Speed;
			}
		}
	}

	private void PreventFloorWarping()
	{
		if (!body.isKinematic)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (Mathf.Abs(position.y - lastY) < 3f && navAgent.isOnNavMesh)
		{
			lastY = position.y;
			return;
		}
		Vector3 vector = new Vector3(position.x, lastY, position.z);
		if (NavMesh.SamplePosition(vector, out var hit, 2f, -1))
		{
			navAgent.Warp(hit.position + new Vector3(0f, navAgent.baseOffset, 0f));
			return;
		}
		body.isKinematic = false;
		base.transform.position = vector;
	}

	private bool HandleFalling()
	{
		if (body.isKinematic)
		{
			return false;
		}
		if (navAgent.enabled)
		{
			navAgent.enabled = false;
			return true;
		}
		if (body.velocity.sqrMagnitude > 1E-05f)
		{
			return true;
		}
		if (!NavMesh.SamplePosition(base.transform.TransformPoint(0f, 0f - navAgent.baseOffset, 0f), out var _, navAgent.height, -1))
		{
			return true;
		}
		body.isKinematic = true;
		navAgent.enabled = true;
		lastY = base.transform.position.y;
		return false;
	}

	private void UpdateAnimation(bool isAttacking)
	{
		if (animator.isInitialized)
		{
			Vector3 velocity = navAgent.velocity;
			animator.SetFloat(speedHash, velocity.magnitude);
			Vector3 lhs = velocity;
			if (target != null && isAttacking)
			{
				lhs = target.transform.position - base.transform.position;
			}
			Vector3 localScale = base.transform.localScale;
			float num = Vector3.Dot(lhs, cam.right);
			if (Mathf.Abs(num) > float.Epsilon)
			{
				localScale.x = ((num < 0f) ? Mathf.Abs(localScale.x) : (0f - Mathf.Abs(localScale.x)));
			}
			base.transform.localScale = localScale;
			float num2 = Vector3.Dot(lhs, cam.forward);
			if (Mathf.Abs(num2) > float.Epsilon)
			{
				animator.SetBool(forwardHash, num2 < 0f);
			}
		}
	}

	private void HandleStrife(bool isAttacking)
	{
		if (target == null || !IsValidTarget(target))
		{
			if (base.IsInStrife)
			{
				target = base.Enemies.Find(IsValidTarget);
				if ((object)target == null)
				{
					LeaveStrife();
					return;
				}
			}
			else if (!FindTarget())
			{
				return;
			}
		}
		if ((base.transform.position - target.transform.position).sqrMagnitude > 400f)
		{
			LeaveStrife();
			target = null;
			navAgent.isStopped = true;
		}
		else
		{
			if (isAttacking || abilities.TrueForAll((Ability a) => !a.IsAvailable()))
			{
				return;
			}
			navAgent.SetDestination(GetAttackPosition(target));
			for (int num = abilities.Count - 1; num >= 0; num--)
			{
				if (abilities[num].IsAvailable() && abilities[num].Execute(target))
				{
					EngageStrife(target);
					break;
				}
			}
		}
	}

	private static Vector3 GetCameraDirection(Vector3 from, Vector3 to)
	{
		Vector3 right = cam.right;
		return Mathf.Sign(Vector3.Dot(right, to - from)) * right;
	}

	protected Vector3 GetAttackPosition(Attackable toAttack)
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = toAttack.transform.position;
		Vector3 vector = toAttack.Collider.ClosestPoint(position);
		vector.y = position2.y;
		return vector + GetCameraDirection(position2, position) * attackDistance;
	}

	protected virtual void HandleIdle()
	{
		navAgent.speed = Speed / 3f;
	}

	protected virtual bool FindTarget()
	{
		float num = 100f;
		Transform obj = base.transform;
		Vector3 localPosition = obj.localPosition;
		Transform parent = obj.parent;
		bool result = false;
		foreach (Attackable enemy in base.Faction.GetEnemies())
		{
			Transform transform = enemy.transform;
			if (!(transform.parent != parent))
			{
				float sqrMagnitude = (transform.localPosition - localPosition).sqrMagnitude;
				if (!(sqrMagnitude >= num))
				{
					result = true;
					num = sqrMagnitude;
					target = enemy;
				}
			}
		}
		return result;
	}

	protected Transform FindNearestExit(float mdist)
	{
		mdist *= mdist;
		Transform result = null;
		OffMeshLink[] componentsInChildren = base.transform.parent.GetComponentsInChildren<OffMeshLink>();
		foreach (OffMeshLink offMeshLink in componentsInChildren)
		{
			float sqrMagnitude = (offMeshLink.transform.position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude < mdist)
			{
				mdist = sqrMagnitude;
				result = offMeshLink.endTransform;
			}
		}
		return result;
	}

	protected new void OnCollisionEnter(Collision collision)
	{
		Attackable component = collision.gameObject.GetComponent<Attackable>();
		if (component != null && component == target && !base.IsInStrife)
		{
			EngageStrife(component);
		}
		base.OnCollisionEnter(collision);
	}

	public override void OnStrifeStart()
	{
		List<Attackable> enemies = base.Enemies;
		target = enemies[Random.Range(0, enemies.Count)];
		base.OnStrifeStart();
	}

	private void OnDamage(Attack attack)
	{
		if (attack.source != null && target != attack.source && !flee)
		{
			target = attack.source;
		}
	}

	public virtual bool ApplyFear(Vector3 from, float duration)
	{
		flee = true;
		target = null;
		if (!NavMesh.FindClosestEdge(base.transform.position + duration / navAgent.speed * (base.transform.position - from).normalized, out var hit, navAgent.areaMask))
		{
			return false;
		}
		navAgent.SetDestination(hit.position);
		return true;
	}

	public virtual void ApplyCalm()
	{
		pacifist = true;
		navAgent.destination = base.transform.position;
		navAgent.isStopped = true;
		target = null;
	}

	public virtual void RemoveCalm()
	{
		pacifist = false;
	}

	public void ClearTarget()
	{
		target = null;
	}

	public void ClearTarget(Attackable ifEqualTo)
	{
		if (target == ifEqualTo)
		{
			target = null;
		}
	}

	private void OnHit(Attack attack)
	{
		if (walkAway && !attack.isRanged && navAgent.isOnNavMesh && attack.target != null)
		{
			Vector3 position = base.transform.position;
			navAgent.destination = position + attackDistance * (position - attack.target.transform.position).normalized;
		}
	}

	private void MirrorProcessed()
	{
	}
}
