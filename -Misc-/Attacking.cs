using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRuby.LightningBolt;
using Mirror;
using Mirror.RemoteCalls;
using ProtoBuf;
using UnityEngine;

[ProtoContract(Surrogate = typeof(PBNetworkBehaviour<Attacking>))]
public class Attacking : Attackable
{
	public abstract class Ability
	{
		private static readonly int attackHash = Animator.StringToHash("Attack");

		protected Attacking caster;

		public float cooldown;

		public float maxCooldown;

		public float vimcost;

		public string name;

		public string description;

		public string animation;

		public AudioClip audio;

		public readonly Color color;

		protected bool isOnHit;

		protected readonly Animator animator;

		private WeaponCollision[] weapons;

		public event Action OnExecute;

		protected Ability(Attacking caster, Color? color = null, string name = "", float maxCooldown = 1f, string animation = null, bool isOnHit = false)
		{
			this.caster = caster;
			this.color = color ?? Color.black;
			this.name = name;
			this.maxCooldown = maxCooldown;
			this.animation = animation;
			this.isOnHit = isOnHit;
			animator = caster.animator;
			weapons = caster.GetComponentsInChildren<WeaponCollision>(includeInactive: true);
		}

		public bool Execute(Attackable target = null, Vector3? position = null)
		{
			return caster.DoAbility(this, target, position);
		}

		public bool ExecuteLocally(Attackable target, Vector3? position)
		{
			if (position.HasValue)
			{
				position = caster.RegionChild.Area.transform.TransformPoint(position.Value);
			}
			float multiplier = caster.abilityMultiplier * Mathf.Min(1f, caster.Vim / vimcost);
			try
			{
				if (!Effect(target, position, multiplier))
				{
					return false;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return false;
			}
			cooldown = maxCooldown;
			caster.Vim -= vimcost;
			if (animation != null)
			{
				animator.Play(animation);
				animator.SetTrigger(attackHash);
				caster.StartCoroutine(HandleAnimationEvents(target, position, multiplier));
				if (isOnHit)
				{
					WeaponCollision[] array = weapons;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Set(OnHit, IsPlaying, multiplier);
					}
				}
				else
				{
					WeaponCollision[] array = weapons;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].enabled = false;
					}
				}
			}
			if ((object)audio != null)
			{
				caster.PlayAudio(audio);
			}
			this.OnExecute?.Invoke();
			return true;
		}

		protected virtual bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			return true;
		}

		protected virtual bool OnHit(Attackable target, float multiplier = 1f)
		{
			return false;
		}

		private IEnumerator HandleAnimationEvents(Attackable target, Vector3? position, float multiplier)
		{
			yield return null;
			int layer = Enumerable.Range(0, animator.layerCount).First((int i) => animator.GetCurrentAnimatorStateInfo(i).IsName(animation));
			yield return null;
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
			OnAnimate(currentAnimatorStateInfo);
			yield return new WaitForSeconds(currentAnimatorStateInfo.length);
			foreach (object item in AfterAnimation(target, position, multiplier))
			{
				yield return item;
			}
		}

		protected virtual void OnAnimate(AnimatorStateInfo state)
		{
		}

		protected virtual IEnumerable AfterAnimation(Attackable target, Vector3? position, float multiplier)
		{
			yield break;
		}

		public bool IsAvailable()
		{
			if (cooldown > 0f)
			{
				return false;
			}
			if (animator.layerCount <= 1)
			{
				return true;
			}
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(1);
			if (!currentAnimatorStateInfo.IsName("Normal"))
			{
				return currentAnimatorStateInfo.IsName("Idle");
			}
			return true;
		}

		public bool IsPlaying()
		{
			int layerCount = animator.layerCount;
			for (int i = 0; i < layerCount; i++)
			{
				if (animator.GetCurrentAnimatorStateInfo(i).IsName(animation) || animator.GetCurrentAnimatorStateInfo(i).IsTag("KeepAttack"))
				{
					return true;
				}
			}
			return false;
		}

		public void SetOnHit(bool to)
		{
			isOnHit = to;
		}

		public void Update()
		{
			if (cooldown > 0f)
			{
				cooldown -= Time.deltaTime;
			}
		}
	}

	protected class BasicAttack : Ability
	{
		private static readonly int attackSpeedHash = Animator.StringToHash("AttackSpeed");

		public BasicAttack(Attacking caster, string animation, string name = "", Color? color = null)
			: base(caster, color, name, Mathf.Min(10f, 1f / caster.AttackSpeed), animation, isOnHit: true)
		{
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (caster.bullet != null && (position.HasValue || target != null))
			{
				caster.bullet.target = position ?? target.transform.position;
			}
			return base.Effect(target, position, multiplier);
		}

		protected override bool OnHit(Attackable target, float multiplier = 1f)
		{
			return caster.Attack(target, caster.AttackDamage * multiplier) > 0f;
		}

		protected override void OnAnimate(AnimatorStateInfo state)
		{
			float num = Mathf.Min(10f, 1f / caster.AttackSpeed);
			cooldown = num - (maxCooldown - cooldown);
			maxCooldown = num;
			float num2 = state.length * state.speedMultiplier;
			if (num2 > maxCooldown)
			{
				animator.SetFloat(attackSpeedHash, num2 / maxCooldown);
			}
			else
			{
				animator.SetFloat(attackSpeedHash, 1f);
			}
		}
	}

	protected abstract class ToggledAbility : Ability, IStatusEffect
	{
		private bool active;

		public float EndTime
		{
			get
			{
				if (!active)
				{
					return 0f;
				}
				return float.PositiveInfinity;
			}
		}

		protected ToggledAbility(Attacking caster, Color? color = null, string name = "", float maxCooldown = 1f, string animation = null, bool isOnHit = false)
			: base(caster, color, name, maxCooldown, animation, isOnHit)
		{
		}

		protected sealed override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (active)
			{
				caster.Remove(this);
			}
			else
			{
				caster.Affect(this);
			}
			return true;
		}

		public virtual void Begin(Attackable att)
		{
			active = true;
			caster = (Attacking)att;
		}

		public virtual void Stop(Attackable att)
		{
			active = false;
		}

		public virtual float Update(Attackable att)
		{
			return float.PositiveInfinity;
		}

		public virtual bool AfterAttack(Attack attack)
		{
			return false;
		}

		public virtual bool OnAttack(Attack attack)
		{
			return false;
		}

		public virtual bool OnAttacked(Attack attack)
		{
			return false;
		}
	}

	[SerializeField]
	protected bool hasTagAttack;

	public List<Ability> abilities = new List<Ability>();

	public float abilityMultiplier = 1f;

	[SerializeField]
	protected Bullet bullet;

	private float vim = 1f;

	[SerializeField]
	private float attackDamage;

	[SerializeField]
	private float attackSpeed = 1f;

	public float luck;

	public virtual float Vim
	{
		get
		{
			return vim;
		}
		set
		{
			vim = Mathf.Clamp(value, 0f, VimMax);
		}
	}

	public virtual float VimMax { get; set; } = 1f;


	public virtual float VimRegen { get; set; } = 0.1f;


	public virtual float AttackDamage
	{
		get
		{
			return attackDamage;
		}
		set
		{
			attackDamage = value;
		}
	}

	public virtual float AttackSpeed
	{
		get
		{
			return attackSpeed;
		}
		set
		{
			attackSpeed = value;
		}
	}

	public event Attack.Handler OnAttack;

	public event Attack.Handler AfterAttack;

	protected override void Awake()
	{
		base.Awake();
		OnAttack += base.StatusEffects.OnAttack;
		AfterAttack += base.StatusEffects.AfterAttack;
	}

	public new void LateUpdate()
	{
		foreach (Ability ability in abilities)
		{
			ability.Update();
		}
		base.LateUpdate();
		Vim += VimRegen * Time.deltaTime;
	}

	[Server]
	private bool DoAbility(Ability ability, Attackable target, Vector3? position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean Attacking::DoAbility(Attacking/Ability,Attackable,System.Nullable`1<UnityEngine.Vector3>)' called when server was not active");
			return default(bool);
		}
		if (!ability.ExecuteLocally(target, position))
		{
			return false;
		}
		int index = abilities.IndexOf(ability);
		RpcDoAbility(index, target, position);
		return true;
	}

	[ClientRpc]
	private void RpcDoAbility(int index, Attackable target, Vector3? position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(index);
		writer.WriteNetworkBehaviour(target);
		writer.WriteNullable(position);
		SendRPCInternal(typeof(Attacking), "RpcDoAbility", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public virtual bool IsValidTarget(Attackable target)
	{
		if (!IsEnemy(target))
		{
			return !base.Faction.IsAllyOf(target.Faction);
		}
		return true;
	}

	public float Attack(Attackable target, float damage, bool ranged = false)
	{
		if (!IsValidTarget(target))
		{
			return 0f;
		}
		Attack attack = new Attack
		{
			source = this,
			target = target,
			damage = damage,
			isRanged = ranged,
			CritMultiplier = GetCrit(target)
		};
		if (!ranged && TryGetAttackTag(out var tag))
		{
			NormalItem.TagHit(tag, damage, this, target);
		}
		this.OnAttack?.Invoke(attack);
		target.Damage(attack);
		this.AfterAttack?.Invoke(attack);
		return attack.damage;
	}

	public virtual bool TryGetAttackTag(out NormalItem.Tag tag, bool ignoreHasAttack = false)
	{
		if (!ignoreHasAttack && !hasTagAttack)
		{
			tag = NormalItem.Tag.Count;
			return false;
		}
		tag = NormalItem.GetGristTag(base.GristType);
		return true;
	}

	public float GetCrit(Attackable target)
	{
		float num = luck;
		if (target is Attacking attacking)
		{
			num -= attacking.luck;
		}
		num = 2f / (1f + Mathf.Pow(2f, 0f - num)) - 1f;
		if (num > 0f && UnityEngine.Random.value < num)
		{
			return 2f;
		}
		if (num < 0f && UnityEngine.Random.value < 0f - num)
		{
			return 0.5f;
		}
		return 1f;
	}

	public virtual Bullet GetBullet()
	{
		if (bullet != null)
		{
			NormalItem weapon = bullet.weapon;
			if (weapon != null && weapon.equipSprites.Length > 2)
			{
				bullet.RefreshSprite();
			}
		}
		return bullet;
	}

	public static void ChainAttack(Attackable source, List<Attackable> targets, float damage, float maxDistSqr, float divisionFactor = 2f)
	{
		Attackable attackable = null;
		foreach (Attackable target in targets)
		{
			float sqrMagnitude = (target.transform.position - source.transform.position).sqrMagnitude;
			if (sqrMagnitude < maxDistSqr)
			{
				maxDistSqr = sqrMagnitude;
				attackable = target;
			}
		}
		if (attackable != null)
		{
			Lightning(source.gameObject, attackable.gameObject);
			targets.Remove(attackable);
			attackable.Damage(damage);
			if (damage / divisionFactor > 1f)
			{
				ChainAttack(attackable, targets, damage / divisionFactor, maxDistSqr);
			}
		}
	}

	public static void Lightning(GameObject source, GameObject target, float duration = 1f, Vector3? from = null, Vector3? to = null, float chaosFactor = 0.15f)
	{
		LightningBoltScript lightningBoltScript = UnityEngine.Object.Instantiate(Resources.Load<LightningBoltScript>("LightningBolt"));
		lightningBoltScript.StartObject = source;
		lightningBoltScript.StartPosition = from ?? new Vector3(0f, source.GetComponentInChildren<Collider>().bounds.extents.y, 0f);
		lightningBoltScript.EndObject = target;
		lightningBoltScript.EndPosition = to ?? new Vector3(0f, target.GetComponentInChildren<Collider>().bounds.extents.y, 0f);
		lightningBoltScript.ChaosFactor = chaosFactor;
		UnityEngine.Object.Destroy(lightningBoltScript.gameObject, duration);
	}

	public static void ColoredLightning(GameObject source, GameObject target, Color c, float duration = 1f, Vector3? from = null, Vector3? to = null, float chaosFactor = 0.15f)
	{
		LightningBoltScript lightningBoltScript = UnityEngine.Object.Instantiate(Resources.Load<LightningBoltScript>("LightningBoltColored"));
		ImageEffects.SetShiftColor(lightningBoltScript.GetComponent<LineRenderer>().materials[1], c);
		lightningBoltScript.StartObject = source;
		lightningBoltScript.StartPosition = from ?? new Vector3(0f, source.GetComponentInChildren<Collider>().bounds.extents.y, 0f);
		lightningBoltScript.EndObject = target;
		lightningBoltScript.EndPosition = to ?? new Vector3(0f, target.GetComponentInChildren<Collider>().bounds.extents.y, 0f);
		lightningBoltScript.ChaosFactor = chaosFactor;
		UnityEngine.Object.Destroy(lightningBoltScript.gameObject, duration);
	}

	public static void Explosion(Vector3 position, float damage, float force, Attackable ignore = null, float radius = 3f, bool visual = true)
	{
		if (visual)
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Explosion"));
			UnityEngine.Object.Destroy(obj, 10f);
			obj.transform.position = position;
		}
		HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
		Collider[] array = Physics.OverlapSphere(position, radius);
		foreach (Collider collider in array)
		{
			if (collider.attachedRigidbody != null)
			{
				hashSet.Add(collider.attachedRigidbody);
			}
		}
		foreach (Rigidbody rb in hashSet)
		{
			Attackable component = rb.GetComponent<Attackable>();
			if (component != null)
			{
				if (component == ignore)
				{
					continue;
				}
				component.Damage(damage / Vector3.Distance(position, rb.transform.position));
			}
			ApplyPhysics(rb, delegate
			{
				rb.AddExplosionForce(force, position, radius, 1f, ForceMode.Impulse);
			});
		}
	}

	public static void AddForce(Rigidbody rb, Vector3 force, ForceMode mode = ForceMode.Force)
	{
		ApplyPhysics(rb, delegate
		{
			rb.AddForce(force, mode);
		});
	}

	private static void ApplyPhysics(Rigidbody rb, Action action)
	{
		if ((bool)rb)
		{
			rb.isKinematic = false;
			rb.useGravity = true;
			if (rb.TryGetComponent<Attackable>(out var component))
			{
				component.StartCoroutine(DoDelayed());
			}
			else
			{
				action();
			}
		}
		IEnumerator DoDelayed()
		{
			yield return null;
			action();
		}
	}

	public static IEnumerable<Player> GetPlayers(Vector3 point, float radius)
	{
		radius *= radius;
		foreach (Player item in Player.GetAll())
		{
			if ((item.transform.position - point).sqrMagnitude <= radius)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<Attackable> GetNearby(float radius, bool enemy = true, bool inBattle = true, bool indiscriminate = false)
	{
		return GetNearby(base.transform.position, radius, enemy, inBattle, indiscriminate);
	}

	public IEnumerable<Attackable> GetNearby(Vector3 point, float radius, bool enemy = true, bool inBattle = true, bool indiscriminate = false)
	{
		radius *= radius;
		IEnumerable<Attackable> enumerable;
		if (!inBattle)
		{
			enumerable = ((!indiscriminate) ? (enemy ? base.Faction.GetEnemies() : base.Faction.GetAllies()) : base.Faction.All);
		}
		else
		{
			if (!base.IsInStrife)
			{
				yield break;
			}
			IEnumerable<Attackable> enumerable3;
			if (!indiscriminate)
			{
				IEnumerable<Attackable> enumerable2 = (enemy ? base.Enemies : base.Allies);
				enumerable3 = enumerable2;
			}
			else
			{
				enumerable3 = base.Allies.Concat(base.Enemies);
			}
			enumerable = enumerable3;
		}
		foreach (Attackable item in enumerable)
		{
			if (Vector3.SqrMagnitude(point - item.transform.position) < radius)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<Attackable> GetNearest(float radius, bool enemy = true)
	{
		return GetNearest(base.transform.position, radius, enemy);
	}

	public IEnumerable<Attackable> GetNearest(Vector3 point, float radius, bool enemy = true)
	{
		if (base.IsInStrife)
		{
			return GetNearest(point, radius, enemy ? base.Enemies : base.Allies);
		}
		return Enumerable.Empty<Attackable>();
	}

	public IEnumerable<Attackable> GetNearestAny(float radius)
	{
		return GetNearest(base.transform.position, radius, base.Faction.All);
	}

	public IEnumerable<Player> GetNearestPlayers(float radius)
	{
		return GetNearest(base.transform.position, radius, Player.GetAll());
	}

	public IEnumerable<T> GetNearest<T>(Vector3 point, float radius, IEnumerable<T> options) where T : Component
	{
		PriorityQueue<T> targets = new PriorityQueue<T>();
		radius *= radius;
		foreach (T option in options)
		{
			if (!(option == this))
			{
				float sqrMagnitude = (point - option.transform.position).sqrMagnitude;
				if (sqrMagnitude <= radius)
				{
					targets.Add(option, sqrMagnitude);
				}
			}
		}
		while (targets.Count != 0)
		{
			yield return targets.Pop();
		}
	}

	protected Attackable FindNearestEnemy(float range = float.PositiveInfinity)
	{
		if (!base.IsInStrife)
		{
			return null;
		}
		Attackable result = null;
		range *= range;
		foreach (Attackable enemy in base.Enemies)
		{
			float sqrMagnitude = (base.transform.position - enemy.transform.position).sqrMagnitude;
			if (sqrMagnitude < range)
			{
				result = enemy;
				range = sqrMagnitude;
			}
		}
		return result;
	}

	private void MirrorProcessed()
	{
	}

	private void UserCode_RpcDoAbility(int index, Attackable target, Vector3? position)
	{
		if (!base.isServer)
		{
			abilities[index].ExecuteLocally(target, position);
		}
	}

	protected static void InvokeUserCode_RpcDoAbility(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoAbility called on server.");
		}
		else
		{
			((Attacking)obj).UserCode_RpcDoAbility(reader.ReadInt(), reader.ReadNetworkBehaviour<Attackable>(), reader.ReadNullable());
		}
	}

	static Attacking()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(Attacking), "RpcDoAbility", InvokeUserCode_RpcDoAbility);
	}
}
