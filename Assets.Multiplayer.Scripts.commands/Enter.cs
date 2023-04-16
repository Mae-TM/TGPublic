using System;

namespace Assets.Multiplayer.Scripts.commands;

internal class Enter : CommandBase
{
	private string enterhelp = "/enter <buzzword1> <buzzword2> <aspect>\nEnters the player into the session. Will screw up the session due to a lack of certain variables set.\nThe arguments can be used to get a specific land by giving the names of the buzzword files and the aspect they belong to.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("enter", new Enter().SetHelp(enterhelp));
	}

	public override void RunCommand(string command)
	{
		string[] array = command.Split(' ');
		int num = array.Length;
		if (num <= 1)
		{
			if (Player.player.RegionChild.Area is House house)
			{
				house.Enter();
			}
			return;
		}
		if (Enum.TryParse<Aspect>(array[num - 1], ignoreCase: true, out var result))
		{
			num--;
		}
		else
		{
			result = Aspect.Count;
		}
		string file = null;
		string file2 = null;
		if (num >= 2)
		{
			file = array[1];
			if (num >= 3)
			{
				file2 = array[2];
			}
		}
		if (Player.player.RegionChild.Area is House house2)
		{
			house2.Enter(fromLoad: false, file, file2, result);
		}
	}
}
