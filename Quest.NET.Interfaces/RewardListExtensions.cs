using System.Collections.Generic;

namespace Quest.NET.Interfaces;

public static class RewardListExtensions
{
	public static void GrantRewards(this List<IReward> rewards)
	{
		foreach (IReward reward in rewards)
		{
			reward.GrantReward();
		}
	}
}
