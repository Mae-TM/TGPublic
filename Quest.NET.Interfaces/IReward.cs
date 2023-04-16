using ProtoBuf;

namespace Quest.NET.Interfaces;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public abstract class IReward
{
	protected bool _granted;

	public bool Granted => _granted;

	public virtual void GrantReward()
	{
		_granted = true;
	}
}
