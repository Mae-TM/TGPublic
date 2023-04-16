using TheGenesisLib.Models;
using UnityEngine;

public class Basilisk : Enemy
{
	protected override void Start()
	{
		base.HealthMax = 35f;
		HealthRegen = 1.5f;
		if (Random.value > 0.6f)
		{
			healthDrop = 0;
		}
		base.Start();
		bullet.GetComponent<Light>().color = Grist.GetColor(base.GristType);
		if (TryGetAttackTag(out var tag))
		{
			LDBItem lDBItem = AbstractSingletonManager<DatabaseManager>.Instance.FindItem(WeaponKind.Ball, tag, (int)AttackDamage, (int)AttackSpeed);
			if (lDBItem != null)
			{
				bullet.weapon = new NormalItem(lDBItem);
				bullet.RefreshSprite();
			}
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
