namespace Assets.Multiplayer.Scripts.commands;

internal class Build : CommandBase
{
	private string buildhelp = "/build\nSwitches into sburb building mode.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("build", new Build().SetSafe(safe: true).SetHelp(buildhelp));
	}

	public override void RunCommand(string command)
	{
		BuildExploreSwitcher.Instance.SwitchToBuild(null, Player.player.RegionChild.Area as House);
	}
}
