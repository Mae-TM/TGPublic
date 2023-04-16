using System.Collections.Generic;
using UnityEngine;

public class Bee : StrifeAI
{
	private static readonly List<Bee> bees = new List<Bee>();

	private static readonly int friendlyHash = Animator.StringToHash("friendly");

	protected override void Start()
	{
		base.GristType = Grist.GetIndex(0, Aspect.Doom);
		base.Faction.Parent = base.name;
		base.OnHurt += OnDamage;
		base.Start();
	}

	protected override void OnEnable()
	{
		bees.Add(this);
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		bees.Remove(this);
		base.OnDisable();
	}

	protected new void Update()
	{
		base.Update();
		animator.SetBool(friendlyHash, target == null);
	}

	private static void OnDamage(Attack attack)
	{
		foreach (Bee bee in bees)
		{
			bee.target = attack.source;
		}
	}

	private void MirrorProcessed()
	{
	}
}
