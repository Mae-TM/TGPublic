using UnityEngine;

namespace Assets.Multiplayer.Scripts.commands;

internal class Teleportation : CommandBase
{
	private string homehelp = "/home\nTeleports their player to the current spawnpoint.\n";

	private string homerhelp = "/homer\nTeleports the player to the current worlds spawnpoint (if you are in a dungeon, then the world the dungeon is in).\n";

	private string positionhelp = "/position x y z\nIf given no parameters, prints the players current location.\nIf given three paramters, teleports the player to that location.\n";

	private string dimensionhelp = "/dimension index\nIf given no parameters, prints the players current dimension.\nIf given three paramters, teleports the player to that dimension.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("home", new Teleportation().SetSafe(safe: true).SetHelp(homehelp));
		CommandBase.RegisterCommand("homer", new Teleportation().SetSafe(safe: true).SetHelp(homerhelp));
		CommandBase.RegisterCommand("position", new Teleportation().SetSafe(safe: true).SetHelp(positionhelp));
		CommandBase.RegisterCommand("dimension", new Teleportation().SetSafe(safe: true).SetHelp(dimensionhelp));
	}

	public override void RunCommand(string command)
	{
		string[] array = command.Split(' ');
		if (array[0] == "home")
		{
			Home();
		}
		else if (array[0] == "homer")
		{
			Homer();
		}
		else if (array[0] == "position")
		{
			Position(array);
		}
		else if (array[0] == "dimension")
		{
			Dimension(array);
		}
	}

	private void Home()
	{
		WorldArea area = Player.player.RegionChild.Area;
		Player.player.MoveToSpawn(area);
	}

	private void Homer()
	{
		WorldArea worldArea = Player.player.RegionChild.Area;
		if (worldArea is Dungeon dungeon)
		{
			worldArea = dungeon.World;
		}
		Player.player.MoveToSpawn(worldArea);
	}

	private void Position(string[] commandArgs)
	{
		Message(Player.player.GetPosition(local: true).ToString() + "\n");
		if (commandArgs.Length > 1)
		{
			if (commandArgs.Length > 2 && commandArgs.Length < 5)
			{
				Player.player.SetPosition(new Vector3(float.Parse(commandArgs[1]), float.Parse(commandArgs[2]), float.Parse(commandArgs[3])));
			}
			else
			{
				Message("Invalid amount of arguments. Max 3\n");
			}
		}
	}

	private void Dimension(string[] commandArgs)
	{
		Message(Player.player.RegionChild.Area.Id + "\n");
		if (commandArgs.Length <= 1)
		{
			return;
		}
		if (commandArgs.Length == 2)
		{
			if (int.TryParse(commandArgs[1], out var result))
			{
				Player.player.RegionChild.Area = AbstractSingletonManager<WorldManager>.Instance.GetArea(result);
			}
			else
			{
				Message("Could not parse '" + commandArgs[0] + "'");
			}
		}
		else
		{
			Message("Invalid amount of arguments. Max 1\n");
		}
	}
}
