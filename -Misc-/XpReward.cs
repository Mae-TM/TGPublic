using Quest.NET.Interfaces;

public class XpReward : IReward
{
	private readonly int amount;

	public XpReward(int amount)
	{
		this.amount = amount;
	}

	public override void GrantReward()
	{
		base.GrantReward();
		Player.player.Experience += amount;
	}

	public override string ToString()
	{
		return $"{amount} XP";
	}
}
