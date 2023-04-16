using ProtoBuf;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
public class RevealEffect : StatusEffect
{
	public RevealEffect(float duration)
		: base(duration)
	{
	}

	public override void Begin(Attackable att)
	{
		att.RegionChild.enabled = false;
		Visibility.Set(att.gameObject, value: true);
	}

	public override void End(Attackable att)
	{
		att.RegionChild.enabled = true;
		att.RegionChild.SetRegion();
	}
}
