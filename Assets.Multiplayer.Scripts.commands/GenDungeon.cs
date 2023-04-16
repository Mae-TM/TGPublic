namespace Assets.Multiplayer.Scripts.commands;

internal class GenDungeon : CommandBase
{
	private string dungeonhelp = "/dungeon <level> <chunk>\nGenerates and teleports the player into a dungeon. Incredibly dangerous and can screw things up, only used for debugging.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("dungeon", new GenDungeon().SetHelp(dungeonhelp));
	}

	public override void RunCommand(string command)
	{
		string[] array = command.Split(' ');
		int result = 10;
		if (array.Length > 1)
		{
			int.TryParse(array[1], out result);
		}
		int result2 = 0;
		if (array.Length > 2)
		{
			int.TryParse(array[2], out result2);
		}
		if (!(Player.player.RegionChild.Area is House world))
		{
			Message("Can only generate a dungeon when currently in a house area.");
			return;
		}
		Dungeon dungeon = DungeonManager.Build(world, result2, result);
		Message($"Generated dungeon {dungeon.Id}.");
		Player.player.MoveToSpawn(dungeon);
	}
}
