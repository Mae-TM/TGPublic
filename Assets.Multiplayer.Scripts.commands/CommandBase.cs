using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Multiplayer.Scripts.commands;

internal class CommandBase
{
	public static Dictionary<string, CommandBase> commands;

	public bool safe;

	public string help = "Someone did a dum-dum and didn't add a command help text";

	public static void RegisterAllCommands()
	{
		commands = new Dictionary<string, CommandBase>();
		foreach (CommandBase item in from t in typeof(CommandBase).Assembly.GetTypes()
			where t.IsSubclassOf(typeof(CommandBase)) && !t.IsAbstract
			select (CommandBase)Activator.CreateInstance(t))
		{
			item.Init();
		}
	}

	public CommandBase SetSafe(bool safe)
	{
		this.safe = safe;
		return this;
	}

	public CommandBase SetHelp(string help)
	{
		this.help = help;
		return this;
	}

	public virtual void Init()
	{
	}

	public virtual void RunCommand(string command)
	{
	}

	public static void RegisterCommand(string name, CommandBase theCommand)
	{
		commands[name] = theCommand;
	}

	protected void Message(string message)
	{
		GlobalChat.WriteCommandMessage(message);
	}
}
