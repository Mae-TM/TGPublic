using Quest.NET.Interfaces;

public class GristReward : IReward
{
	private int _amount;

	private int _type;

	public int Amount => _amount;

	public int Type => _type;

	public GristReward(int type, int amount)
	{
		_type = type;
		_amount = amount;
	}

	public override void GrantReward()
	{
		base.GrantReward();
		Player.player.Grist[Type] += Amount;
	}

	public override string ToString()
	{
		return $"{Amount} {Grist.GetName(Type)} Grist";
	}
}
