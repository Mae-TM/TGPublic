using Quest.NET.Interfaces;

public class BoonBuckReward : IReward
{
	private readonly int amount;

	public BoonBuckReward(int amount)
	{
		this.amount = amount;
	}

	public override void GrantReward()
	{
		base.GrantReward();
		Player.player.boonBucks += amount;
	}

	public override string ToString()
	{
		return $"{amount} Boonbucks";
	}
}
