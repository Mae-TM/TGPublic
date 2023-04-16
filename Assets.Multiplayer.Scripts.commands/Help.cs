using System;
using System.Collections.Generic;

namespace Assets.Multiplayer.Scripts.commands;

internal class Help : CommandBase
{
	private string helphelp = "/help command\nReveals help for a specific command. If no command is given, prints a list of commands instead.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("help", new Help().SetSafe(safe: true).SetHelp(helphelp));
	}

	public override void RunCommand(string command)
	{
		string[] array = command.Split(' ');
		if (array.Length > 1)
		{
			try
			{
				if (CommandBase.commands.ContainsKey(array[1]))
				{
					if (!CommandBase.commands[array[1]].safe)
					{
						Message("<color=red>This is an extremely dangerous command. Use with caution, only when testing.</color>");
					}
					Message(CommandBase.commands[array[1]].help);
				}
				else
				{
					Message("No such command: " + array[1] + "\n");
				}
				return;
			}
			catch (Exception ex)
			{
				Message("<color=red>Could not find help, exception occured: " + ex.ToString() + "</color>\n");
				return;
			}
		}
		Message("This is a list of all the commands. Those marked in red are dangerous, and can permanently make your session unplayable. Run /help commandname to get more information about what a command does.");
		foreach (KeyValuePair<string, CommandBase> command2 in CommandBase.commands)
		{
			Message((command2.Value.safe ? "" : "<color=red>") + "/" + command2.Key + (command2.Value.safe ? "" : "</color>"));
		}
		Message("");
	}
}
