namespace Assets.Multiplayer.Scripts.commands;

internal class CreatureCheats : CommandBase
{
	private const string creaturehelp = "/spawn Name <number>\nSpawns the given creature at your current position. If a number is given, it will spawn that many, otherwise it only spawns one.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("spawn", new CreatureCheats().SetSafe(safe: true).SetHelp("/spawn Name <number>\nSpawns the given creature at your current position. If a number is given, it will spawn that many, otherwise it only spawns one.\n"));
	}

	public override void RunCommand(string command)
	{
		string[] array = command.Split(' ');
		if (array.Length <= 2 || !int.TryParse(array[2], out var result))
		{
			result = 1;
		}
		for (int i = 0; i < result; i++)
		{
			SpawnHelper.instance.Spawn(array[1], Player.player.RegionChild.Area, Player.player.GetPosition());
		}
	}
}
