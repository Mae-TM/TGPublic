using System;
using System.IO;
using UnityEngine;

namespace Assets.Multiplayer.Scripts.commands;

internal class ResourceCheats : CommandBase
{
	private string gristhelp = "/grist amount <id> \nAdds grist to the player. The amount parameter is how much grit will be added to the player. The id is the type of grist. Defaults to buildgrist.\n";

	private string xphelp = "/xp number\nAdds XP to the player\n";

	private string boonhelp = "/boon number\nAdds boonbucks to the player\n";

	private string itemhelp = "/item captcha\nAdds the item with the captcha code supplied.\n";

	private string equiphelp = "/equip equipmentset\nDangerous experimental command for equiping a set of armor and tools from a file.\n";

	public override void Init()
	{
		CommandBase.RegisterCommand("grist", new ResourceCheats().SetSafe(safe: true).SetHelp(gristhelp));
		CommandBase.RegisterCommand("xp", new ResourceCheats().SetSafe(safe: true).SetHelp(xphelp));
		CommandBase.RegisterCommand("boon", new ResourceCheats().SetSafe(safe: true).SetHelp(boonhelp));
		CommandBase.RegisterCommand("item", new ResourceCheats().SetSafe(safe: true).SetHelp(itemhelp));
		CommandBase.RegisterCommand("equip", new ResourceCheats().SetSafe(safe: false).SetHelp(equiphelp));
	}

	public override void RunCommand(string command)
	{
		string[] array = command.Split(' ');
		if (array.Length > 1)
		{
			switch (array[0])
			{
			case "grist":
				try
				{
					Player.player.Grist[int.Parse((array.Length == 3) ? array[2] : "0")] += int.Parse(array[1]);
					break;
				}
				catch (Exception)
				{
					Message("<color=red>Error adding grist. Probably no such grist type.</color>\n");
					break;
				}
			case "xp":
				Player.player.Experience += int.Parse(array[1]);
				break;
			case "boon":
				Player.player.boonBucks += int.Parse(array[1]);
				break;
			case "item":
				Player.player.sylladex.AddItem(array[1]);
				break;
			case "equip":
				Equip(array[1]);
				break;
			}
		}
		else
		{
			Message("<color=red>The command  " + command + "requires more than one parameter</color>\n");
		}
	}

	public void Equip(string setName)
	{
		string[] array = File.ReadAllLines(Application.streamingAssetsPath + "/EquipmentSets/" + setName + ".txt");
		foreach (string text in array)
		{
			if (text.Length != 0 && text[0] != '#')
			{
				Item item = new NormalItem(text.Split(' ')[0]);
				Player.player.AcceptItem(item);
			}
		}
	}
}
