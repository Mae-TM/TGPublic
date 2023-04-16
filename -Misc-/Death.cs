using System;
using System.Collections.Generic;
using UnityEngine;

public static class Death
{
	private static Material mat;

	private static Transform prefab;

	public static void Summon(string cause, Action respawn = null)
	{
		string dialoguePath = Dialogue.GetDialoguePath("Death/" + cause);
		if (mat == null)
		{
			mat = ImageEffects.SetShiftColor(new Material(Player.player.sync.GetMaterial()), Color.grey);
		}
		if (prefab == null)
		{
			prefab = SpawnHelper.instance.GetCreature("Death").transform;
		}
		Dialogue.StartDialogue(dialoguePath, mat, prefab);
		Dialogue.OnDone += delegate(ISet<string> labels)
		{
			if (respawn != null && labels.Contains("respawn"))
			{
				Fader.instance.BeginFade(1);
				Fader.instance.fadedToBlack = respawn;
			}
			else
			{
				NetcodeManager.Instance.ButtonQuit(toMenu: true);
			}
		};
	}
}
