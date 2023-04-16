using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using ProtoBuf;
using TheGenesisLib.Models;
using UnityEngine;

[RequireComponent(typeof(ParticleManager))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(RegionChild))]
[RequireComponent(typeof(StatusEffects))]
[ProtoContract(Surrogate = typeof(PBNetworkBehaviour<Attackable>))]
public class Attackable : NetworkBehaviour
{
	public delegate void OnDeathHandler();

	protected class StrifeManager
	{
		public List<Attackable>[] participants = new List<Attackable>[2];

		public StrifeManager(Attackable ngood, Attackable nbad)
		{
			participants[0] = new List<Attackable> { ngood };
			participants[1] = new List<Attackable> { nbad };
			for (int i = 0; i < participants.Length; i++)
			{
				foreach (Attackable item in participants[i])
				{
					item.battleSide = i;
					item.strife = this;
					item.OnStrifeStart();
				}
			}
		}

		public void JoinStrife(Attackable attacking, int side)
		{
			if (!participants[side].Contains(attacking))
			{
				participants[side].Add(attacking);
			}
			attacking.battleSide = side;
			attacking.strife = this;
			attacking.OnStrifeStart();
		}

		public void EndStrife()
		{
			List<Attackable>[] array = participants;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Attackable item in array[i])
				{
					if (item != null)
					{
						item.battleSide = -1;
						item.strife = null;
						item.OnStrifeEnd();
					}
				}
			}
		}

		public void LeaveStrife(Attackable a)
		{
			a.strife = null;
			if (participants[a.battleSide] != null)
			{
				participants[a.battleSide].Remove(a);
				if (participants[a.battleSide].Count == 0)
				{
					EndStrife();
				}
			}
		}

		public void Merge(StrifeManager other, bool flipSides = false)
		{
			if (other == null)
			{
				throw new ArgumentNullException();
			}
			if (other == this)
			{
				throw new ArgumentException("Tried to merge strife with self!");
			}
			List<Attackable>[] array;
			if (flipSides)
			{
				participants[0].AddRange(other.participants[1]);
				participants[1].AddRange(other.participants[0]);
				array = other.participants;
				for (int i = 0; i < array.Length; i++)
				{
					foreach (Attackable item in array[i])
					{
						if (item != null)
						{
							item.strife = this;
							item.battleSide = 1 - item.battleSide;
						}
					}
				}
				return;
			}
			participants[0].AddRange(other.participants[0]);
			participants[1].AddRange(other.participants[1]);
			array = other.participants;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Attackable item2 in array[i])
				{
					if (item2 != null)
					{
						item2.strife = this;
					}
				}
			}
		}
	}

	private static GameObject healthCube;

	private static AudioClip[] hitSound;

	protected AudioClip[] specialHitSound;

	private AudioSource audioSource;

	[SerializeField]
	protected string damageAnimation;

	[SyncVar(hook = "OnShieldChange")]
	private float shield;

	[SerializeField]
	protected HealthVial healthVial;

	protected Animator animator;

	[SyncVar(hook = "OnHealthChange")]
	private float health;

	private Collider collider;

	[SerializeField]
	private string[] editorLoot;

	[SerializeField]
	protected int gristDrop;

	[SerializeField]
	protected int healthDrop;

	[SerializeField]
	protected float xpvalue;

	private static readonly int deadHash;

	private StrifeManager strife;

	private int battleSide = -1;

	public virtual float Speed { get; set; } = 1f;


	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			if (base.isActiveAndEnabled)
			{
				Networkhealth = Mathf.Min(value, HealthMax);
			}
		}
	}

	public float HealthMax { get; set; }

	public virtual float HealthRegen { get; set; }

	public float Defense { get; set; }

	public virtual AudioClip StrifeClip => null;

	public Collider Collider => collider ?? (collider = GetComponentInChildren<Collider>());

	protected virtual IEnumerable<Item> Loot
	{
		get
		{
			if (editorLoot != null)
			{
				return from item in ItemDownloader.Instance.GetItems(editorLoot)
					select new NormalItem(item);
			}
			return Enumerable.Empty<Item>();
		}
	}

	public int GristType { get; set; }

	[field: SerializeField]
	[field: HideInInspector]
	public RegionChild RegionChild { get; private set; }

	public Faction.Member Faction { get; }

	[field: SerializeField]
	[field: HideInInspector]
	public StatusEffects StatusEffects { get; private set; }

	public virtual bool IsSavedWithHouse => true;

	public bool IsInStrife => strife != null;

	public List<Attackable> Allies
	{
		get
		{
			if (strife == null)
			{
				return new List<Attackable>(1) { this };
			}
			return strife.participants[battleSide];
		}
	}

	public List<Attackable> Enemies
	{
		get
		{
			if (strife == null)
			{
				return new List<Attackable>(0);
			}
			return strife.participants[1 - battleSide];
		}
	}

	public float Networkshield
	{
		get
		{
			return shield;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref shield))
			{
				float shieldOld = shield;
				SetSyncVar(value, ref shield, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnShieldChange(shieldOld, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public float Networkhealth
	{
		get
		{
			return health;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref health))
			{
				float healthOld = health;
				SetSyncVar(value, ref health, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnHealthChange(healthOld, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public event Attack.Handler OnHurt;

	public event OnDeathHandler OnDeath;

	public Attackable()
	{
		Faction = new Faction.Member(this);
	}

	private void OnValidate()
	{
		if ((object)RegionChild == null)
		{
			RegionChild regionChild = (RegionChild = GetComponent<RegionChild>());
		}
		if ((object)StatusEffects == null)
		{
			StatusEffects statusEffects = (StatusEffects = GetComponent<StatusEffects>());
		}
	}

	protected virtual void OnEnable()
	{
		Faction.Enable();
	}

	protected virtual void OnDisable()
	{
		Faction.Disable();
		LeaveStrife();
	}

	protected virtual void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		if (animator != null)
		{
			DeathBehaviour behaviour = animator.GetBehaviour<DeathBehaviour>();
			if (behaviour != null)
			{
				behaviour.OnPlay += StartDying;
			}
		}
		HealthMax = 25f;
		HealthRegen = 0.5f;
		Health = HealthMax;
		Defense = 0f;
		OnHurt += StatusEffects.OnAttacked;
		if (!TryGetComponent<AudioSource>(out audioSource))
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
		}
		audioSource.volume = 50f;
		audioSource.spatialBlend = 1f;
		if (hitSound == null)
		{
			hitSound = Resources.LoadAll<AudioClip>("Music/hitsound2");
		}
		if (healthVial == null)
		{
			healthVial = HealthVialBasic.Make(base.gameObject);
			Faction.OnParentChange += OnFactionChange;
		}
		if (healthCube == null)
		{
			healthCube = Resources.Load<GameObject>("Prefabs/healthCube");
			NetworkClient.RegisterPrefab(healthCube);
			Grist.RegisterPrefabs();
		}
	}

	private void OnFactionChange(Faction from, Faction to)
	{
		if (to != null && healthVial is HealthVialBasic healthVialBasic)
		{
			healthVialBasic.SetColor(to.color, to.gelColor);
		}
	}

	protected bool IsTooLow()
	{
		Transform transform = base.transform;
		if (transform.localPosition.y < -128f)
		{
			return (object)transform.parent != null;
		}
		return false;
	}

	public void LateUpdate()
	{
		if (base.isServer && (Health <= 0f || IsTooLow()))
		{
			ServerSetDead();
		}
		Health += HealthRegen * Time.deltaTime;
		Networkshield = Mathf.Max(0f, shield - 1f * Time.deltaTime);
		healthVial.Set(health, shield, HealthMax);
	}

	public float Damage(float damage, Attacking assailant = null)
	{
		return Damage(new Attack
		{
			damage = damage,
			source = assailant
		});
	}

	public float Damage(Attack attack)
	{
		if (float.IsNaN(attack.damage) || float.IsNegativeInfinity(attack.damage))
		{
			Debug.LogError($"Tried to deal {attack.damage} damage!");
			return 0f;
		}
		if (!base.isActiveAndEnabled)
		{
			return 0f;
		}
		if (attack.source != null)
		{
			EngageStrife(attack.source);
		}
		if (attack.damage == 0f)
		{
			return 0f;
		}
		PlayAudio((specialHitSound != null) ? specialHitSound[UnityEngine.Random.Range(0, specialHitSound.Length)] : hitSound[UnityEngine.Random.Range(0, hitSound.Length)]);
		if (attack.CritMultiplier != 1f)
		{
			healthVial.ShowCrit(attack.CritMultiplier > 1f);
		}
		attack.target = this;
		this.OnHurt?.Invoke(attack);
		if (Defense >= 0f)
		{
			attack.damage /= Mathf.Sqrt(Defense + 1f);
		}
		else
		{
			attack.damage *= Mathf.Sqrt(1f - Defense);
		}
		Networkshield = shield - attack.damage;
		if (shield < 0f)
		{
			Health += shield;
			Networkshield = 0f;
		}
		if (Health <= 0f)
		{
			if (base.isServer)
			{
				ServerSetDead();
			}
			attack.damage += Health;
			return attack.damage;
		}
		if (!string.IsNullOrEmpty(damageAnimation) && animator != null)
		{
			animator.Play(damageAnimation);
		}
		healthVial.Enable(1f);
		return attack.damage;
	}

	public void PlayAudio(AudioClip clip)
	{
		audioSource.clip = clip;
		audioSource.Play();
	}

	[Command]
	public virtual void SyncedHeal(float value)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(value);
		SendCommandInternal(typeof(Attackable), "SyncedHeal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void AddShield(float value)
	{
		healthVial.Enable(1f);
		Networkshield = shield + value;
	}

	public bool Affect<T>(T effect, bool stacking = true) where T : IStatusEffect
	{
		return StatusEffects.Add(effect, stacking);
	}

	public IStatusEffect GetEffect<T>() where T : IStatusEffect
	{
		return StatusEffects.FirstOrDefault((IStatusEffect e) => e is T);
	}

	public IEnumerable<T> GetEffects<T>() where T : IStatusEffect
	{
		return StatusEffects.GetEffects<T>();
	}

	public bool Remove<T>() where T : IStatusEffect
	{
		return StatusEffects.Remove<T>();
	}

	public void Remove(IStatusEffect effect)
	{
		StatusEffects.Remove(effect);
	}

	private void ServerSetDead(bool dead = true)
	{
		RpcSetDead(dead);
		SetDead(dead);
	}

	public void Kill()
	{
		ServerSetDead();
	}

	[ClientRpc]
	private void RpcSetDead(bool dead)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(dead);
		SendRPCInternal(typeof(Attackable), "RpcSetDead", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void SetDead(bool dead)
	{
		if (!dead)
		{
			base.enabled = true;
		}
		if (animator == null || (dead && IsTooLow()))
		{
			if (dead)
			{
				StartDying();
			}
			return;
		}
		bool logWarnings = animator.logWarnings;
		animator.logWarnings = false;
		if (animator.GetBool(deadHash) != dead)
		{
			animator.SetBool(deadHash, dead);
			if (dead && !animator.GetBool(deadHash))
			{
				StartDying();
			}
		}
		animator.logWarnings = logWarnings;
	}

	private void StartDying(float delay = 0f)
	{
		if (!base.enabled)
		{
			return;
		}
		foreach (Player item in Enemies.OfType<Player>())
		{
			item.Experience += xpvalue;
		}
		this.OnDeath?.Invoke();
		base.enabled = false;
		StartCoroutine(Die(delay));
	}

	protected virtual IEnumerator Die(float delay = 0f)
	{
		if (!base.isServer)
		{
			yield break;
		}
		yield return new WaitForSeconds(delay);
		Bounds bounds = ModelUtility.GetBounds(base.gameObject);
		foreach (Item item in Loot)
		{
			DropLoot(item, bounds);
		}
		while (gristDrop > 0)
		{
			int num = (int)Mathf.Clamp(Mathf.Ceil((float)gristDrop / 4f), 1f, 128f);
			int type = ((UnityEngine.Random.Range(0, 2) != 0) ? GristType : 0);
			Grist.Make(num, type, ModelUtility.RandomInBounds(bounds), RegionChild.Area);
			gristDrop -= num;
		}
		while (healthDrop > 0)
		{
			GameObject obj = UnityEngine.Object.Instantiate(healthCube);
			obj.name = "healthCube";
			obj.GetComponent<RegionChild>().Area = RegionChild.Area;
			obj.transform.position = ModelUtility.RandomInBounds(bounds);
			NetworkServer.Spawn(obj);
			healthDrop--;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected void DropLoot(Item item, Bounds bounds)
	{
		item.PutDown(RegionChild.Area, ModelUtility.RandomInBounds(bounds));
	}

	protected void OnCollisionEnter(Collision collision)
	{
		ItemObject componentInParent = collision.gameObject.GetComponentInParent<ItemObject>();
		if (componentInParent != null && collision.rigidbody != null)
		{
			NormalItem normalItem = componentInParent.Item as NormalItem;
			if (collision.rigidbody.velocity.sqrMagnitude > 15f)
			{
				float damage = ((normalItem != null && normalItem.Power > 0f) ? (2f * normalItem.Power) : 1f);
				if (Damage(damage) > 0f)
				{
					WeaponCollision.ShowHitMarker(collision.contacts[0].thisCollider, collision.contacts[0].point);
				}
			}
			if (normalItem != null && !normalItem.IsWeapon() && normalItem.HasTag(NormalItem.Tag.Explosive))
			{
				Attacking.Explosion(collision.contacts[0].point, normalItem.Power, 10f);
				UnityEngine.Object.Destroy(collision.gameObject);
			}
		}
		Attackable componentInParent2 = collision.gameObject.GetComponentInParent<Attackable>();
		if (componentInParent2 != null)
		{
			IStatusEffect effect = GetEffect<BurningEffect>();
			if (effect != null)
			{
				componentInParent2.Affect(new BurningEffect((BurningEffect)effect), stacking: false);
			}
		}
	}

	public virtual void OnStrifeStart()
	{
		healthVial.Enable();
	}

	public virtual void OnStrifeEnd()
	{
		healthVial.Disable();
	}

	[Client]
	private void ShowHealthVial()
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void Attackable::ShowHealthVial()' called when client was not active");
			return;
		}
		if (!healthVial)
		{
			if (Health >= HealthMax)
			{
				return;
			}
			healthVial = HealthVialBasic.Make(base.gameObject);
		}
		healthVial.Enable(1f);
	}

	private void OnHealthChange(float healthOld, float healthNew)
	{
		ShowHealthVial();
	}

	private void OnShieldChange(float shieldOld, float shieldNew)
	{
		ShowHealthVial();
	}

	protected virtual HouseData.Attackable SaveSpecific()
	{
		return new HouseData.Attackable();
	}

	protected virtual void LoadSpecific(HouseData.Attackable data)
	{
	}

	public HouseData.Attackable Save()
	{
		HouseData.Attackable attackable = SaveSpecific();
		attackable.name = base.name;
		attackable.pos = base.transform.localPosition;
		attackable.health = Health;
		attackable.statusEffects = StatusEffects.Save();
		return attackable;
	}

	public static Attackable Load(HouseData.Attackable data)
	{
		if (!SpawnHelper.instance.TryGetCreature(data.name, out var att))
		{
			return null;
		}
		Attackable attackable = UnityEngine.Object.Instantiate(att);
		attackable.name = att.name;
		attackable.transform.localPosition = data.pos;
		attackable.Health = data.health;
		attackable.StatusEffects.Load(data.statusEffects);
		attackable.LoadSpecific(data);
		return attackable;
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write(ProtobufHelpers.ProtoSerialize(SaveSpecific()));
		}
		return base.OnSerialize(writer, initialState);
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			LoadSpecific(ProtobufHelpers.ProtoDeserialize<HouseData.Attackable>(reader.Read<byte[]>()));
		}
		base.OnDeserialize(reader, initialState);
	}

	public bool IsEnemy(Attackable other)
	{
		if (!IsInStrife || !other.IsInStrife)
		{
			return false;
		}
		if (strife == other.strife)
		{
			return battleSide != other.battleSide;
		}
		return false;
	}

	public void JoinStrife(Attackable other)
	{
		other.strife.JoinStrife(this, other.battleSide);
	}

	protected virtual void EngageStrife(Attackable other)
	{
		if (!other || !other.isActiveAndEnabled)
		{
			return;
		}
		if (strife == null)
		{
			if (other.strife == null)
			{
				new StrifeManager(other, this);
			}
			else
			{
				other.strife.JoinStrife(this, 1 - other.battleSide);
			}
		}
		else if (other.strife == null)
		{
			strife.JoinStrife(other, 1 - battleSide);
		}
		else if (other.strife != strife)
		{
			strife.Merge(other.strife, battleSide == other.battleSide);
		}
	}

	public void LeaveStrife()
	{
		if (strife != null)
		{
			strife.LeaveStrife(this);
		}
	}

	static Attackable()
	{
		deadHash = Animator.StringToHash("Dead");
		RemoteCallHelper.RegisterCommandDelegate(typeof(Attackable), "SyncedHeal", InvokeUserCode_SyncedHeal, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(Attackable), "RpcSetDead", InvokeUserCode_RpcSetDead);
	}

	private void MirrorProcessed()
	{
	}

	public virtual void UserCode_SyncedHeal(float value)
	{
		healthVial.Enable(1f);
		if (Health + value > 0f)
		{
			ServerSetDead(dead: false);
		}
		Health += value;
	}

	protected static void InvokeUserCode_SyncedHeal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command SyncedHeal called on client.");
		}
		else
		{
			((Attackable)obj).UserCode_SyncedHeal(reader.ReadFloat());
		}
	}

	private void UserCode_RpcSetDead(bool dead)
	{
		if (!base.isServer)
		{
			SetDead(dead);
		}
	}

	protected static void InvokeUserCode_RpcSetDead(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetDead called on server.");
		}
		else
		{
			((Attackable)obj).UserCode_RpcSetDead(reader.ReadBool());
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(shield);
			writer.WriteFloat(health);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteFloat(shield);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteFloat(health);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			float num = shield;
			Networkshield = reader.ReadFloat();
			if (!SyncVarEqual(num, ref shield))
			{
				OnShieldChange(num, shield);
			}
			float num2 = health;
			Networkhealth = reader.ReadFloat();
			if (!SyncVarEqual(num2, ref health))
			{
				OnHealthChange(num2, health);
			}
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			float num4 = shield;
			Networkshield = reader.ReadFloat();
			if (!SyncVarEqual(num4, ref shield))
			{
				OnShieldChange(num4, shield);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			float num5 = health;
			Networkhealth = reader.ReadFloat();
			if (!SyncVarEqual(num5, ref health))
			{
				OnHealthChange(num5, health);
			}
		}
	}
}
