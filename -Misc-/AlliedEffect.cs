using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class AlliedEffect : StatusEffect
{
	private readonly Attackable ally;

	private Faction oldFaction;

	public AlliedEffect(float duration, Attackable ally)
		: base(duration)
	{
		this.ally = ally;
	}

	public AlliedEffect(AlliedEffect copy, float duration)
		: base(duration)
	{
		ally = copy.ally;
	}

	public override void Begin(Attackable att)
	{
		if (att is Enemy enemy && ally is Player player)
		{
			enemy.SetMaterial(player.sync.GetMaterial());
		}
		att.LeaveStrife();
		oldFaction = att.Faction.Parent;
		att.Faction.Parent = ally.Faction.Parent;
		if (att is StrifeAI strifeAI)
		{
			strifeAI.ClearTarget();
		}
	}

	public override void End(Attackable att)
	{
		if (att is Enemy enemy)
		{
			enemy.SetMaterial();
		}
		if (att is StrifeAI strifeAI)
		{
			strifeAI.ClearTarget();
		}
		att.Faction.Parent = oldFaction;
		if (!att.IsInStrife)
		{
			return;
		}
		foreach (Attackable enemy2 in att.Enemies)
		{
			if (enemy2 is StrifeAI strifeAI2)
			{
				strifeAI2.ClearTarget(att);
			}
		}
		att.LeaveStrife();
	}
}
