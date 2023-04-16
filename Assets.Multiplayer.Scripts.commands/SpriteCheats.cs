namespace Assets.Multiplayer.Scripts.commands;

internal class SpriteCheats : CommandBase
{
	private const string prototypehelp = "/prototype name\nPrototypes the sprite. Only the host can use this command.\nNot guaranteed to work well if the sprite is already prototyped.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("prototype", new SpriteCheats().SetSafe(safe: false).SetHelp("/prototype name\nPrototypes the sprite. Only the host can use this command.\nNot guaranteed to work well if the sprite is already prototyped.\n"));
	}

	public override void RunCommand(string command)
	{
		Player.player.KernelSprite.SetPrototype(new string[2]
		{
			command.Substring("prototype ".Length),
			null
		});
	}
}
