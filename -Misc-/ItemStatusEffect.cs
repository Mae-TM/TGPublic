using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class ItemStatusEffect : StatusEffect
{
	private readonly NormalItem item;

	public ItemStatusEffect(NormalItem item, float duration)
		: base(duration)
	{
		this.item = item;
	}

	public override void Begin(Attackable att)
	{
		item.ArmorSet(att, 3f);
	}

	public override float Update(Attackable att)
	{
		item.ArmorUpdate(att, 3f);
		return 2f;
	}

	public override bool OnAttacked(Attack attack)
	{
		if (attack.target is Attacking self && attack.source != null)
		{
			item.OnDamage(self, attack.source, attack.damage, 3f);
		}
		return false;
	}

	public override void End(Attackable att)
	{
		item.ArmorUnset(att, 3f);
	}
}
