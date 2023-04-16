namespace Assets.Multiplayer.Scripts.commands;

internal class Movement : CommandBase
{
	private string flyhelp = "/fly\nToggles flying, allows the player to jump while already in the air.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("fly", new Movement().SetSafe(safe: true).SetHelp(flyhelp));
	}

	public override void RunCommand(string command)
	{
		Player.player.GetComponent<PlayerMovement>().ToggleFly();
	}
}
