using Steamworks;

namespace Assets.Multiplayer.Scripts.commands;

internal class ResetAch : CommandBase
{
	private string resethelp = "This command will clear all stats and achievements that are applied on the current Steam user.";

	public override void Init()
	{
		CommandBase.RegisterCommand("resetach", new ResetAch().SetHelp(resethelp).SetSafe(safe: true));
	}

	public override void RunCommand(string command)
	{
		if (SteamUserStats.ResetAll(includeAchievements: true))
		{
			Message("Stat clearing successful!");
		}
		else
		{
			Message("Stat clearing unsuccessful, please try again!");
		}
	}
}
