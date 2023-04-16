using System;

namespace Assets.Multiplayer.Scripts.commands;

internal class PlayerCheats : CommandBase
{
	private const string classpecthelp = "/classpect aspect class\nChanges the player's classpect. This is only meant for testing and will break multiplayer sessions.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("classpect", new PlayerCheats().SetSafe(safe: false).SetHelp("/classpect aspect class\nChanges the player's classpect. This is only meant for testing and will break multiplayer sessions.\n"));
	}

	public override void RunCommand(string command)
	{
		string[] array = command.Split(' ');
		Player.player.SetClasspect((Aspect)Enum.Parse(typeof(Aspect), array[1]), (Class)Enum.Parse(typeof(Class), array[2]));
	}
}
