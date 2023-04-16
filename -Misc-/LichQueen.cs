using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class LichQueen : Boss
{
	private class SummonHelp : Ability
	{
		private readonly Attackable[] underlings = SpawnHelper.instance.GetCreatures<Attackable>(new string[3] { "Imp", "Gremlin", "Lich" });

		private readonly Transform[] spawnPoints;

		public SummonHelp(LichQueen caster, Transform[] spawnPoints)
			: base(caster, null, "Summon Help", 1f, "Scream")
		{
			this.spawnPoints = spawnPoints;
			audio = caster.shortScream;
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!NetworkServer.active)
			{
				return true;
			}
			caster.StartCoroutine(Coroutine(target as Attacking));
			return true;
		}

		private IEnumerator Coroutine(Attacking target)
		{
			WaitForSeconds delay = new WaitForSeconds(0.5f);
			Action<Attackable> spawnFunc = null;
			if ((object)target != null)
			{
				spawnFunc = delegate(Attackable att)
				{
					att.Affect(new TauntEffect(1f, target));
				};
			}
			int i = 0;
			while (i < 10)
			{
				Attackable prefab = underlings[i % underlings.Length];
				Vector3 position = spawnPoints[i % spawnPoints.Length].position;
				int index = Grist.GetIndex(0, (Aspect)UnityEngine.Random.Range(0, 12));
				SpawnHelper.instance.Spawn(prefab, caster.RegionChild.Area, position, index, spawnFunc);
				yield return delay;
				int num = i + 1;
				i = num;
			}
		}
	}

	private class MovePlatforms : Ability
	{
		private readonly Platform[] platforms;

		public MovePlatforms(LichQueen caster, Platform[] platforms)
			: base(caster, null, "Move Platforms", 5f, "Slam", isOnHit: true)
		{
			this.platforms = platforms;
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			bool state = !platforms[0].GetState();
			Platform[] array = platforms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetState(state);
			}
			return true;
		}

		protected override bool OnHit(Attackable target, float multiplier = 1f)
		{
			return target.Damage(15f * multiplier, caster) > 0f;
		}
	}

	private class Slash : Ability
	{
		public Slash(LichQueen caster, string nanimation = null)
			: base(caster, null, "Slash", 1f, nanimation, isOnHit: true)
		{
			audio = caster.scream;
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			AnimatorRepetitions.SetRepetitions(animator, 4);
			return true;
		}

		protected override bool OnHit(Attackable target, float multiplier = 1f)
		{
			return target.Damage(15f * multiplier, caster) > 0f;
		}
	}

	[SerializeField]
	private Transform[] spawnPoints;

	[SerializeField]
	private Platform[] platforms;

	[SerializeField]
	private Animator table;

	[SerializeField]
	private new Animator animator;

	[SerializeField]
	private AudioClip music;

	[SerializeField]
	private AudioClip scream;

	[SerializeField]
	private AudioClip shortScream;

	private int lastMilestone = -1;

	private static readonly int hurtHash = Animator.StringToHash("Hurt");

	private static readonly int ruinedHash = Animator.StringToHash("ruined");

	public override AudioClip StrifeClip => music;

	protected override void Awake()
	{
		base.Awake();
		base.Faction.Parent = "Derse";
		abilities.Add(new SummonHelp(this, spawnPoints));
		abilities.Add(new MovePlatforms(this, platforms));
		abilities.Add(new Slash(this, "FrenzySlash"));
		base.HealthMax = 500f;
		base.Health = base.HealthMax;
		base.OnHurt += OnDamage;
	}

	private void OnDamage(Attack attack)
	{
		int num = (int)(base.HealthMax - base.Health) / 25;
		if (num > lastMilestone)
		{
			animator.SetBool(hurtHash, base.Health < base.HealthMax / 2f);
			lastMilestone = num;
			int num2 = num % abilities.Count;
			abilities[num2].Execute(attack.source);
			if (num2 == 2)
			{
				table.SetTrigger(ruinedHash);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
