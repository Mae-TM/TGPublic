using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using ProtoBuf;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerMovement))]
public class Player : Attacking, ItemAcceptor
{
	private class Block : Ability
	{
		public Block(Attacking ncaster)
			: base(ncaster, Color.cyan, "Abjure", 1.5f, "Heal")
		{
			vimcost = 0.25f;
			description = "Negate most of the damage received during a short timespan.";
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			caster.Defense += 100f * multiplier;
			caster.StartCoroutine(EndEffect(0.5f, multiplier));
			return true;
		}

		private IEnumerator EndEffect(float waitTime, float multiplier)
		{
			yield return new WaitForSeconds(waitTime);
			caster.Defense += -100f * multiplier;
		}
	}

	private class Slide : Ability
	{
		public Slide(Attacking ncaster)
			: base(ncaster, new Color(1f, 0.5f, 0.5f), "Abscond", 0.5f, "Slide")
		{
			vimcost = 0.2f;
			description = "Slide away in the direction you're facing.";
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			animator.SetFloat(Effectiveness, 1f / multiplier);
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class DecreasingStackEffect : IStatusEffect
	{
		public int stacks;

		public List<float> stackTimes;

		protected bool resetStackTimer;

		private Action<int> onIncrease;

		private Action<int> onDecrease;

		private readonly float maxDecayTime;

		[ProtoIgnore]
		public Ability ability;

		public Aspect aspect;

		public float EndTime => float.PositiveInfinity;

		public DecreasingStackEffect(Aspect aspect, Action<int> onIncrease, Action<int> onDecrease, Ability ability = null, float maxDecayTime = 10f)
		{
			this.maxDecayTime = maxDecayTime;
			this.aspect = aspect;
			stacks = 0;
			stackTimes = new List<float>();
			Init(onIncrease, onDecrease, ability);
		}

		public void Init(Action<int> onIncrease, Action<int> onDecrease, Ability ability = null)
		{
			this.onIncrease = onIncrease;
			this.onDecrease = onDecrease;
			this.ability = ability;
			if (stackTimes == null)
			{
				stackTimes = new List<float>();
				stacks = 0;
			}
		}

		public bool AfterAttack(Attack attack)
		{
			if (!attack.WasLethal)
			{
				return false;
			}
			stacks++;
			stackTimes.Add(Mathf.Max(maxDecayTime - Mathf.Log(stacks, 1.5f), 0f));
			resetStackTimer = true;
			onIncrease?.Invoke(stacks);
			return false;
		}

		public void Begin(Attackable att)
		{
		}

		public bool OnAttack(Attack attack)
		{
			return false;
		}

		public bool OnAttacked(Attack attack)
		{
			return false;
		}

		public void Stop(Attackable att)
		{
		}

		public float Update(Attackable att)
		{
			if (resetStackTimer)
			{
				resetStackTimer = false;
				return stackTimes.Last();
			}
			if (stacks <= 0)
			{
				if (stackTimes.Count != 0)
				{
					return stackTimes.Last();
				}
				return 0f;
			}
			stacks--;
			onDecrease?.Invoke(stacks);
			stackTimes.RemoveAt(stackTimes.Count - 1);
			if (stackTimes.Count != 0)
			{
				return stackTimes.Last();
			}
			return 0f;
		}
	}

	public abstract class ClasspectAbility : Ability, IStatusEffect
	{
		protected bool needsTarget;

		protected Func<Attackable, Vector3?, float, bool> effect;

		protected Func<float> onUpdate;

		protected Attack.Handler onAttack;

		protected Attack.Handler afterAttack;

		protected Attack.Handler onAttacked;

		protected StatusEffects.OnAffectHandler onAffect;

		protected Action<int> onStackIncrease;

		protected Action<int> onStackDecrease;

		protected float maxStackDecayTime = 10f;

		public float EndTime => float.PositiveInfinity;

		protected ClasspectAbility(Player caster)
			: base(caster, Classpect.GetColor(caster.classpect.aspect))
		{
			Init();
		}

		private void Init()
		{
			maxCooldown = 4f;
			vimcost = 0.15f;
			animation = "MagicShoot";
			switch (((Player)caster).classpect.role)
			{
			case Class.Knight:
				name = "Weaponize";
				Knight();
				break;
			case Class.Page:
				name = "Provide";
				Page();
				break;
			case Class.Prince:
				name = "Destroy";
				Prince();
				break;
			case Class.Bard:
				name = "Destroy";
				Bard();
				break;
			case Class.Sylph:
				name = "Restore";
				Sylph();
				break;
			case Class.Maid:
				name = "Create";
				Maid();
				break;
			case Class.Witch:
				name = "Manipulate";
				Witch();
				break;
			case Class.Heir:
				name = "Boost";
				Heir();
				break;
			case Class.Thief:
				name = "Steal";
				Thief();
				break;
			case Class.Rogue:
				name = "Share";
				Rogue();
				break;
			case Class.Mage:
				name = "Learn";
				Mage();
				break;
			case Class.Seer:
				name = "Foresee";
				Seer();
				break;
			}
			if (onUpdate != null || onAttack != null || onAttacked != null || afterAttack != null || effect == null)
			{
				caster.Affect(this, stacking: false);
			}
			if (onAffect != null)
			{
				caster.StatusEffects.OnAffect += onAffect;
			}
			if (effect != null)
			{
				caster.abilities.Add(this);
			}
			if (onStackIncrease != null || onStackDecrease != null)
			{
				DecreasingStackEffect decreasingStackEffect = (DecreasingStackEffect)caster.GetEffect<DecreasingStackEffect>();
				if (decreasingStackEffect != null)
				{
					decreasingStackEffect.Init(onStackIncrease, onStackDecrease, this);
				}
				else
				{
					caster.Affect(new DecreasingStackEffect(((Player)caster).classpect.aspect, onStackIncrease, onStackDecrease, this, maxStackDecayTime), stacking: false);
				}
			}
			GetDescription(((Player)caster).classpect, out description);
		}

		private static void GetDescription(Classpect classpect, out string description)
		{
			using StreamReader streamReader = File.OpenText(Application.streamingAssetsPath + "/Aspects/" + classpect.aspect.ToString() + "/abilities.txt");
			for (Class @class = Class.Maid; @class < classpect.role; @class++)
			{
				streamReader.ReadLine();
			}
			description = streamReader.ReadLine();
		}

		protected sealed override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (target != null || !needsTarget)
			{
				return effect(target, position, multiplier);
			}
			return false;
		}

		protected abstract void Knight();

		protected virtual void Page()
		{
			onUpdate = delegate
			{
				foreach (Player player in Attacking.GetPlayers(caster.transform.position, 3f))
				{
					Add(player, 0.75f + 0.5f * ((Player)caster).AbilityPower);
				}
				return 1f;
			};
		}

		protected virtual void Prince()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				Missile.Make(caster, target, 0.4f, Color.red, 0.4f, 0.4f, delegate
				{
					Subtract(target, multiplier * (8f + ((Player)caster).AbilityPower));
				});
				return true;
			};
			needsTarget = true;
		}

		protected virtual void Bard()
		{
			onUpdate = delegate
			{
				foreach (Attackable item in caster.GetNearby(6f))
				{
					Subtract(item, 1f + 0.25f * ((Player)caster).AbilityPower);
				}
				return 1f;
			};
		}

		protected virtual void Sylph()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				ParticleCollision.Ring(caster.gameObject, color, 0.5f, 4f);
				foreach (Player player in Attacking.GetPlayers(caster.transform.position, 4f))
				{
					Add(player, multiplier * (6f + 0.5f * ((Player)caster).AbilityPower));
				}
				return true;
			};
		}

		protected virtual void Maid()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => Add(target, multiplier * (8f + ((Player)caster).AbilityPower)) > 0f;
			needsTarget = true;
		}

		protected virtual void Witch()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => Add(target, multiplier * (target.HealthMax - target.Health) / 2f) > 0f;
			needsTarget = true;
		}

		protected virtual void Heir()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => Add(caster, multiplier * (9f + ((Player)caster).AbilityPower), 2f) > 0f;
		}

		protected virtual void Thief()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float change = Subtract(target, multiplier * 6f);
				if (Math.Abs(change) < 0.01f)
				{
					return false;
				}
				Missile.Make(target, caster, 0.4f, color, 0.2f, 0.8f, delegate
				{
					Add(caster, change * 2f / 3f);
				});
				return true;
			};
			needsTarget = true;
		}

		protected virtual void Rogue()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float change = Subtract(target, multiplier * 6f);
				if (Math.Abs(change) < 0.01f)
				{
					return false;
				}
				Missile.Make(target, caster, 0.4f, color, 0.2f, 0.8f, delegate
				{
					ParticleCollision.Ring(caster.gameObject, color, 0.5f, 4f);
					foreach (Player player in Attacking.GetPlayers(caster.transform.position, 4f))
					{
						Add(player, change / 2f);
					}
				});
				return true;
			};
			needsTarget = true;
		}

		protected virtual void Mage()
		{
			onStackIncrease = delegate
			{
				Add(caster, 1f);
			};
			onStackDecrease = delegate
			{
				Subtract(caster, 1f);
			};
		}

		protected virtual void Seer()
		{
			onUpdate = delegate
			{
				float value = NetcodeManager.rng.value;
				float value2 = NetcodeManager.rng.value;
				float value3 = NetcodeManager.rng.value;
				if (!Physics.Raycast(caster.transform.position, new Vector3(value - 0.5f, (0f - value2) / 2f, value3 - 0.5f), out var hitInfo, 16f))
				{
					return 0f;
				}
				if (hitInfo.collider.transform.IsChildOf(caster.transform))
				{
					return 0f;
				}
				ParticleCollision.Spot(hitInfo.point, caster.transform.rotation, Classpect.GetIcon(((Player)caster).classpect.aspect), 4f, caster.Allies, delegate(Attackable att)
				{
					Add(att, 10f);
				});
				return 8f;
			};
		}

		protected virtual float Add(Attackable target, float delta, float duration = 4f)
		{
			return 0f;
		}

		protected virtual float Subtract(Attackable target, float delta, float duration = 4f)
		{
			return 0f;
		}

		public void Begin(Attackable att)
		{
		}

		public void Stop(Attackable att)
		{
			if (onAffect != null)
			{
				caster.StatusEffects.OnAffect -= onAffect;
			}
		}

		public float Update(Attackable att)
		{
			if (caster != null)
			{
				return onUpdate?.Invoke() ?? float.PositiveInfinity;
			}
			caster = att as Attacking;
			Init();
			return onUpdate?.Invoke() ?? float.PositiveInfinity;
		}

		public bool OnAttack(Attack attack)
		{
			if (caster == null)
			{
				caster = attack.source;
				Init();
			}
			onAttack?.Invoke(attack);
			return false;
		}

		public bool OnAttacked(Attack attack)
		{
			if (caster == null)
			{
				caster = attack.target as Attacking;
				Init();
			}
			if (attack.target != null)
			{
				onAttacked?.Invoke(attack);
			}
			return false;
		}

		public bool AfterAttack(Attack attack)
		{
			afterAttack?.Invoke(attack);
			return false;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class SpaceAbility : ClasspectAbility
	{
		[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
		private class ResizeEffect : StatusEffect
		{
			private readonly float factor;

			private float deltaDamage;

			public ResizeEffect(float duration, float factor)
				: base(duration)
			{
				this.factor = factor;
			}

			public override void Begin(Attackable att)
			{
				att.transform.localScale *= factor;
				Attacking attacking = att as Attacking;
				if (attacking != null)
				{
					deltaDamage = attacking.AttackDamage * (factor - 1f);
					attacking.AttackDamage += deltaDamage;
				}
				att.Defense += factor;
			}

			public override void End(Attackable att)
			{
				att.transform.localScale /= factor;
				if (att is Attacking attacking)
				{
					attacking.AttackDamage -= deltaDamage;
				}
				att.Defense -= factor;
			}
		}

		public SpaceAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextHit(4f, delegate(Attack attack)
			{
				attack.damage += multiplier * 2f * ((Player)caster).AbilityPower;
			}));
		}

		protected override void Page()
		{
			onUpdate = delegate
			{
				foreach (Player player in Attacking.GetPlayers(caster.transform.position, 3f))
				{
					Add(player, 1.25f);
				}
				return 1f;
			};
		}

		protected override void Prince()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				if (!position.HasValue)
				{
					return false;
				}
				float num = multiplier * 6f;
				Vector3 position2 = caster.transform.position;
				HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
				RaycastHit[] array = Physics.RaycastAll(position2, position.Value - position2, num);
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit raycastHit = array[i];
					if (raycastHit.rigidbody != null)
					{
						hashSet.Add(raycastHit.rigidbody);
					}
				}
				foreach (Rigidbody item in hashSet)
				{
					Attacking.AddForce(item, 2f * (position2 - item.position), ForceMode.VelocityChange);
				}
				ParticleCollision.Ring(caster.gameObject, color, 0.25f, num, 0f, position.Value - position2, null, reverse: true);
				return true;
			};
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				caster.Affect(new APBoost(float.PositiveInfinity, 0.5f));
			};
			onStackDecrease = delegate
			{
				APBoost aPBoost = caster.GetEffects<APBoost>().FirstOrDefault((APBoost e) => e.EndTime >= float.PositiveInfinity);
				if (aPBoost != null)
				{
					caster.Remove(aPBoost);
				}
			};
		}

		protected override void Bard()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float radius = 4f * multiplier;
				Vector3 position2 = caster.transform.position;
				HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
				Collider[] array = Physics.OverlapSphere(position2, radius);
				foreach (Collider collider in array)
				{
					if (collider.attachedRigidbody != null && collider.attachedRigidbody.gameObject != caster.gameObject)
					{
						hashSet.Add(collider.attachedRigidbody);
					}
				}
				foreach (Rigidbody item in hashSet)
				{
					Attacking.AddForce(item, position2 - item.position, ForceMode.VelocityChange);
					Attackable component = item.GetComponent<Attackable>();
					if (component != null)
					{
						component.Affect(new SlowEffect(1f, 99f));
					}
				}
				ParticleCollision.Ring(caster.gameObject, color, 0.25f, radius, 360f, Vector3.zero, null, reverse: true);
				return true;
			};
		}

		protected override void Maid()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => target.Affect(new ResizeEffect(4f, Mathf.Pow(1.5f, multiplier)), stacking: false);
			needsTarget = true;
		}

		protected override void Witch()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => target.Affect(new ResizeEffect(4f, Mathf.Pow(2f, 0f - multiplier)), stacking: false);
			needsTarget = true;
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Player))
			{
				return 0f;
			}
			target.Affect(new APBoost(duration, delta));
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			if (target is Player player)
			{
				delta = Mathf.Min(delta, player.AbilityPower);
				player.Affect(new APBoost(duration, 0f - delta));
				return delta;
			}
			if (!(target is Attacking attacking))
			{
				return 0f;
			}
			delta = Mathf.Min(delta, attacking.AttackDamage);
			attacking.Affect(new AttackBoost(duration, 0f - delta));
			return delta;
		}
	}

	private class Teleport : Ability
	{
		public Teleport(Attacking ncaster)
			: base(ncaster, new Color(1f, 0.9f, 0f), "Appear", 4f)
		{
			vimcost = 0.2f;
			description = "Teleport towards your mouse position.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Appear");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			Transform transform = caster.transform;
			ImageEffects.ShadowImage(transform, Resources.Load<Sprite>("Effect/Space"), 0.7f, 0.15f);
			float maxLength = 16f * multiplier;
			Vector3 vector = Vector3.ProjectOnPlane(position.Value - transform.position, transform.up);
			vector = Vector3.ClampMagnitude(vector, maxLength);
			if (caster.GetComponent<Rigidbody>().SweepTest(vector, out var hitInfo, vector.magnitude))
			{
				vector = vector.normalized * hitInfo.distance;
			}
			transform.Translate(vector);
			return true;
		}
	}

	private class Push : Ability
	{
		private readonly float radius;

		private readonly float damage;

		private readonly Sprite[] sprites;

		private readonly Sprite background;

		public Push(Attacking caster, float radius, float damage)
			: base(caster, Color.black, "PUSH", 6f, "MagicCharge")
		{
			this.radius = radius;
			this.damage = damage;
			vimcost = 0.5f;
			description = "Push all nearby enemies back.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Push");
			background = Resources.Load<Sprite>("Effect/Space");
			Sprite[] array = Resources.LoadAll<Sprite>("Push");
			sprites = new Sprite[array.Length + 4];
			for (int i = 0; i <= 8; i++)
			{
				sprites[i] = array[Math.Min(i, 4)];
			}
			for (int j = 9; j < sprites.Length; j++)
			{
				sprites[j] = array[j - 4];
			}
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			AbilityAnimator abilityAnimator = AbilityAnimator.Make(sprites, caster.transform, radius);
			abilityAnimator.SetEvent(7, delegate
			{
				Attacking.Explosion(caster.transform.position, damage * multiplier, 10f * multiplier, caster, radius, visual: false);
			});
			abilityAnimator.AnimateMask(background);
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class TimeAbility : ClasspectAbility
	{
		public TimeAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextHit(4f, delegate(Attack attack)
			{
				attack.target.Affect(new TimeEffect(4f, Mathf.Pow(1.5f, 0f - multiplier)));
			}));
		}

		protected override void Bard()
		{
			onAttacked = delegate(Attack attack)
			{
				attack.source.Affect(new AttackSpeedBoost(0.5f, 0.5f));
			};
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				ParticleCollision.Add(ParticleCollision.Ring(caster.gameObject, color, 0.5f, 3f), delegate
				{
					target.Affect(new AttackSpeedBoost(1f, Mathf.Pow(2f, 0f - multiplier)));
				});
				return true;
			};
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				caster.Affect(new AttackSpeedBoost(float.PositiveInfinity, 1.05f));
			};
			onStackDecrease = delegate
			{
				AttackSpeedBoost attackSpeedBoost = caster.GetEffects<AttackSpeedBoost>().FirstOrDefault((AttackSpeedBoost e) => e.EndTime >= float.PositiveInfinity);
				if (attackSpeedBoost != null)
				{
					caster.Remove(attackSpeedBoost);
				}
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking))
			{
				return 0f;
			}
			target.Affect(new AttackSpeedBoost(duration, Mathf.Pow(2f, delta / 10f)));
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking))
			{
				return 0f;
			}
			target.Affect(new AttackSpeedBoost(duration, Mathf.Pow(2f, (0f - delta) / 10f)));
			return delta;
		}
	}

	private class Speedify : Ability
	{
		private readonly float factor;

		private readonly float duration;

		public Speedify(Attacking caster, float factor, float duration)
			: base(caster, Color.red, "Time Distortion", 8f)
		{
			this.factor = factor;
			this.duration = duration;
			description = "Speed up time for yourself.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Time_Distortion");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			caster.Affect(new TimeEffect(duration, Mathf.Pow(factor, multiplier)));
			return true;
		}
	}

	private class Clone : Ability
	{
		private readonly Sprite[] gears = Resources.LoadAll<Sprite>("Rising Gears");

		private Player clone;

		public Clone(Attacking caster)
			: base(caster, Color.red, "Clone", 12f)
		{
			description = "Summon a clone from a doomed timeline.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Clone");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			if (clone != null && !clone.Equals(null))
			{
				GameObject gameObject = clone.gameObject;
				ImageEffects.FadingShadow fadingShadow = gameObject.AddComponent<ImageEffects.FadingShadow>();
				fadingShadow.sprites = gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
				fadingShadow.delta = 0.5f;
				fadingShadow.color = Color.black;
				fadingShadow.color.a = 0.5f;
				UnityEngine.Object.Destroy(gameObject);
			}
			AbilityAnimator abilityAnimator = AbilityAnimator.Make(gears, position.Value, 1f, 0.01f);
			abilityAnimator.pingPong = true;
			abilityAnimator.SetEndEvent(delegate
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(NetcodeManager.Instance.playerPrefab, caster.transform.parent, worldPositionStays: true);
				PlayerSync component = gameObject2.GetComponent<PlayerSync>();
				NetworkPlayer np = ((Player)caster).sync.np;
				component.np.character = np.character;
				component.np.name = np.name;
				component.np.id = np.id + 144;
				component.ApplyLooks();
				UnityEngine.Object.Destroy(gameObject2.gameObject.GetComponent<AudioListener>());
				UnityEngine.Object.Destroy(component);
				clone = gameObject2.GetComponent<Player>();
				clone.SetPosition(position.Value);
				for (int i = 0; i < 5; i++)
				{
					clone.SetArmor(i, ((Player)caster).GetArmor(i));
				}
				clone.Health *= multiplier / 2f;
				clone.HealthMax *= multiplier / 2f;
				NetworkServer.Spawn(gameObject2);
				foreach (Attackable enemy in caster.Enemies)
				{
					enemy.Damage(1f, clone);
				}
			});
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class LightAbility : ClasspectAbility
	{
		public LightAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextHit(4f, delegate(Attack attack)
			{
				attack.CritMultiplier = Mathf.Pow(2f, multiplier);
			}));
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float num = 10f * multiplier;
				ParticleCollision.Ring(caster.gameObject, new Color(0f, 0.96f, 0.96f), 0.5f, num);
				Story componentInParent = caster.transform.GetComponentInParent<Story>();
				if (componentInParent == null)
				{
					return false;
				}
				Vector3 localPosition = caster.transform.localPosition;
				Furniture[] componentsInChildren = componentInParent.GetComponentsInChildren<Furniture>();
				foreach (Furniture furniture in componentsInChildren)
				{
					if (!((double)(furniture.transform.localPosition - localPosition).sqrMagnitude >= Math.Pow(num, 2.0)) && !(furniture == null) && (!(furniture.GetComponentInChildren<Container>() == null) || !(furniture.GetComponent<DungeonEntrance>() == null)) && furniture.TryGetComponent<RegionChild>(out var component))
					{
						component.enabled = false;
						Visibility.Set(component.gameObject, value: true);
						furniture.StartCoroutine(ResetVisibility(component, 4f));
					}
				}
				return true;
			};
		}

		public static IEnumerator ResetVisibility(RegionChild vis, float delay)
		{
			yield return new WaitForSeconds(delay);
			vis.enabled = true;
			vis.SetRegion();
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				caster.Affect(new LuckBoost(float.PositiveInfinity, 0.05f));
			};
			onStackDecrease = delegate
			{
				LuckBoost luckBoost = caster.GetEffects<LuckBoost>().FirstOrDefault((LuckBoost e) => e.EndTime >= float.PositiveInfinity);
				if (luckBoost != null)
				{
					caster.Remove(luckBoost);
				}
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking))
			{
				return 0f;
			}
			target.Affect(new LuckBoost(duration, delta / 10f));
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking))
			{
				return 0f;
			}
			target.Affect(new LuckBoost(duration, (0f - delta) / 10f));
			return delta;
		}
	}

	private class Stun : Ability
	{
		private readonly float radius;

		private readonly float damage;

		private readonly float duration;

		private readonly float castDuration;

		private readonly float angle;

		private readonly Sprite[] sprites;

		public Stun(Attacking ncaster, string name, float radius, float damage, float duration, Color color, float angle = 360f, float castDuration = 0.5f)
			: base(ncaster, color, name, 4f, "MagicShoot")
		{
			this.radius = radius;
			this.damage = damage;
			this.duration = duration;
			this.castDuration = castDuration;
			this.angle = angle;
			vimcost = 0.5f;
			if (angle == 360f)
			{
				description = "Blind all nearby enemies.";
				sprites = Resources.LoadAll<Sprite>("Photorefractive Keractectomy");
			}
			else
			{
				description = "Stun enemies in a cone.";
			}
			audio = Resources.Load<AudioClip>("Music/Abilities/Abey");
		}

		protected override bool Effect(Attackable att = null, Vector3? position = null, float multiplier = 1f)
		{
			if (sprites != null)
			{
				Vector3 pos = ((caster is Player player) ? player.GetPosition() : caster.transform.position);
				AbilityAnimator.Make(sprites, pos, radius).SetEvent(10, MakeParticles);
			}
			else
			{
				MakeParticles();
			}
			return true;
			void MakeParticles()
			{
				Vector3 vector = (position - caster.transform.position) ?? Vector3.forward;
				vector = Vector3.ProjectOnPlane(vector, Vector3.up);
				ParticleCollision.Add(ParticleCollision.Ring(caster.gameObject, color, castDuration, radius, angle, vector), delegate(Attackable target)
				{
					if (target != caster)
					{
						target.Affect(new SlowEffect(duration * multiplier, 99f));
						target.Damage(damage, caster);
					}
				});
			}
		}
	}

	private class Laser : Ability
	{
		private Sprite[] sprites;

		private const float SPEED = 25f;

		private const float DAMAGE = 10f;

		public Laser(Attacking caster)
			: base(caster, Color.yellow, "Radioactive Photonic Emission", 5f, "MagicShoot")
		{
			sprites = Resources.LoadAll<Sprite>("Radioactive Photonic Emission");
			vimcost = 0.25f;
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			ISet<NormalItem.Tag> tags = new HashSet<NormalItem.Tag> { NormalItem.Tag.Piercing };
			Bullet.Make(caster, sprites, 25f * multiplier, 10f * multiplier, position.Value, tags);
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class VoidAbility : ClasspectAbility
	{
		private class AttackedEffect : StatusEffect
		{
			public AttackedEffect(float duration = float.PositiveInfinity)
				: base(duration)
			{
			}
		}

		public VoidAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new AfterNextHit(4f, delegate(Attack attack)
			{
				if (attack.WasLethal)
				{
					cooldown -= maxCooldown / Mathf.Pow(2f, multiplier);
				}
			}, delegate(Attack attack)
			{
				attack.damage += ((Player)caster).AbilityPower * multiplier;
			}));
		}

		protected override void Page()
		{
			afterAttack = delegate(Attack attack)
			{
				if (attack.WasLethal)
				{
					Add(caster, 4f);
					Player player = caster.GetNearestPlayers(8f).FirstOrDefault();
					if ((object)player != null)
					{
						Add(player, 4f);
					}
				}
			};
		}

		protected override void Prince()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				if (!position.HasValue)
				{
					return false;
				}
				float num = 6f * multiplier;
				Vector3 position2 = caster.transform.position;
				HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
				Vector3 vector = position.Value - position2;
				RaycastHit[] array = Physics.RaycastAll(position2, vector, num);
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit raycastHit = array[i];
					if (raycastHit.rigidbody != null)
					{
						hashSet.Add(raycastHit.rigidbody);
					}
				}
				foreach (Rigidbody item in hashSet)
				{
					Vector3 vector2 = item.position - position2;
					Attacking.AddForce(item, vector2.normalized * (2f * (num - vector2.magnitude)), ForceMode.VelocityChange);
				}
				ParticleCollision.Ring(caster.gameObject, color, 0.25f, num, 0f, vector);
				return true;
			};
		}

		protected override void Bard()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float num = 4f * multiplier;
				Vector3 position2 = caster.transform.position;
				HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
				Collider[] array = Physics.OverlapSphere(position2, num);
				foreach (Collider collider in array)
				{
					if (collider.attachedRigidbody != null)
					{
						hashSet.Add(collider.attachedRigidbody);
					}
				}
				foreach (Rigidbody item in hashSet)
				{
					Vector3 vector = item.position - position2;
					Attacking.AddForce(item, vector.normalized * (2f * (num - vector.magnitude)), ForceMode.VelocityChange);
				}
				ParticleCollision.Ring(caster.gameObject, color, 0.25f, num);
				return true;
			};
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				target.Affect(new InvisibleEffect(multiplier * 2f));
				target.Affect(new CalmEffect(multiplier * 2f));
				return true;
			};
			maxCooldown = 5f;
			needsTarget = true;
		}

		protected override void Thief()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextHit(4f, delegate(Attack attack)
			{
				attack.damage += ((Player)caster).AbilityPower * multiplier;
				if (target.GetEffect<AttackedEffect>() == null)
				{
					target.Affect(new AttackedEffect());
					cooldown -= maxCooldown / Mathf.Pow(2f, multiplier);
				}
			}));
		}

		protected override void Rogue()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				Missile.Make(caster, target, 0.2f, color, multiplier * 0.4f, 0.4f, delegate
				{
					target.Damage(4f + ((Player)caster).AbilityPower, caster);
					if (!(target.Health > 0f))
					{
						Missile.Make(target, caster, 0.3f, color, multiplier * 0.4f, 0.4f, delegate
						{
							ParticleCollision.Ring(caster.gameObject, color, 0.4f, 6f);
							foreach (Player player in Attacking.GetPlayers(caster.transform.position, 6f))
							{
								Add(player, 4f);
							}
						});
					}
				});
				return true;
			};
			needsTarget = true;
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				Add(caster, 2f);
			};
			onStackDecrease = delegate
			{
				Subtract(caster, 2f);
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking attacking))
			{
				return 0f;
			}
			foreach (Ability ability in attacking.abilities)
			{
				ability.cooldown = Mathf.Max(0f, ability.cooldown - delta / 4f);
			}
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking))
			{
				return 0f;
			}
			foreach (Ability ability in ((Attacking)target).abilities)
			{
				ability.cooldown = Mathf.Min(ability.maxCooldown, ability.cooldown + delta / 4f);
			}
			return delta;
		}
	}

	private class RandomShoot : Ability
	{
		private readonly float[] damage;

		private readonly GameObject[] prefab;

		public RandomShoot(Attacking caster, float[] damage)
			: base(caster, new Color(1f, 0.9f, 0f), "Tarot Card", 1f, "MagicShoot")
		{
			this.damage = damage;
			prefab = new GameObject[3]
			{
				ItemDownloader.GetPrefab("GenericObject"),
				Resources.Load<GameObject>("Prefabs/Pumpkin"),
				Resources.Load<GameObject>("Prefabs/Rock")
			};
			vimcost = 0.3f;
			description = "Fire a random projectile.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Tarot_Card");
		}

		protected override bool Effect(Attackable att = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			int num = NetcodeManager.rng.Next(Mathf.Min(damage.Length, prefab.Length));
			float num2 = ((num == 2) ? 12f : 20f) * multiplier;
			int num3 = ((num != 0) ? 1 : 3);
			Debug.Log(num2);
			Collider[] array = new Collider[num3];
			for (int i = 0; i < num3; i++)
			{
				array[i] = Bullet.Make(caster, prefab[num], num2, damage[num] * multiplier, position.Value);
				for (int j = 0; j < i; j++)
				{
					Physics.IgnoreCollision(array[i], array[j]);
				}
			}
			return true;
		}
	}

	private class Invisible : Ability
	{
		private readonly float cost;

		private readonly float speedBoost;

		public Invisible(Attacking caster, float cost, float speedBoost)
			: base(caster, Classpect.GetColor(Aspect.Void), "Disillusion", 8f)
		{
			this.cost = cost;
			this.speedBoost = speedBoost;
			description = "Go invisible.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Disillusion");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!caster.Remove<InvisibleEffect>())
			{
				caster.Affect(new InvisibleEffect(float.PositiveInfinity, cost, speedBoost));
			}
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class MindAbility : ClasspectAbility
	{
		public MindAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new AfterNextHit(4f, delegate(Attack attack)
			{
				if (attack.WasLethal)
				{
					caster.Vim += 0.3f * multiplier;
				}
			}, delegate(Attack attack)
			{
				attack.damage += ((Player)caster).AbilityPower * multiplier;
			}));
		}

		protected override void Prince()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => target.Affect(new SlowEffect(multiplier, 99f));
			needsTarget = true;
		}

		protected override void Bard()
		{
			onUpdate = () => (!caster.Affect(new OnNextHit(float.PositiveInfinity, delegate(Attack attack)
			{
				attack.target.Affect(new SlowEffect(1f, 99f));
			}), stacking: false)) ? 0f : 10f;
		}

		protected override void Sylph()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				ParticleCollision.Ring(caster.gameObject, color, 0.5f, 4f);
				foreach (Player player in Attacking.GetPlayers(caster.transform.position, 4f))
				{
					player.Affect(new VimRegenBoost(2f, multiplier * 0.5f));
				}
				return true;
			};
		}

		protected override void Maid()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => target.Affect(new APBoost(4f, multiplier * ((Player)caster).AbilityPower));
			needsTarget = true;
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float num = multiplier;
				while (num >= 1f && (target.Remove<SlowEffect>() || target.Remove<TauntEffect>()))
				{
					num -= 1f;
				}
				bool result = Math.Abs(num - multiplier) > 0.01f;
				if (num <= 0f)
				{
					return result;
				}
				IStatusEffect statusEffect = target.GetEffect<SlowEffect>() ?? target.GetEffect<TauntEffect>();
				if (statusEffect == null)
				{
					return result;
				}
				((StatusEffect)statusEffect).ReduceTime(1f / (1f - num));
				return true;
			};
			needsTarget = true;
		}

		protected override void Heir()
		{
			onAffect = delegate(ref IStatusEffect effect)
			{
				if (effect is SlowEffect || effect is TauntEffect)
				{
					((StatusEffect)effect).ReduceTime(1.25f);
				}
			};
		}

		protected override void Thief()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				target.Affect(new FrenzyEffect(3f * multiplier), stacking: false);
				return true;
			};
			needsTarget = true;
		}

		protected override void Rogue()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				StrifeAI strifeAI = target as StrifeAI;
				if (strifeAI == null)
				{
					return false;
				}
				strifeAI.Affect(new CalmEffect(2f * multiplier), stacking: false);
				return true;
			};
			needsTarget = true;
		}

		protected override void Mage()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				foreach (Attackable item in caster.GetNearby(3f))
				{
					item.Affect(new OnNextHit(2f * multiplier, delegate(Attack attack)
					{
						attack.damage = 0f;
					}));
				}
				return true;
			};
		}

		protected override void Seer()
		{
			onUpdate = delegate
			{
				if (Protect(caster))
				{
					return 10f;
				}
				Player player = caster.GetNearestPlayers(8f).FirstOrDefault();
				if ((object)player == null)
				{
					return 0f;
				}
				Protect(player);
				return 10f;
			};
			static bool Protect(Attackable target)
			{
				return target.Affect(new OnNextAttacked(float.PositiveInfinity, delegate(Attack attack)
				{
					attack.damage = 0f;
				}), stacking: false);
			}
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking))
			{
				return 0f;
			}
			Attacking attacking = (Attacking)target;
			delta = Mathf.Min(delta, attacking.VimMax - attacking.Vim);
			attacking.Vim += delta;
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking attacking))
			{
				return 0f;
			}
			delta = Mathf.Min(delta, attacking.Vim);
			attacking.Vim -= delta;
			return delta;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class HeartAbility : ClasspectAbility
	{
		private int kills;

		public HeartAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Prince()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				if (!position.HasValue)
				{
					return false;
				}
				Vector3 position2 = caster.transform.position;
				HashSet<GameObject> hashSet = new HashSet<GameObject>();
				RaycastHit[] array = Physics.RaycastAll(position2, position.Value - position2, 6f);
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit raycastHit = array[i];
					if (raycastHit.rigidbody != null)
					{
						hashSet.Add(raycastHit.transform.gameObject);
					}
				}
				foreach (Attackable item in from rb in hashSet.TakeWhile((GameObject rb) => caster.IsInStrife)
					select caster.Enemies.FirstOrDefault((Attackable x) => x.gameObject.Equals(rb)) into a
					where a != null
					select a)
				{
					item.Damage(5f * multiplier, caster);
				}
				GameObject gameObject = ((hashSet.Count > 0) ? hashSet.ElementAt(hashSet.Count - 1) : null);
				if (gameObject != null)
				{
					Attacking.ColoredLightning(caster.gameObject, gameObject, new Color(1f, 0.78f, 0.91f));
				}
				return gameObject == null;
			};
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				caster.Affect(new WeakenEffect(float.PositiveInfinity, 0f, -1f));
			};
			onStackDecrease = delegate
			{
				WeakenEffect weakenEffect = caster.GetEffects<WeakenEffect>().FirstOrDefault((WeakenEffect e) => e.EndTime >= float.PositiveInfinity);
				if (weakenEffect != null)
				{
					caster.Remove(weakenEffect);
				}
			};
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new AfterNextHit(4f, delegate(Attack attack)
			{
				if (attack.WasLethal)
				{
					kills++;
				}
			}, delegate(Attack attack)
			{
				attack.damage += multiplier * (float)kills;
			}));
			onUpdate = delegate
			{
				if (kills > 0)
				{
					kills--;
				}
				return 20f;
			};
		}

		protected override void Page()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextAttacked(2f, delegate(Attack attack)
			{
				attack.source.Affect(new CalmEffect(2f * multiplier), stacking: false);
			}, color));
		}

		protected override void Bard()
		{
			onUpdate = () => (!caster.Affect(new OnNextHit(float.PositiveInfinity, delegate(Attack attack)
			{
				Attackable target = attack.target;
				if (target.Health < 0.1f * target.HealthMax && target.Health < caster.AttackDamage * 10f)
				{
					target.Health = 0f;
				}
			}), stacking: false)) ? 0f : 10f;
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float num = multiplier;
				while (num >= 1f && target.Remove<PoisonEffect>() && target.Remove<BurningEffect>() && target.Remove<RadiationEffect>() && target.Remove<WeakenEffect>())
				{
					num -= 1f;
				}
				if (num <= 0f)
				{
					return Math.Abs(num - multiplier) > 0.01f;
				}
				IStatusEffect statusEffect = ((target.GetEffect<PoisonEffect>() ?? target.GetEffect<BurningEffect>()) ?? target.GetEffect<RadiationEffect>()) ?? target.GetEffect<WeakenEffect>();
				if (statusEffect == null)
				{
					return Math.Abs(num - multiplier) > 0.01f;
				}
				((StatusEffect)statusEffect).ReduceTime(1f / (1f - num));
				return true;
			};
			needsTarget = true;
		}

		protected override void Heir()
		{
			onAffect = delegate(ref IStatusEffect effect)
			{
				if (effect is PoisonEffect || effect is BurningEffect || effect is RadiationEffect || effect is WeakenEffect)
				{
					((StatusEffect)effect).ReduceTime(1.25f);
				}
			};
		}

		protected override void Thief()
		{
			afterAttack = delegate(Attack attack)
			{
				if (!attack.WasLethal)
				{
					Transform transform = attack.target.transform;
					ParticleCollision.Spot(transform.position, transform.rotation, Classpect.GetIcon(Aspect.Heart), 3f, new Attackable[1] { caster }, delegate(Attackable att)
					{
						att.Affect(new OnNextHit(3f, delegate(Attack attack2)
						{
							attack2.damage += ((Player)caster).AbilityPower;
						}));
					});
				}
			};
		}

		protected override void Rogue()
		{
			afterAttack = delegate(Attack attack)
			{
				if (!attack.WasLethal)
				{
					Transform transform = attack.target.transform;
					ParticleCollision.Spot(transform.position, transform.rotation, Classpect.GetIcon(Aspect.Heart), 3f, new Attackable[1] { caster }, delegate(Attackable att)
					{
						ParticleCollision.Ring(att.gameObject, color, 0.5f, 4f);
						foreach (Player player in Attacking.GetPlayers(caster.transform.position, 4f))
						{
							player.Affect(new OnNextHit(3f, delegate(Attack attack2)
							{
								attack2.damage += 0.8f * player.AbilityPower;
							}));
						}
					});
				}
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			target.Affect(new WeakenEffect(duration, 0f, (0f - delta) / 2f));
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			return target.Damage(delta / 2f, caster);
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class Shock : ToggledAbility
	{
		private readonly float damage;

		private readonly float period;

		private readonly float radius;

		private Dictionary<uint, float> attacked;

		[ProtoIgnore]
		private readonly AudioClip clip;

		[ProtoIgnore]
		private AudioSource source;

		public Shock(Attacking caster, float damage, float period, float radius)
			: base(caster, new Color(1f, 0.5f, 0.5f), "Fluctuate", 2f)
		{
			this.damage = damage;
			this.period = period;
			this.radius = radius;
			description = "Toggle to shock enemies.";
			clip = Resources.Load<AudioClip>("Music/Abilities/Fluctuate");
		}

		public override float Update(Attackable self)
		{
			if (caster.Vim < 0.3f * period)
			{
				return -1f;
			}
			caster.Vim -= 0.3f * period;
			int num = 0;
			foreach (Attackable item in caster.GetNearest(radius))
			{
				if (!attacked.TryGetValue(item.netId, out var value) || value <= Time.time)
				{
					Attacking.Lightning(caster.gameObject, item.gameObject, 4f * period);
					item.Damage(damage, caster);
					attacked[item.netId] = Time.time + 5f * period;
					num++;
					if (num >= 2)
					{
						break;
					}
				}
			}
			if (num == 0)
			{
				Attacking.Lightning(caster.gameObject, null, 3f * period, null, caster.transform.position + radius * UnityEngine.Random.insideUnitSphere);
			}
			return period;
		}

		public override void Begin(Attackable att)
		{
			base.Begin(att);
			source = att.gameObject.AddComponent<AudioSource>();
			source.clip = clip;
			source.loop = true;
			source.Play();
			if (attacked == null)
			{
				attacked = new Dictionary<uint, float>();
			}
		}

		public override void Stop(Attackable att)
		{
			base.Stop(att);
			UnityEngine.Object.Destroy(source);
			attacked.Clear();
		}
	}

	private class Astonish : Ability
	{
		private readonly float dist;

		public Astonish(Attacking caster, float dist)
			: base(caster, Classpect.GetColor(Aspect.Heart), "Astonish", 8f, "MagicShoot")
		{
			this.dist = dist;
			vimcost = 0.4f;
			description = "Shoot a beam of electricity towards the cursor";
			audio = Resources.Load<AudioClip>("Music/Abilities/Astonish");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			Vector3 position2 = caster.transform.position;
			RaycastHit[] array = Physics.RaycastAll(position2, position.Value - position2, dist);
			Attackable component = null;
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if (!(raycastHit.rigidbody == null) && !raycastHit.rigidbody.TryGetComponent<Attackable>(out component))
				{
					if (component != caster && !caster.Allies.Contains(component))
					{
						break;
					}
					component = null;
				}
			}
			if ((object)component == null)
			{
				return false;
			}
			Vector3 position3 = caster.transform.position;
			float num = multiplier * (10f + ((Player)caster).AbilityPower * 2f);
			float num2 = 1f;
			do
			{
				Attacking.ColoredLightning(null, component.gameObject, color, 0.5f, position3);
				component.Damage(num2 * num, caster);
				num2 -= 0.05f;
				position3 = component.transform.position;
			}
			while (num2 >= 0.4f && component.Health <= 0f && (object)(component = caster.GetNearest(dist).FirstOrDefault()) != null);
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class HopeAbility : ClasspectAbility
	{
		public HopeAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new AfterNextHit(4f, delegate(Attack attack)
			{
				caster.Affect(new HealthRegenBoost(2f, multiplier * attack.damage / 3f));
			}));
		}

		protected override void Page()
		{
			onAttacked = delegate(Attack attack)
			{
				foreach (Attackable ally in caster.Allies)
				{
					ally.AddShield(attack.damage / 16f);
				}
			};
		}

		protected override void Prince()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				StrifeAI strifeAI = target as StrifeAI;
				return strifeAI != null && strifeAI.ApplyFear(caster.transform.position, 3f * multiplier);
			};
		}

		protected override void Bard()
		{
			onStackIncrease = delegate(int stacks)
			{
				if (stacks % 3 != 0)
				{
					return;
				}
				foreach (Attackable item in caster.GetNearby(4f))
				{
					StrifeAI strifeAI = item as StrifeAI;
					if (strifeAI != null)
					{
						strifeAI.ApplyFear(caster.transform.position, 2f);
					}
				}
			};
		}

		protected override void Witch()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => Missile.Make(caster, target, 1f, color, 0.75f, 1f, delegate
			{
				target.Affect(new OnNextAttacked(10f, delegate(Attack attack)
				{
					attack.damage *= Mathf.Max(0f, 1f - multiplier);
				}, color));
			});
			needsTarget = true;
		}

		protected override void Heir()
		{
			onUpdate = delegate
			{
				caster.Affect(new OnNextAttacked(10f, delegate(Attack attack)
				{
					attack.damage = 0f;
				}, color), stacking: false);
				return 20f;
			};
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				caster.Affect(new HealthRegenBoost(float.PositiveInfinity, 0.25f));
			};
			onStackDecrease = delegate
			{
				IStatusEffect statusEffect = caster.GetEffects<HealthRegenBoost>().FirstOrDefault((HealthRegenBoost e) => float.IsPositiveInfinity(e.EndTime));
				if (statusEffect != null)
				{
					caster.Remove(statusEffect);
				}
			};
		}

		protected override void Seer()
		{
			onUpdate = delegate
			{
				float value = NetcodeManager.rng.value;
				float value2 = NetcodeManager.rng.value;
				float value3 = NetcodeManager.rng.value;
				if (!Physics.Raycast(caster.transform.position, new Vector3(value - 0.5f, (0f - value2) / 2f, value3 - 0.5f), out var hitInfo, 16f))
				{
					return 0f;
				}
				if (hitInfo.collider.transform.IsChildOf(caster.transform))
				{
					return 0f;
				}
				ParticleCollision.Spot(hitInfo.point, caster.transform.rotation, Classpect.GetIcon(((Player)caster).classpect.aspect), 4f, caster.Allies, delegate(Attackable att)
				{
					att.Affect(new HealthRegenBoost(4f, 2.5f));
				});
				return 8f;
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			target.AddShield(delta);
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			return target.Damage(delta, caster);
		}
	}

	private class ReflectiveShieldAbility : Ability
	{
		private readonly float factor;

		private readonly float duration;

		public ReflectiveShieldAbility(Attacking caster, float factor, float duration)
			: base(caster, Color.magenta, "Bravery", 4f, "MagicShoot")
		{
			this.factor = factor;
			this.duration = duration;
			vimcost = 0.5f;
			description = "Cover the target in a shield that damages attackers.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Bravery");
		}

		protected override bool Effect(Attackable att = null, Vector3? position = null, float multiplier = 1f)
		{
			if (att == null)
			{
				return false;
			}
			if (att.GetEffect<ReflectiveShield>() != null)
			{
				return false;
			}
			att.Affect(new ReflectiveShield(duration, factor * multiplier));
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class RageAbility : ClasspectAbility
	{
		public RageAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextHit(4f, delegate(Attack attack)
			{
				attack.damage *= Mathf.Pow(2f, multiplier);
			}));
		}

		protected override void Bard()
		{
			onAttacked = delegate(Attack attack)
			{
				attack.source.Affect(new AttackBoost(0.5f, -4f));
			};
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float radius = 4f * multiplier;
				ParticleCollision.Ring(caster.gameObject, color, 0.5f, radius);
				foreach (Attackable item in caster.GetNearby(radius))
				{
					item.Affect(new CalmEffect(1f * multiplier), stacking: false);
				}
				return true;
			};
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				caster.Affect(new AttackBoost(float.PositiveInfinity, 0.5f));
			};
			onStackDecrease = delegate
			{
				AttackBoost attackBoost = caster.GetEffects<AttackBoost>().FirstOrDefault((AttackBoost e) => e.EndTime >= float.PositiveInfinity);
				if (attackBoost != null)
				{
					caster.Remove(attackBoost);
				}
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking))
			{
				return 0f;
			}
			target.Affect(new AttackBoost(duration, delta / 2f));
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			if (!(target is Attacking attacking))
			{
				return 0f;
			}
			delta = Mathf.Min(delta, attacking.AttackDamage);
			attacking.Affect(new AttackBoost(duration, (0f - delta) / 2f));
			return delta;
		}
	}

	private class AttackStun : Ability
	{
		private readonly float damageAmp;

		private readonly float duration;

		private readonly float timeOut;

		private readonly AudioClip clip;

		private float multiplier;

		public AttackStun(Attacking caster, float damageAmp, float duration, float timeOut)
			: base(caster, Color.magenta, "Empower", 4f)
		{
			this.damageAmp = damageAmp;
			this.duration = duration;
			this.timeOut = timeOut;
			vimcost = 0.5f;
			description = "Empower your next basic attack to stun.";
			clip = Resources.Load<AudioClip>("Music/Abilities/Empower");
		}

		protected override bool Effect(Attackable att = null, Vector3? position = null, float multiplier = 1f)
		{
			this.multiplier = multiplier;
			caster.Affect(new OnNextHit(timeOut, EmpoweredAttack));
			return true;
		}

		private void EmpoweredAttack(Attack attack)
		{
			attack.damage *= 1f + damageAmp * multiplier;
			attack.target.Affect(new SlowEffect(duration * multiplier, 99f));
			caster.PlayAudio(clip);
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class Turmoil : ToggledAbility
	{
		private bool active;

		private const float period = 0.5f;

		public Turmoil(Attacking caster)
			: base(caster, Classpect.GetColor(Aspect.Rage), "Turmoil", 6f)
		{
			description = "Toggle to make any enemy hit go berserk, attacking anyone and anything around it.";
		}

		public override float Update(Attackable self)
		{
			if (caster.Vim < 0.125f)
			{
				return -1f;
			}
			caster.Vim -= 0.15f;
			return 0.5f;
		}

		public override bool AfterAttack(Attack attack)
		{
			if (!(attack.target is StrifeAI strifeAI))
			{
				return false;
			}
			strifeAI.Affect(new FrenzyEffect(1f), stacking: false);
			return false;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class BloodAbility : ClasspectAbility
	{
		public BloodAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new AbilityEffectivenessBoost(4f, multiplier * 1.5f));
			maxCooldown = 8f;
		}

		protected override void Bard()
		{
			onUpdate = delegate
			{
				float delta = 1f + 0.25f * ((Player)caster).AbilityPower;
				StatusEffect subtractEffect = GetSubtractEffect(caster, ref delta);
				foreach (Attackable item in caster.GetNearby(6f))
				{
					item.Affect(subtractEffect);
				}
				return subtractEffect.EndTime - Time.time;
			};
		}

		protected override void Witch()
		{
			afterAttack = delegate(Attack attack)
			{
				if (attack.WasLethal)
				{
					Transform transform = attack.target.transform;
					ParticleCollision.Spot(transform.position, transform.rotation, Classpect.GetIcon(Aspect.Blood), 3f, new Attackable[1] { caster }, delegate(Attackable att)
					{
						att.Affect(GetAddEffect(attack.target, 2f));
					});
				}
			};
		}

		protected override void Heir()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				Player player = caster.GetNearestPlayers(16f).FirstOrDefault();
				if ((object)player == null)
				{
					return false;
				}
				float abilityPower = ((Player)caster).AbilityPower;
				switch (player.classpect.aspect)
				{
				case Aspect.Life:
					caster.Health += multiplier * (abilityPower + 5f);
					break;
				case Aspect.Hope:
					caster.Affect(new HealthRegenBoost(4f, multiplier * (abilityPower / 5f + 2f)));
					break;
				case Aspect.Mind:
					caster.Vim += multiplier * (abilityPower / 10f + 0.1f);
					break;
				case Aspect.Heart:
					caster.Affect(new VimRegenBoost(4f, multiplier * (abilityPower / 40f + 0.025f)));
					break;
				case Aspect.Space:
					target.Affect(new APBoost(2f, multiplier * (8f + abilityPower)));
					break;
				case Aspect.Time:
					caster.Affect(new AttackSpeedBoost(2f, Mathf.Pow(2f, (0f - multiplier) * (0.8f + abilityPower / 10f)) - 1f));
					break;
				case Aspect.Doom:
					caster.Affect(new WeakenEffect(2f, 0f, multiplier * (8f + abilityPower)));
					break;
				case Aspect.Breath:
					caster.Affect(new SlowEffect(2f, Mathf.Pow(2f, (0f - multiplier) * (0.8f + abilityPower / 10f)) - 1f));
					break;
				case Aspect.Void:
					foreach (Ability ability in ((Attacking)target).abilities)
					{
						ability.cooldown = Mathf.Max(0f, ability.cooldown - multiplier * (8f + abilityPower) / 4f);
					}
					break;
				case Aspect.Light:
					caster.Affect(new LuckBoost(2f, multiplier * (8f + abilityPower)));
					break;
				}
				return true;
			};
		}

		protected override void Mage()
		{
			onStackIncrease = delegate(int stacks)
			{
				DecreasingStackEffect decreasingStackEffect = (DecreasingStackEffect)caster.GetEffect<DecreasingStackEffect>();
				if (stacks == 1)
				{
					Aspect[] array = new Aspect[7]
					{
						Aspect.Time,
						Aspect.Breath,
						Aspect.Heart,
						Aspect.Space,
						Aspect.Light,
						Aspect.Void,
						Aspect.Rage
					};
					int num = array.Length - 1;
					for (int i = 0; i < num; i++)
					{
						if (array[i] == decreasingStackEffect.aspect)
						{
							array[i] = array[num];
							break;
						}
					}
					decreasingStackEffect.aspect = array[NetcodeManager.rng.Next(num)];
				}
				switch (decreasingStackEffect.aspect)
				{
				case Aspect.Time:
					caster.Affect(new AttackSpeedBoost(float.PositiveInfinity, 1.1f));
					break;
				case Aspect.Breath:
					caster.Affect(new SlowEffect(float.PositiveInfinity, -0.05f));
					break;
				case Aspect.Heart:
					caster.Affect(new WeakenEffect(float.PositiveInfinity, 0f, -1f));
					break;
				case Aspect.Space:
					caster.Affect(new APBoost(float.PositiveInfinity, 0.5f));
					break;
				case Aspect.Light:
					caster.Affect(new LuckBoost(float.PositiveInfinity, 0.05f));
					break;
				case Aspect.Void:
				{
					foreach (Ability ability in caster.abilities)
					{
						ability.cooldown = Mathf.Max(0f, ability.cooldown - 0.5f);
					}
					break;
				}
				case Aspect.Rage:
					caster.Affect(new AttackBoost(float.PositiveInfinity, 0.5f));
					break;
				case Aspect.Doom:
				case Aspect.Blood:
				case Aspect.Mind:
					break;
				}
			};
			onStackDecrease = delegate
			{
				DecreasingStackEffect obj = (DecreasingStackEffect)caster.GetEffect<DecreasingStackEffect>();
				IStatusEffect statusEffect = null;
				switch (obj.aspect)
				{
				case Aspect.Time:
					statusEffect = caster.GetEffects<AttackSpeedBoost>().FirstOrDefault((AttackSpeedBoost e) => e.EndTime >= float.PositiveInfinity);
					break;
				case Aspect.Breath:
					statusEffect = caster.GetEffects<SlowEffect>().FirstOrDefault((SlowEffect e) => e.EndTime >= float.PositiveInfinity);
					break;
				case Aspect.Heart:
					statusEffect = caster.GetEffects<WeakenEffect>().FirstOrDefault((WeakenEffect e) => e.EndTime >= float.PositiveInfinity);
					break;
				case Aspect.Space:
					statusEffect = caster.GetEffects<APBoost>().FirstOrDefault((APBoost e) => e.EndTime >= float.PositiveInfinity);
					break;
				case Aspect.Light:
					statusEffect = caster.GetEffects<LuckBoost>().FirstOrDefault((LuckBoost e) => e.EndTime >= float.PositiveInfinity);
					break;
				case Aspect.Void:
					foreach (Ability ability2 in caster.abilities)
					{
						ability2.cooldown = Mathf.Min(ability2.maxCooldown, ability2.cooldown + 0.5f);
					}
					break;
				case Aspect.Rage:
					statusEffect = caster.GetEffects<AttackBoost>().FirstOrDefault((AttackBoost e) => e.EndTime >= float.PositiveInfinity);
					break;
				}
				if (statusEffect != null)
				{
					caster.Remove(statusEffect);
				}
			};
		}

		protected override void Seer()
		{
			onUpdate = delegate
			{
				float value = NetcodeManager.rng.value;
				float value2 = NetcodeManager.rng.value;
				float value3 = NetcodeManager.rng.value;
				if (!Physics.Raycast(caster.transform.position, new Vector3(value - 0.5f, (0f - value2) / 2f, value3 - 0.5f), out var hitInfo, 16f))
				{
					return 0f;
				}
				if (hitInfo.collider.transform.IsChildOf(caster.transform))
				{
					return 0f;
				}
				ParticleCollision.Spot(hitInfo.point, caster.transform.rotation, Classpect.GetIcon(((Player)caster).classpect.aspect), 4f, caster.Allies, delegate(Attackable att)
				{
					switch (NetcodeManager.rng.Next(5))
					{
					case 0:
						att.Affect(new APBoost(2f, 11f));
						break;
					case 1:
						att.Affect(new AttackSpeedBoost(2f, 2.1f));
						break;
					case 2:
						att.Affect(new SlowEffect(2f, -0.52f));
						break;
					case 3:
						att.Affect(new WeakenEffect(2f, 0f, -6f));
						break;
					case 4:
						att.Affect(new LuckBoost(2f, 1.1f));
						break;
					case 5:
					{
						foreach (Ability ability in ((Attacking)att).abilities)
						{
							ability.cooldown = Mathf.Max(0f, ability.cooldown - 2.75f);
						}
						break;
					}
					case 6:
						att.Affect(new AttackBoost(2f, 11f));
						break;
					}
				});
				return 4f;
			};
		}

		protected override void Thief()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float change = Subtract(target, 6f * multiplier);
				if (Math.Abs(change) < 0.01f)
				{
					return false;
				}
				Missile.Make(target, caster, 0.4f, color, 0.2f, 0.8f, delegate
				{
					caster.Affect(GetAddEffect(target, change * 2f / 3f));
				});
				return true;
			};
			needsTarget = true;
		}

		protected override void Rogue()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float num = Subtract(target, 6f * multiplier);
				if (Math.Abs(num) < 0.01f)
				{
					return false;
				}
				StatusEffect effect = GetAddEffect(target, num / 2f);
				Missile.Make(target, caster, 0.4f, color, 0.2f, 0.8f, delegate
				{
					ParticleCollision.Ring(caster.gameObject, color, 0.5f, 4f);
					foreach (Player player in Attacking.GetPlayers(caster.transform.position, 4f))
					{
						player.Affect(effect);
					}
				});
				return true;
			};
			needsTarget = true;
		}

		private static StatusEffect GetAddEffect(Attackable target, float delta, float duration = 2f)
		{
			float num = target.Health / 10f * ((target.Defense < 0f) ? (1f / Mathf.Sqrt(1f - target.Defense)) : Mathf.Sqrt(1f + target.Defense));
			StatusEffect result = new WeakenEffect(2f, 0f, 0f - delta);
			if (!(target is Attacking attacking))
			{
				return result;
			}
			if (attacking.AttackSpeed >= 3f)
			{
				return new AttackSpeedBoost(duration, 1f + delta / 10f);
			}
			if (attacking.AttackDamage > num)
			{
				num = attacking.AttackDamage;
				result = new AttackBoost(duration, delta);
			}
			if (!(attacking is Player player))
			{
				return result;
			}
			if (player.AbilityPower <= num)
			{
				return result;
			}
			return new APBoost(duration, delta);
		}

		private static StatusEffect GetSubtractEffect(Attackable target, ref float delta, float duration = 2f)
		{
			float num = delta;
			float num2 = target.Health / 10f * ((target.Defense < 0f) ? (1f / Mathf.Sqrt(1f - target.Defense)) : Mathf.Sqrt(1f + target.Defense));
			StatusEffect result = new WeakenEffect(duration, 0f, num);
			if (!(target is Attacking))
			{
				return result;
			}
			Attacking attacking = (Attacking)target;
			if (attacking.AttackSpeed >= 3f)
			{
				return new AttackSpeedBoost(duration, 1f - num / 10f);
			}
			if (attacking.AttackDamage > num2)
			{
				num2 = attacking.AttackDamage;
				delta = Mathf.Max(attacking.AttackDamage, num);
				result = new AttackBoost(duration, 0f - delta);
			}
			if (!(target is Player player))
			{
				return result;
			}
			if (player.AbilityPower <= num2)
			{
				return result;
			}
			delta = Mathf.Max(player.AbilityPower, num);
			return new APBoost(duration, 0f - delta);
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			target.Affect(GetAddEffect(target, delta, duration));
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			target.Affect(GetSubtractEffect(target, ref delta, duration));
			return delta;
		}
	}

	private class Convert : Ability
	{
		private readonly float radius;

		public Convert(Attacking ncaster, float radius)
			: base(ncaster, new Color(0.75f, 0.1f, 0.1f), "Propaganda", 2f, "MagicCharge")
		{
			this.radius = radius;
			vimcost = 0.25f;
			description = "Temporarily convert enemies to your side.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Propaganda");
		}

		protected override bool Effect(Attackable att = null, Vector3? position = null, float multiplier = 1f)
		{
			ParticleCollision.Add(ParticleCollision.Ring(caster.gameObject, color, 0.5f, radius), delegate(Attackable target)
			{
				if (!target.Faction.IsAllyOf(caster.Faction))
				{
					target.Affect(new AlliedEffect(5f * multiplier, caster), stacking: false);
				}
			});
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class BreathAbility : ClasspectAbility
	{
		public BreathAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new TimeEffect(4f, Mathf.Pow(2f, multiplier)));
		}

		protected override void Prince()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => Missile.Make(caster, target, 0.5f, new Color(0f, 0f, 0.7f), 0.4f, 0.4f, delegate
			{
				target.Affect(new SlowEffect(2f, 2f * multiplier));
			});
			needsTarget = true;
		}

		protected override void Sylph()
		{
			effect = delegate
			{
				ParticleCollision.Ring(caster.gameObject, color, 0.5f, 4f);
				foreach (Player player in Attacking.GetPlayers(caster.transform.position, 4f))
				{
					player.Affect(new SlowEffect(2f, -0.25f));
				}
				return true;
			};
		}

		protected override void Mage()
		{
			onStackIncrease = delegate
			{
				caster.Affect(new SlowEffect(float.PositiveInfinity, -0.05f));
			};
			onStackDecrease = delegate
			{
				SlowEffect slowEffect = caster.GetEffects<SlowEffect>().FirstOrDefault((SlowEffect e) => e.EndTime >= float.PositiveInfinity);
				if (slowEffect != null)
				{
					caster.Remove(slowEffect);
				}
			};
		}

		protected override void Maid()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => Missile.Make(caster, target, 0.5f, color, 0.4f, 0.4f, delegate
			{
				target.Affect(new SlowEffect(2f, -0.5f * multiplier));
			});
			needsTarget = true;
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				bool result = false;
				foreach (Player player in caster.GetNearestPlayers(4f).Take(Mathf.RoundToInt(multiplier * 2f)))
				{
					result = true;
					Missile.Make(caster, player, 0.5f, color, 0.4f, 0.4f, delegate
					{
						player.Remove<SlowEffect>();
					});
				}
				return result;
			};
		}

		protected override void Thief()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				target.Affect(new SlowEffect(2f, 1f * multiplier));
				Missile.Make(target, caster, 0.4f, color, 0.2f, 0.8f, delegate
				{
					caster.Affect(new SlowEffect(2f, -0.25f * multiplier));
				});
				return true;
			};
			needsTarget = true;
		}

		protected override void Rogue()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				target.Affect(new SlowEffect(2f, 1f * multiplier));
				Missile.Make(target, caster, 0.4f, color, 0.2f, 0.8f, delegate
				{
					ParticleCollision.Ring(caster.gameObject, color, 0.5f, 4f);
					foreach (Player player in Attacking.GetPlayers(caster.transform.position, 4f))
					{
						player.Affect(new SlowEffect(2f, -0.2f * multiplier));
					}
				});
				return true;
			};
			needsTarget = true;
		}

		protected override void Bard()
		{
			onAttacked = delegate(Attack attack)
			{
				attack.source.Affect(new SlowEffect(0.5f, 0.5f));
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 4f)
		{
			target.Affect(new SlowEffect(duration, Mathf.Pow(2f, (0f - delta) / 10f) - 1f));
			return delta;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			target.Affect(new SlowEffect(duration, Mathf.Pow(2f, delta / 10f) - 1f));
			return delta;
		}
	}

	private class Fly : Ability
	{
		private readonly Sprite[] sprites = Resources.LoadAll<Sprite>("Its a Bird");

		public Fly(Attacking caster)
			: base(caster, Classpect.GetColor(Aspect.Breath), "It's a Bird!", 2f, "Fly")
		{
			vimcost = 0.1f;
			description = "Fly in the direction you're facing.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Its_A_Bird");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			caster.GetComponent<PlayerMovement>().Jump();
			animator.SetFloat(Effectiveness, 1f / multiplier);
			AbilityAnimator.Make(sprites, (caster is Player player) ? player.GetPosition() : caster.transform.position);
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class LifeAbility : ClasspectAbility
	{
		private static readonly Sprite[][] flowers = new string[3] { "Life Flower_0", "Life Flower_1", "Life Flower_2" }.Select(Resources.LoadAll<Sprite>).ToArray();

		public LifeAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextHit(4f, delegate(Attack attack)
			{
				attack.damage *= 1f + multiplier * caster.Health / caster.HealthMax;
			}));
		}

		protected override void Heir()
		{
			onUpdate = delegate
			{
				caster.Health += (caster.HealthMax - caster.Health) / 5f;
				return 1f;
			};
		}

		protected override void Mage()
		{
			effect = delegate
			{
				caster.Affect(new PhysicalBuffBoost(1f, 1.1f));
				foreach (Player player in Attacking.GetPlayers(caster.transform.position, 6f))
				{
					player.Affect(new PhysicalBuffBoost(1f, 1.3f));
				}
				return true;
			};
		}

		protected override void Seer()
		{
			onUpdate = delegate
			{
				caster.Affect(new PhysicalBuffBoost(1f, 1.1f));
				caster.GetNearestPlayers(4f).FirstOrDefault()?.Affect(new PhysicalBuffBoost(1f, 1.1f));
				return 1f;
			};
		}

		protected override float Add(Attackable target, float delta, float duration = 0f)
		{
			float num = Mathf.Min(delta, target.HealthMax - target.Health);
			target.Health += num;
			Vector3 vector = ((target is Player player) ? player.GetPosition() : target.transform.position);
			for (int i = 0; (float)i < 5f * delta; i++)
			{
				Vector3 vector2 = 1f * UnityEngine.Random.insideUnitSphere;
				float maxDistance = 1f - vector2.magnitude;
				if (NavMesh.SamplePosition(vector + vector2, out var hit, maxDistance, -1))
				{
					AbilityAnimator.Make(flowers[i % flowers.Length], hit.position);
				}
			}
			return num;
		}

		protected override float Subtract(Attackable target, float delta, float duration = 4f)
		{
			return target.Damage(delta, caster);
		}
	}

	private class Heal : Ability
	{
		private readonly float radius;

		public Heal(Attacking ncaster, float radius)
			: base(ncaster, Classpect.GetColor(Aspect.Life), "Flourish", 3f, "MagicCharge")
		{
			this.radius = radius;
			vimcost = 0.8f;
			description = "Heal nearby players.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Flourish");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			foreach (Player player in Attacking.GetPlayers(caster.transform.position, radius))
			{
				player.Health += 5f * ((Player)caster).AbilityPower * multiplier;
				player.Affect(new SlowEffect(2f, -0.5f * multiplier));
			}
			ParticleCollision.Ring(caster.gameObject, color, 0.5f, radius);
			return true;
		}
	}

	private class Lightbeam : Ability
	{
		private readonly float radius;

		private readonly float damage;

		public Lightbeam(Attacking caster, float radius, float damage)
			: base(caster, Classpect.GetColor(Aspect.Life), "Lightbeam", 3f, "MagicCharge")
		{
			this.radius = radius;
			this.damage = damage;
			vimcost = 0.3f;
			description = "Summon a beam of light that heals friends and harms foes.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Lightbeam");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			foreach (Attackable item in caster.GetNearby(position.Value, radius, enemy: false))
			{
				item.Health += damage * multiplier;
			}
			foreach (Attackable item2 in caster.GetNearby(position.Value, radius))
			{
				item2.Damage(damage * multiplier, caster);
			}
			ParticleCollision.Ring(caster.gameObject, new Color(1f, 1f, 0.9f), 0.5f, radius, 360f, Vector3.zero, position.Value);
			return true;
		}
	}

	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	private class DoomAbility : ClasspectAbility
	{
		public DoomAbility(Player caster)
			: base(caster)
		{
		}

		protected override void Knight()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => caster.Affect(new OnNextHit(4f, delegate(Attack attack)
			{
				attack.damage *= 1f + multiplier * (caster.HealthMax - caster.Health) / caster.HealthMax;
			}));
		}

		protected override void Page()
		{
			onUpdate = delegate
			{
				foreach (Attackable item in caster.GetNearby(6f))
				{
					item.Affect(new WeakenEffect(0.5f, 0f, 1f));
				}
				return 0.5f;
			};
		}

		protected override void Prince()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float num = multiplier;
				while (num >= 1f && (target.Remove<SlowEffect>() || target.Remove<TauntEffect>() || target.Remove<PoisonEffect>() || target.Remove<BurningEffect>() || target.Remove<RadiationEffect>() || target.Remove<WeakenEffect>()))
				{
					num -= 1f;
				}
				if (num <= 0f)
				{
					return Math.Abs(num - multiplier) > 0.01f;
				}
				object obj = (((target.GetEffect<SlowEffect>() ?? target.GetEffect<TauntEffect>()) ?? target.GetEffect<PoisonEffect>()) ?? target.GetEffect<BurningEffect>()) ?? target.GetEffect<RadiationEffect>();
				if (obj == null)
				{
					obj = target.GetEffect<WeakenEffect>();
				}
				IStatusEffect statusEffect = (IStatusEffect)obj;
				if (statusEffect == null)
				{
					return Math.Abs(num - multiplier) > 0.01f;
				}
				((StatusEffect)statusEffect).ReduceTime(1f / (1f - num));
				return true;
			};
			needsTarget = true;
		}

		protected override void Bard()
		{
			onUpdate = delegate
			{
				foreach (Attackable item in caster.GetNearby(4f, enemy: false))
				{
					item.Affect(new DebuffResistance(0.5f, 1.1f), stacking: false);
				}
				return 0.5f;
			};
		}

		protected override void Sylph()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				if (target == null)
				{
					return false;
				}
				target.Damage(10f * multiplier, caster);
				if (target.Health <= 0f)
				{
					caster.Health += 10f * multiplier;
				}
				return true;
			};
		}

		protected override void Maid()
		{
			effect = (Attackable target, Vector3? position, float multiplier) => target.Affect(new WeakenEffect(2f, 0f, 3f * multiplier));
			needsTarget = true;
		}

		protected override void Heir()
		{
			afterAttack = delegate(Attack attack)
			{
				if (attack.WasLethal && attack.target is Enemy && !attack.target.Faction.IsAllyOf(caster.Faction) && NetcodeManager.rng.Next(1, 101) <= 30)
				{
					SpawnFriendlyCopy(attack.target);
				}
			};
		}

		protected override void Witch()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				if (target == null)
				{
					return false;
				}
				if (!(target is Enemy))
				{
					return false;
				}
				if (target.Faction.IsAllyOf(caster.Faction))
				{
					return false;
				}
				target.Damage(5f * multiplier, caster);
				if (target.Health <= 0f)
				{
					SpawnFriendlyCopy(target);
				}
				return true;
			};
		}

		[ServerCallback]
		private void SpawnFriendlyCopy(Attackable target)
		{
			if (NetworkServer.active && target is Enemy)
			{
				SpawnHelper.instance.Spawn(target.name, target.RegionChild.Area, target.transform.position, delegate(Attackable att)
				{
					att.Affect(new AlliedEffect(float.PositiveInfinity, caster));
				});
			}
		}

		protected override void Thief()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float num = multiplier;
				while (num > 0f)
				{
					object obj = (caster.GetEffect<SlowEffect>() ?? caster.GetEffect<PoisonEffect>()) ?? caster.GetEffect<BurningEffect>();
					if (obj == null)
					{
						obj = caster.GetEffect<RadiationEffect>();
					}
					if (obj == null)
					{
						obj = caster.GetEffect<WeakenEffect>();
					}
					IStatusEffect statusEffect = (IStatusEffect)obj;
					if (statusEffect == null)
					{
						break;
					}
					IStatusEffect copy = StatusEffect.Clone(statusEffect);
					Missile.Make(caster, target, 0.25f, color, 0.4f * Mathf.Min(1f, num), 0.4f, delegate
					{
						target.Affect(copy);
					});
					if (num < 1f)
					{
						((StatusEffect)copy).ReduceTime(1f / num);
						((StatusEffect)statusEffect).ReduceTime(1f / (1f - num));
						num = 0f;
					}
					else
					{
						caster.Remove(statusEffect);
						num -= 1f;
					}
				}
				return Math.Abs(num - multiplier) > 0.01f;
			};
			needsTarget = true;
		}

		protected override void Rogue()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				foreach (Attackable att in caster.GetNearest(4f))
				{
					float num = multiplier;
					while (num > 0f)
					{
						IStatusEffect statusEffect = target.GetEffect<SlowEffect>() ?? target.GetEffect<PoisonEffect>() ?? target.GetEffect<BurningEffect>() ?? target.GetEffect<RadiationEffect>() ?? target.GetEffect<WeakenEffect>();
						if (statusEffect == null)
						{
							break;
						}
						IStatusEffect copy = StatusEffect.Clone(statusEffect);
						Missile.Make(target, att, 0.25f, color, 0.4f * Mathf.Min(1f, num), 0.4f, delegate
						{
							att.Affect(copy);
						});
						if (num < 1f)
						{
							((StatusEffect)copy).ReduceTime(1f / num);
							((StatusEffect)statusEffect).ReduceTime(1f / (1f - num));
							num = 0f;
						}
						else
						{
							target.Remove(statusEffect);
							num -= 1f;
						}
					}
					if (num != multiplier)
					{
						return true;
					}
				}
				return false;
			};
			needsTarget = true;
		}

		protected override void Mage()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float radius = 6f * multiplier;
				float duration = 4f;
				ParticleCollision.Ring(caster.gameObject, new Color(0f, 0.96f, 0.96f), 0.5f, radius);
				foreach (Attackable item in caster.GetNearby(radius, enemy: true, inBattle: false))
				{
					item.Affect(new RevealEffect(duration), stacking: false);
				}
				RevealTraps(radius, duration, spring: true);
				return true;
			};
		}

		protected override void Seer()
		{
			effect = delegate(Attackable target, Vector3? position, float multiplier)
			{
				float radius = 8f * multiplier;
				float duration = 4f;
				ParticleCollision.Ring(caster.gameObject, new Color(0f, 0.9f, 0.9f), 0.5f, radius);
				foreach (Attackable item in caster.GetNearby(radius, enemy: true, inBattle: false))
				{
					item.Affect(new RevealEffect(duration), stacking: false);
					item.Affect(new WeakenEffect(duration, 0f, 1.5f));
				}
				RevealTraps(radius, duration);
				return true;
			};
		}

		private void RevealTraps(float radius, float duration, bool spring = false)
		{
			Story componentInParent = caster.transform.GetComponentInParent<Story>();
			if (componentInParent == null)
			{
				return;
			}
			Vector3 localPosition = caster.transform.localPosition;
			Furniture[] componentsInChildren = componentInParent.GetComponentsInChildren<Furniture>();
			foreach (Furniture furniture in componentsInChildren)
			{
				if ((double)(furniture.transform.localPosition - localPosition).sqrMagnitude >= Math.Pow(radius, 2.0))
				{
					continue;
				}
				Trap componentInChildren = furniture.GetComponentInChildren<Trap>();
				if (!(furniture == null) && !(componentInChildren == null))
				{
					if (spring)
					{
						componentInChildren.Spring();
					}
					if (furniture.TryGetComponent<RegionChild>(out var component))
					{
						component.enabled = false;
						Visibility.Set(component.gameObject, value: true);
						furniture.StartCoroutine(LightAbility.ResetVisibility(component, duration));
					}
				}
			}
		}
	}

	private class Weaken : Ability
	{
		private readonly float radius;

		private readonly float duration;

		private readonly float slow;

		private readonly float defense;

		private readonly Sprite[] sprites;

		private readonly Sprite[] smoke;

		public Weaken(Attacking caster, float radius, float duration, float slow, float defense)
			: base(caster, new Color(0f, 0.5f, 0f), "Judgement", 10f, "MagicCharge")
		{
			this.radius = radius;
			this.duration = duration;
			this.slow = slow;
			this.defense = defense;
			vimcost = 0.8f;
			description = "Weaken enemies near the cursor.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Judgement");
			sprites = Resources.LoadAll<Sprite>("judgement");
			smoke = Resources.LoadAll<Sprite>("judgement particles");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			AbilityAnimator anim = AbilityAnimator.Make(sprites, position.Value);
			anim.SetEvent(10, delegate
			{
				ParticleSystem system = ParticleCollision.Ring(anim.gameObject, Color.white, 1f, radius, 360f, default(Vector3), position);
				ParticleCollision.Add(system, delegate(Attackable att)
				{
					if (caster.IsValidTarget(att))
					{
						att.Affect(new WeakenEffect(duration, slow * multiplier, defense * multiplier));
					}
				});
				ParticleCollision.SetSprites(system, smoke);
			});
			return true;
		}
	}

	private class Wrath : Ability
	{
		private readonly float radius;

		private readonly float knockback;

		private readonly Sprite[] sprites;

		public Wrath(Attacking caster, float radius, float knockback)
			: base(caster, Classpect.GetColor(Aspect.Doom), "Wrath", 5f, "MagicCharge")
		{
			this.radius = radius;
			this.knockback = knockback;
			vimcost = 0.3f;
			description = "Summon a pillar that pushes back and damages.";
			audio = Resources.Load<AudioClip>("Music/Abilities/Wrath");
			sprites = Resources.LoadAll<Sprite>("Wrath");
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!position.HasValue)
			{
				return false;
			}
			AbilityAnimator.Make(sprites, position.Value, radius).SetEvent(8, delegate
			{
				HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
				Collider[] array = Physics.OverlapSphere(position.Value, radius);
				foreach (Collider collider in array)
				{
					if (collider.attachedRigidbody != null)
					{
						hashSet.Add(collider.attachedRigidbody);
					}
				}
				foreach (Rigidbody item in hashSet)
				{
					Attacking.AddForce(item, (item.position - position.Value).normalized * (knockback * multiplier), ForceMode.VelocityChange);
					if (item.TryGetComponent<Attackable>(out var component) && caster.IsValidTarget(component))
					{
						component.Damage(multiplier * (5f + ((Player)caster).AbilityPower), caster);
					}
				}
			});
			return true;
		}
	}

	public const float INTERACTRANGE = 6f;

	public static byte[] loadedPlayerData;

	public static int spawnLocation;

	public bool self;

	public static Player player;

	private static readonly List<Player> all;

	public Classpect classpect;

	public Sylladex sylladex;

	public PlayerSync sync;

	[SerializeField]
	private KernelSprite kernelSprite;

	[SerializeField]
	private WeaponAnimator weaponAnimator;

	private PlayerMovement movement;

	private static PlayerUI playerUi;

	private Text gristCache;

	public float boonBucks;

	private bool updateCacheText;

	[SyncVar(hook = "OnWeaponChanged")]
	private NormalItem weapon;

	private readonly NormalItem[] armor = new NormalItem[5];

	private Vector3 footOffset;

	public readonly GristCollection Grist = new GristCollection();

	private float experience;

	[SyncVar(hook = "OnLevelChange")]
	private uint level = 1u;

	private uint prevLevel = 1u;

	private static readonly int Effectiveness;

	public static Transform Ui
	{
		get
		{
			if (!playerUi)
			{
				playerUi = UnityEngine.Object.FindObjectOfType<PlayerUI>(includeInactive: true);
			}
			return playerUi.transform;
		}
	}

	public Text GristCache
	{
		private get
		{
			return gristCache;
		}
		set
		{
			updateCacheText = true;
			gristCache = value;
		}
	}

	public float Offense { get; private set; } = 1f;


	public float Strength { get; private set; }

	public float AbilityPower { get; set; } = 1f;


	public override bool IsSavedWithHouse => false;

	public uint Level
	{
		get
		{
			return level;
		}
		private set
		{
			Networklevel = value;
			RefreshLevel();
		}
	}

	public float Experience
	{
		get
		{
			return experience;
		}
		set
		{
			experience = value;
			if (self)
			{
				playerUi.UpdateExperience(experience, MaxExperience);
			}
		}
	}

	public float MaxExperience => Mathf.Pow(2 * Level + 2, 3f);

	public override float Vim
	{
		get
		{
			return base.Vim;
		}
		set
		{
			float num = base.Vim;
			base.Vim = value;
			if (self && base.Vim != num)
			{
				playerUi.SetVim(value, VimMax);
			}
		}
	}

	public override float VimMax
	{
		get
		{
			return base.VimMax;
		}
		set
		{
			if (base.VimMax != value)
			{
				base.VimMax = value;
				if (self)
				{
					playerUi.SetVim(Vim, value);
				}
			}
		}
	}

	public override float VimRegen
	{
		get
		{
			return base.VimRegen;
		}
		set
		{
			if (base.VimRegen != value)
			{
				base.VimRegen = value;
				if (self)
				{
					playerUi.SetVimRegen(value);
				}
			}
		}
	}

	public override float HealthRegen
	{
		get
		{
			return base.HealthRegen;
		}
		set
		{
			if (base.HealthRegen != value)
			{
				base.HealthRegen = value;
				if (self)
				{
					playerUi.SetHealthRegen(value);
				}
			}
		}
	}

	private Ability basicAttack => abilities[0];

	public override float AttackDamage
	{
		get
		{
			return base.AttackDamage + ((weapon == null) ? 1f : (weapon.Power + Offense));
		}
		set
		{
			base.AttackDamage += value - AttackDamage;
		}
	}

	public override float AttackSpeed
	{
		get
		{
			return base.AttackSpeed * ((weapon == null) ? 1f : Mathf.Sqrt(weapon.Speed * (Strength + weapon.Speed)));
		}
		set
		{
			base.AttackSpeed *= value / AttackSpeed;
		}
	}

	private bool HasKernelSprite => kernelSprite.gameObject.scene.rootCount != 0;

	public KernelSprite KernelSprite
	{
		get
		{
			if (!HasKernelSprite)
			{
				return null;
			}
			return kernelSprite;
		}
	}

	public NormalItem Networkweapon
	{
		get
		{
			return weapon;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref weapon))
			{
				NormalItem from = weapon;
				SetSyncVar(value, ref weapon, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnWeaponChanged(from, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public uint Networklevel
	{
		get
		{
			return level;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref level))
			{
				uint before = level;
				SetSyncVar(value, ref level, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnLevelChange(before, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public static IEnumerable<Player> GetAll()
	{
		return all;
	}

	protected override void Awake()
	{
		sync = GetComponent<PlayerSync>();
		movement = GetComponent<PlayerMovement>();
		Grist[global::Grist.SpecialType.Build] = 10;
		GristCollection grist = Grist;
		grist.OnGristChange = (GristCollection.OnChangeHandler)Delegate.Combine(grist.OnGristChange, new GristCollection.OnChangeHandler(OnGristChange));
		Speed = 4f;
		base.Awake();
		base.Faction.Parent = "Prospit";
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		footOffset = base.transform.position - base.transform.TransformPoint(component.center - new Vector3(0f, component.height / 2f, 0f));
		abilities.Add(new BasicAttack(this, "Confused", "Attempt", new Color(0.4f, 0.53f, 1f)));
		basicAttack.description = "Attempt to attack without a strife weapon.";
		abilities.Add(new Block(this));
		abilities.Add(new Slide(this));
		NetworkClient.RegisterPrefab(kernelSprite.gameObject);
	}

	private void Start()
	{
		base.OnHurt += OnDamage;
		base.OnAttack += OnHit;
		base.RegionChild.onRegionChanged += OnRegionChanged;
		base.RegionChild.onAreaChanged += OnAreaChanged;
		base.name = "Player " + GetID();
		if (!self)
		{
			MakePlacronym();
		}
		else
		{
			if (BuildExploreSwitcher.cheatMode)
			{
				for (int i = 0; i < 63; i++)
				{
					player.Grist[i] = 1073741823;
				}
				Ui.Find("Button Bar/Build").gameObject.SetActive(value: true);
			}
			BuildExploreSwitcher.Instance.SwitchToExplore();
			if (sylladex.enabled)
			{
				RefreshAbilityButtons();
			}
			specialHitSound = Resources.LoadAll<AudioClip>("Music/tgp_hitsound");
			sylladex.strifeSpecibus.InitArmor(armor);
		}
		StartCoroutine(ArmorUpdate());
	}

	public override void OnStartLocalPlayer()
	{
		Debug.Log("Local player started");
		player = this;
		self = true;
		sync.local = true;
		MultiplayerSettings.playerName = sync.np.name;
		BuildExploreSwitcher.Instance.camera.SetTarget(base.transform);
		LoadingScreen.FinishLoading();
		base.RegionChild.Focus();
		if (!playerUi)
		{
			playerUi = UnityEngine.Object.FindObjectOfType<PlayerUI>(includeInactive: true);
		}
		healthVial = playerUi.healthVial;
		sylladex = playerUi.GetComponent<Sylladex>();
		playerUi.SetLevel(level);
		playerUi.UpdateExperience(experience, MaxExperience);
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (!initialState)
		{
			return base.OnSerialize(writer, initialState: false);
		}
		writer.Write(GetID());
		writer.Write(Save());
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (!initialState)
		{
			base.OnDeserialize(reader, initialState: false);
			return;
		}
		sync.np.id = reader.Read<int>();
		Load(reader.Read<PlayerData>());
		NetcodeManager.Instance.RegisterPlayer(this);
	}

	public override void OnStartServer()
	{
		NetcodeManager.Instance.RegisterPlayer(this);
	}

	protected override void OnEnable()
	{
		AcceptorList.acceptors.Add(this);
		all.Add(this);
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		AcceptorList.acceptors.Remove(this);
		all.Remove(this);
		Debug.LogWarning($"Disabled player {GetID()} ({sync.np.name})!");
		base.OnDisable();
	}

	private void OnDestroy()
	{
		if (kernelSprite != null && HasKernelSprite)
		{
			UnityEngine.Object.Destroy(kernelSprite.gameObject);
		}
		Planet componentInParent = GetComponentInParent<Planet>();
		if (componentInParent != null)
		{
			componentInParent.RemovePlayer(this);
		}
		if (!self)
		{
			NetcodeManager.Instance.DeregisterPlayer(this);
		}
		if (base.isLocalPlayer)
		{
			base.RegionChild.Unfocus();
		}
		if (base.isLocalPlayer)
		{
			player = null;
		}
	}

	private void MakePlacronym()
	{
		Image image = new GameObject("Placronym").AddComponent<Image>();
		image.sprite = Resources.Load<Sprite>("Placronym");
		((HealthVialBasic)healthVial).SetNameTag(image.gameObject);
		image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 144f);
		image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 28f);
		Text text = new GameObject("Name tag").AddComponent<Text>();
		text.text = sync.np.name;
		text.font = Resources.Load<Font>("Font/FONTSTUCK");
		text.fontSize = 8;
		text.color = new Color(0.28f, 0.28f, 0.28f);
		text.alignment = TextAnchor.MiddleCenter;
		text.transform.SetParent(image.transform, worldPositionStays: false);
		GetComponentInChildren<HealthVialBasic>().gameObject.GetComponent<RectTransform>().Translate(new Vector3(0f, 1f, 0f));
	}

	private void RefreshAbilityButtons()
	{
		sylladex.RefreshAbilityButtons(abilities, CmdDoAbility);
	}

	[Command]
	private void CmdDoAbility(int index, Attackable target, Vector3? position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(index);
		writer.WriteNetworkBehaviour(target);
		writer.WriteNullable(position);
		SendCommandInternal(typeof(Player), "CmdDoAbility", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	protected override IEnumerator Die(float delay = 0f)
	{
		if (self)
		{
			KeyboardControl.Block();
		}
		yield return new WaitForSeconds(delay);
		NormalItem[] array = armor;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.OnDeath(this);
		}
		base.StatusEffects.Clear();
		if (self)
		{
			if (IsTooLow())
			{
				Death.Summon("glitch", Respawn);
				yield break;
			}
			sylladex.EjectItems();
			Death.Summon("death", Respawn);
		}
		else if (sync.Equals(null))
		{
			ImageEffects.FadingShadow fadingShadow = base.gameObject.AddComponent<ImageEffects.FadingShadow>();
			fadingShadow.sprites = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
			fadingShadow.delta = 0.5f;
			fadingShadow.color = new Color(0f, 0f, 0f, 0.5f);
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Respawn()
	{
		KeyboardControl.Unblock();
		Fader.instance.BeginFade(-1);
		SyncedHeal(base.HealthMax - base.Health);
		MoveToSpawn(base.RegionChild.Area);
	}

	public override bool IsValidTarget(Attackable target)
	{
		return this != target;
	}

	public bool IsDestroyed()
	{
		if (base.RegionChild.Area is House house)
		{
			return house.IsDestroyed;
		}
		return false;
	}

	private void OnRegionChanged(WorldRegion oldRegion, WorldRegion newRegion)
	{
		if (TryGetComponent<CharacterController>(out var component) && component.enabled)
		{
			component.enabled = false;
			component.enabled = true;
		}
		Planet planet = ((newRegion == null) ? null : newRegion.GetComponentInParent<Planet>());
		Planet planet2 = ((oldRegion == null) ? null : oldRegion.GetComponentInParent<Planet>());
		if (planet != planet2)
		{
			if (planet != null)
			{
				planet.AddPlayer(this);
			}
			if (planet2 != null)
			{
				planet2.RemovePlayer(this);
			}
		}
		if (HasKernelSprite)
		{
			kernelSprite.Warp();
		}
	}

	private void OnAreaChanged(WorldArea oldArea, WorldArea newArea)
	{
		if (base.isLocalPlayer)
		{
			GlobalChat.MoveLocationBall(newArea.Id);
		}
		if (newArea is Dungeon)
		{
			WorldManager.SetAreaActive(newArea.gameObject, to: true);
		}
		if (oldArea is Dungeon && GetAll().All((Player p) => p.RegionChild.Area != oldArea))
		{
			WorldManager.SetAreaActive(oldArea.gameObject, to: false);
		}
	}

	public void Update()
	{
		if (updateCacheText)
		{
			updateCacheText = false;
			if (GristCache != null)
			{
				GristCache.text = Sylladex.MetricFormat(Grist[global::Grist.SpecialType.Build]);
			}
			if (self)
			{
				playerUi.SetGrist(Grist[global::Grist.SpecialType.Build]);
			}
		}
		if (weapon != null)
		{
			weapon.TricksterActions(this);
		}
		if (Vim <= 0f)
		{
			Affect(new Fatigue(2f, 1.5f), stacking: false);
		}
	}

	private IEnumerator ArmorUpdate()
	{
		WaitForSeconds wait = new WaitForSeconds(2f);
		while (true)
		{
			yield return wait;
			NormalItem[] array = armor;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.ArmorUpdate(this);
			}
		}
	}

	private void OnDamage(Attack attack)
	{
		if (attack.source != null)
		{
			NormalItem[] array = armor;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.OnDamage(this, attack.source, attack.damage);
			}
		}
		if (self && !BuildExploreSwitcher.IsExploring && attack.damage > 0f)
		{
			BuildExploreSwitcher.Instance.SwitchToExplore();
		}
	}

	public int GetID()
	{
		return sync.np.id;
	}

	public static PlayerData GetSaveData()
	{
		PlayerData result = default(PlayerData);
		result.name = MultiplayerSettings.playerName;
		result.character = ChangeSpritePart.LoadCharacterStatic();
		result.role = ((ClassPick.chosen == Class.Count) ? ((Class)UnityEngine.Random.Range(0, 12)) : ClassPick.chosen);
		result.aspect = ((AspectPick.chosen == Aspect.Count) ? ((Aspect)UnityEngine.Random.Range(0, 12)) : AspectPick.chosen);
		result.armor = ((IEnumerable<LDBItem>)ItemDownloader.Instance.GetItems(ChangeSpritePart.clothes)).Select((Func<LDBItem, Item>)((LDBItem dbItem) => (NormalItem)dbItem)).ToArray();
		return result;
	}

	public new PlayerData Save()
	{
		PlayerData result = default(PlayerData);
		result.name = sync.np.name;
		result.character = sync.np.character;
		result.level = Level;
		result.experience = experience;
		result.grist = Grist.Save();
		result.role = classpect.role;
		result.aspect = classpect.aspect;
		result.kernelSprite = (HasKernelSprite ? kernelSprite.Save() : null);
		result.armor = ((IEnumerable<Item>)armor).ToArray();
		return result;
	}

	public void LoadUnsynced(HostLoadRequest request)
	{
		sylladex.Load(request.sylladex);
		Exile.Load(request.exile);
	}

	public void Load(PlayerData data)
	{
		sync.np.name = data.name;
		sync.np.character = data.character;
		sync.ApplyLooks();
		if (data.grist != null)
		{
			Grist.Load(data.grist);
		}
		classpect.role = data.role;
		classpect.aspect = data.aspect;
		Level = data.level;
		experience = data.experience;
		if (data.armor != null)
		{
			foreach (NormalItem item in data.armor.OfType<NormalItem>())
			{
				SetArmor((int)item.armor, item);
			}
		}
		if (data.kernelSprite != null && NetworkServer.active)
		{
			MakeKernelSprite().Load(data.kernelSprite);
		}
	}

	private void OnLevelChange(uint before, uint after)
	{
		RefreshLevel();
	}

	private IEnumerable<Ability> RefreshLevel()
	{
		List<Ability> list = new List<Ability>();
		while (prevLevel < level)
		{
			base.HealthMax = Mathf.Round(base.HealthMax * 1.2f / 5f) * 5f;
			Offense *= 1.3f;
			Strength += 0.2f;
			prevLevel++;
			Ability ability = AddAspectAbilities(prevLevel);
			if (ability != null)
			{
				list.Add(ability);
			}
		}
		if (self)
		{
			playerUi.SetLevel(level);
		}
		if (list.Count != 0 && sylladex != null)
		{
			RefreshAbilityButtons();
		}
		return list;
	}

	public ICollection<Echeladder.Change> LevelUp()
	{
		if (experience < MaxExperience)
		{
			return null;
		}
		List<Echeladder.Change> list = new List<Echeladder.Change>();
		while (experience >= MaxExperience)
		{
			experience -= MaxExperience;
			uint num = Level + 1;
			Level = num;
			Ability ability = RefreshLevel().FirstOrDefault();
			boonBucks += 10 * Level * Level;
			list.Add(new Echeladder.Change(this, ability));
		}
		playerUi.UpdateExperience(experience, MaxExperience);
		CmdSetLevel(Level);
		sync.CmdSetTrigger("Dance");
		return list;
	}

	[Command]
	private void CmdSetLevel(uint to)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(to);
		SendCommandInternal(typeof(Player), "CmdSetLevel", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public bool AcceptItem(Item item)
	{
		if (self)
		{
			if (item.SceneObject.TryGetComponent<ConsumeAction>(out var component))
			{
				component.Execute();
				return true;
			}
			return sylladex.AcceptItem(item);
		}
		return false;
	}

	public void Hover(Item item)
	{
	}

	public Rect GetItemRect()
	{
		Vector3 vector = MSPAOrthoController.main.WorldToScreenPoint(GetComponentInChildren<SpriteRenderer>().bounds.center);
		vector.x -= 32f;
		vector.y -= 40f;
		return new Rect(vector, new Vector2(64f, 80f));
	}

	public bool IsActive(Item item)
	{
		if (base.gameObject.activeInHierarchy)
		{
			return self;
		}
		return false;
	}

	[Server]
	private void OnGristChange(int index, int before, int after)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Player::OnGristChange(System.Int32,System.Int32,System.Int32)' called when server was not active");
		}
		else
		{
			RpcSetGrist(index, after - before);
		}
	}

	[ClientRpc]
	private void RpcSetGrist(int index, int diff)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(index);
		writer.WriteInt(diff);
		SendRPCInternal(typeof(Player), "RpcSetGrist", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public Vector3 GetPosition(bool local = false)
	{
		return (local ? base.transform.localPosition : base.transform.position) - footOffset;
	}

	public Vector3 GetFootOffset()
	{
		return footOffset;
	}

	public void SetPosition(Transform to)
	{
		Transform root = to.root;
		Vector3 to2 = root.InverseTransformPoint(to.position);
		SetPosition(to2, root.GetComponent<WorldArea>());
	}

	public void MoveToSpawn(WorldArea area)
	{
		SetPosition(area.SpawnPosition, area);
	}

	public void SetPosition(Vector3 to, WorldArea area)
	{
		DungeonEntrance.lastUsed = Time.fixedUnscaledTime;
		if (area != base.RegionChild.Area)
		{
			if (NetworkServer.active)
			{
				base.RegionChild.Area = area;
			}
			else
			{
				CmdSetArea(area);
			}
		}
		if (base.netId == 0 || base.hasAuthority)
		{
			base.transform.localPosition = to + footOffset;
		}
		else if (NetworkServer.active)
		{
			GetComponent<NetworkTransform>().ServerTeleport(to + footOffset);
		}
	}

	public void SetPosition(Vector3 to, bool fromFeet = true)
	{
		DungeonEntrance.lastUsed = Time.fixedUnscaledTime;
		if (fromFeet)
		{
			base.transform.position = to + footOffset;
		}
		else
		{
			base.transform.position = to;
		}
	}

	[Command]
	private void CmdSetArea(NetworkBehaviour area)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteNetworkBehaviour(area);
		SendCommandInternal(typeof(Player), "CmdSetArea", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public override void OnStrifeStart()
	{
		base.OnStrifeStart();
		if (self)
		{
			sylladex.OnStrifeStart(base.Enemies);
		}
	}

	public override void OnStrifeEnd()
	{
		base.OnStrifeEnd();
		if (self)
		{
			sylladex.OnStrifeEnd();
		}
	}

	public NormalItem GetArmor(int kind)
	{
		return armor[kind];
	}

	[Command]
	public void CmdSetArmor(ArmorKind kind, NormalItem to)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_ArmorKind(writer, kind);
		writer.WriteNormalItem(to);
		SendCommandInternal(typeof(Player), "CmdSetArmor", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcSetArmor(ArmorKind kind, NormalItem to)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_ArmorKind(writer, kind);
		writer.WriteNormalItem(to);
		SendRPCInternal(typeof(Player), "RpcSetArmor", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	public void SetArmor(int kind, NormalItem to)
	{
		if (kind >= 5)
		{
			return;
		}
		NormalItem normalItem = armor[kind];
		if (normalItem != null)
		{
			base.Defense += 0f - normalItem.Power;
			if (kind == 4)
			{
				Speed /= 2f / (1f + Mathf.Pow(2f, 0f - normalItem.Speed));
			}
			sync.DisableArmor(normalItem.armor);
			normalItem.ArmorUnset(this);
		}
		if (to != null)
		{
			base.Defense += to.Power;
			if (kind == 4)
			{
				Speed *= 2f / (1f + Mathf.Pow(2f, 0f - to.Speed));
			}
			sync.ChangeArmor(to);
			to.ArmorSet(this);
		}
		armor[kind] = to;
	}

	public bool SetWeapon(NormalItem weapon)
	{
		if (this.weapon == weapon)
		{
			return false;
		}
		CmdSetWeapon(weapon);
		return true;
	}

	[Command]
	private void CmdSetWeapon(NormalItem weapon)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteNormalItem(weapon);
		SendCommandInternal(typeof(Player), "CmdSetWeapon", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnWeaponChanged(NormalItem from, NormalItem to)
	{
		basicAttack.SetOnHit(to != null && !to.IsRanged());
		if (to != null)
		{
			basicAttack.animation = to.animation;
			if (to.IsRanged())
			{
				bullet.damage = to.Power;
				bullet.weapon = to;
				bullet.RefreshSprite();
			}
			Material material = sync.GetMaterial(to.HasTag(NormalItem.Tag.Colored));
			weaponAnimator.SetWeapon(to, material);
		}
		else
		{
			basicAttack.animation = "Confused";
		}
		var (text, color) = NormalItem.GetAbilityBox(to);
		basicAttack.name = text;
		if (sylladex != null)
		{
			RefreshAbilityButtons();
			sylladex.SetBasicAttack(text, color);
		}
		basicAttack.description = ((to == null) ? "Attempt to attack without a strife weapon." : (basicAttack.name + " with your " + to.GetItemName() + "."));
	}

	private void OnHit(Attack attack)
	{
		if (!attack.isRanged)
		{
			weapon.OnHit(this, attack.target);
		}
	}

	public void RegenItem(Item item, float time)
	{
		StartCoroutine(RegenItemCoroutine(item, time));
	}

	private IEnumerator RegenItemCoroutine(Item item, float time)
	{
		yield return new WaitForSeconds(time);
		sylladex.strifeSpecibus.AcceptItem(item);
	}

	[Server]
	public KernelSprite MakeKernelSprite(Vector3? position = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'KernelSprite Player::MakeKernelSprite(System.Nullable`1<UnityEngine.Vector3>)' called when server was not active");
			return null;
		}
		if (!HasKernelSprite)
		{
			kernelSprite = UnityEngine.Object.Instantiate(kernelSprite);
			kernelSprite.SetTarget(this);
			if (position.HasValue)
			{
				kernelSprite.GetComponent<NavMeshAgent>().Warp(position.Value);
			}
			NetworkServer.Spawn(kernelSprite.gameObject);
		}
		else
		{
			Debug.LogWarning($"Tried to give player {GetID()} a second kernelsprite!");
		}
		return kernelSprite;
	}

	[Client]
	public void SetKernelSprite(KernelSprite to)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void Player::SetKernelSprite(KernelSprite)' called when client was not active");
		}
		else
		{
			kernelSprite = to;
		}
	}

	public bool HasEntered()
	{
		return kernelSprite.HasEntered();
	}

	public void Entered()
	{
		if (HasKernelSprite)
		{
			kernelSprite.Entered();
		}
		if (base.isLocalPlayer)
		{
			Ui.Find("Button Bar/Map").gameObject.SetActive(value: true);
		}
	}

	public void SetClasspect(Aspect aspect, Class role)
	{
		Debug.LogError($"Changed aspect to {aspect}." + " Do not use this in a multiplayer session!");
		classpect.aspect = aspect;
		classpect.role = role;
		abilities.RemoveRange(3, abilities.Count - 3);
		base.StatusEffects.RemoveAll<ClasspectAbility>();
		for (uint num = 1u; num <= Level; num++)
		{
			AddAspectAbilities(num);
		}
		RefreshAbilityButtons();
	}

	private Ability AddAspectAbilities(uint level)
	{
		Ability ability = null;
		switch (classpect.aspect)
		{
		case Aspect.Space:
			switch (level)
			{
			case 2u:
				return new SpaceAbility(this);
			case 3u:
				ability = new Teleport(this);
				break;
			case 4u:
				ability = new Push(this, 3f, 6f);
				break;
			}
			break;
		case Aspect.Time:
			switch (level)
			{
			case 2u:
				return new TimeAbility(this);
			case 3u:
				ability = new Speedify(this, 3f, 3f);
				break;
			case 4u:
				ability = new Clone(this);
				break;
			}
			break;
		case Aspect.Light:
			switch (level)
			{
			case 2u:
				return new LightAbility(this);
			case 3u:
				ability = new Stun(this, "Photorefractive Keractectomy", 3f, 4f, 2f, Color.yellow);
				break;
			case 4u:
				ability = new Laser(this);
				break;
			}
			break;
		case Aspect.Void:
			switch (level)
			{
			case 2u:
				return new VoidAbility(this);
			case 3u:
				ability = new RandomShoot(this, new float[3] { 4f, 8f, 10f });
				break;
			case 4u:
				ability = new Invisible(this, 0.3f, 0.5f);
				break;
			}
			break;
		case Aspect.Mind:
			switch (level)
			{
			case 2u:
				return new MindAbility(this);
			case 3u:
				ability = new Stun(this, "BrainWash", 6f, 6f, 1.5f, Color.cyan, 90f);
				break;
			}
			break;
		case Aspect.Heart:
			switch (level)
			{
			case 2u:
				return new HeartAbility(this);
			case 3u:
				ability = new Astonish(this, 4f);
				break;
			case 4u:
				ability = new Shock(this, 2f, 0.2f, 4f);
				break;
			}
			break;
		case Aspect.Hope:
			switch (level)
			{
			case 2u:
				return new HopeAbility(this);
			case 3u:
				ability = new ReflectiveShieldAbility(this, 0.7f, 3f);
				break;
			}
			break;
		case Aspect.Rage:
			switch (level)
			{
			case 2u:
				return new RageAbility(this);
			case 3u:
				ability = new AttackStun(this, 0.6f, 1.5f, 3f);
				break;
			case 4u:
				ability = new Turmoil(this);
				break;
			}
			break;
		case Aspect.Blood:
			switch (level)
			{
			case 2u:
				return new BloodAbility(this);
			case 3u:
				ability = new Convert(this, 3f);
				break;
			}
			break;
		case Aspect.Breath:
			switch (level)
			{
			case 2u:
				return new BreathAbility(this);
			case 3u:
				ability = new Fly(this);
				break;
			}
			break;
		case Aspect.Life:
			switch (level)
			{
			case 2u:
				return new LifeAbility(this);
			case 3u:
				ability = new Heal(this, 10f);
				break;
			case 4u:
				ability = new Lightbeam(this, 4f, 5f);
				break;
			}
			break;
		case Aspect.Doom:
			switch (level)
			{
			case 2u:
				return new DoomAbility(this);
			case 3u:
				ability = new Weaken(this, 6f, 4f, 4f, 2f);
				break;
			case 4u:
				ability = new Wrath(this, 1.5f, 6f);
				break;
			}
			break;
		}
		if (ability != null)
		{
			abilities.Add(ability);
		}
		return ability;
	}

	static Player()
	{
		loadedPlayerData = null;
		all = new List<Player>();
		Effectiveness = Animator.StringToHash("Effectiveness");
		RemoteCallHelper.RegisterCommandDelegate(typeof(Player), "CmdDoAbility", InvokeUserCode_CmdDoAbility, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Player), "CmdSetLevel", InvokeUserCode_CmdSetLevel, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Player), "CmdSetArea", InvokeUserCode_CmdSetArea, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Player), "CmdSetArmor", InvokeUserCode_CmdSetArmor, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Player), "CmdSetWeapon", InvokeUserCode_CmdSetWeapon, requiresAuthority: true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(Player), "RpcSetGrist", InvokeUserCode_RpcSetGrist);
		RemoteCallHelper.RegisterRpcDelegate(typeof(Player), "RpcSetArmor", InvokeUserCode_RpcSetArmor);
	}

	private void MirrorProcessed()
	{
	}

	private void UserCode_CmdDoAbility(int index, Attackable target, Vector3? position)
	{
		Ability ability = abilities[index];
		if (ability.IsAvailable())
		{
			ability.Execute(target, position);
		}
	}

	protected static void InvokeUserCode_CmdDoAbility(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoAbility called on client.");
		}
		else
		{
			((Player)obj).UserCode_CmdDoAbility(reader.ReadInt(), reader.ReadNetworkBehaviour<Attackable>(), reader.ReadNullable());
		}
	}

	private void UserCode_CmdSetLevel(uint to)
	{
		Level = to;
	}

	protected static void InvokeUserCode_CmdSetLevel(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLevel called on client.");
		}
		else
		{
			((Player)obj).UserCode_CmdSetLevel(reader.ReadUInt());
		}
	}

	private void UserCode_RpcSetGrist(int index, int diff)
	{
		if (diff > 0)
		{
			if (self)
			{
				playerUi.ShowGristCollect(index, diff, base.transform.position);
			}
			SoundEffects.Instance.Grist(base.transform.position);
		}
		if (base.isClientOnly)
		{
			GristCollection grist = Grist;
			grist.OnGristChange = (GristCollection.OnChangeHandler)Delegate.Remove(grist.OnGristChange, new GristCollection.OnChangeHandler(OnGristChange));
			Grist[index] += diff;
			GristCollection grist2 = Grist;
			grist2.OnGristChange = (GristCollection.OnChangeHandler)Delegate.Combine(grist2.OnGristChange, new GristCollection.OnChangeHandler(OnGristChange));
		}
		if (index == 0)
		{
			if (GristCache != null)
			{
				GristCache.text = Sylladex.MetricFormat(Grist[index]);
			}
			if (self)
			{
				playerUi.SetGrist(Grist[index]);
			}
		}
	}

	protected static void InvokeUserCode_RpcSetGrist(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetGrist called on server.");
		}
		else
		{
			((Player)obj).UserCode_RpcSetGrist(reader.ReadInt(), reader.ReadInt());
		}
	}

	private void UserCode_CmdSetArea(NetworkBehaviour area)
	{
		base.RegionChild.Area = (WorldArea)area;
	}

	protected static void InvokeUserCode_CmdSetArea(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetArea called on client.");
		}
		else
		{
			((Player)obj).UserCode_CmdSetArea(reader.ReadNetworkBehaviour());
		}
	}

	public void UserCode_CmdSetArmor(ArmorKind kind, NormalItem to)
	{
		RpcSetArmor(kind, to);
	}

	protected static void InvokeUserCode_CmdSetArmor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetArmor called on client.");
		}
		else
		{
			((Player)obj).UserCode_CmdSetArmor(GeneratedNetworkCode._Read_ArmorKind(reader), reader.ReadNormalItem());
		}
	}

	private void UserCode_RpcSetArmor(ArmorKind kind, NormalItem to)
	{
		SetArmor((int)kind, to);
	}

	protected static void InvokeUserCode_RpcSetArmor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetArmor called on server.");
		}
		else
		{
			((Player)obj).UserCode_RpcSetArmor(GeneratedNetworkCode._Read_ArmorKind(reader), reader.ReadNormalItem());
		}
	}

	private void UserCode_CmdSetWeapon(NormalItem weapon)
	{
		Networkweapon = weapon;
	}

	protected static void InvokeUserCode_CmdSetWeapon(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetWeapon called on client.");
		}
		else
		{
			((Player)obj).UserCode_CmdSetWeapon(reader.ReadNormalItem());
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNormalItem(weapon);
			writer.WriteUInt(level);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteNormalItem(weapon);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteUInt(level);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			NormalItem normalItem = weapon;
			Networkweapon = reader.ReadNormalItem();
			if (!SyncVarEqual(normalItem, ref weapon))
			{
				OnWeaponChanged(normalItem, weapon);
			}
			uint num = level;
			Networklevel = reader.ReadUInt();
			if (!SyncVarEqual(num, ref level))
			{
				OnLevelChange(num, level);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			NormalItem normalItem2 = weapon;
			Networkweapon = reader.ReadNormalItem();
			if (!SyncVarEqual(normalItem2, ref weapon))
			{
				OnWeaponChanged(normalItem2, weapon);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			uint num3 = level;
			Networklevel = reader.ReadUInt();
			if (!SyncVarEqual(num3, ref level))
			{
				OnLevelChange(num3, level);
			}
		}
	}
}
