using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Titachnid : Enemy
{
	private class SpawnAbility : Ability
	{
		private List<Attackable> spawned = new List<Attackable>();

		public int spawnCap;

		public SpawnAbility(Attacking ncaster, int spawnCap)
			: base(ncaster, null, "Spawn", 4f, "Attack")
		{
			vimcost = 1f;
			this.spawnCap = spawnCap;
		}

		protected override bool Effect(Attackable target = null, Vector3? position = null, float multiplier = 1f)
		{
			if (!NetworkServer.active)
			{
				return true;
			}
			IStatusEffect effect = caster.GetEffect<AlliedEffect>();
			spawned.RemoveAll((Attackable att) => att == null || !att.enabled);
			for (int i = 0; (float)i < Mathf.Round(6f * multiplier); i++)
			{
				if (spawned.Count >= spawnCap)
				{
					continue;
				}
				spawned.Add(SpawnHelper.instance.Spawn(kheprit, caster.RegionChild.Area, caster.transform.position, caster.GristType, delegate(Attackable att)
				{
					if (effect != null)
					{
						att.Affect(new AlliedEffect((AlliedEffect)effect, float.PositiveInfinity));
					}
				}));
			}
			return true;
		}
	}

	private static Attackable kheprit;

	public int spawnCap = 24;

	protected override void Awake()
	{
		base.Awake();
		base.HealthMax = 40f;
		HealthRegen = 1f;
		if (Random.value > 0.5f)
		{
			healthDrop = 0;
		}
		abilities[0] = new SpawnAbility(this, spawnCap);
		if (kheprit == null)
		{
			SpawnHelper.instance.TryGetCreature("Kheprit", out kheprit);
		}
	}

	public override int GetCost()
	{
		return 4;
	}

	private void MirrorProcessed()
	{
	}
}
