using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Emotes : MonoBehaviour
{
	[SerializeField]
	private Sprite[] emotes;

	[SerializeField]
	private List<Toggle> toggles;

	private void Start()
	{
		for (int i = 0; i < emotes.Length; i++)
		{
			Sprite emote = emotes[i];
			if (i >= toggles.Count)
			{
				toggles.Add(Object.Instantiate(toggles[0], toggles[0].transform.parent));
			}
			Toggle toggle = toggles[i];
			toggle.name = emote.name;
			toggle.transform.GetComponent<Image>().sprite = emote;
			toggle.transform.GetChild(0).GetComponent<Text>().text = emote.name;
			toggle.onValueChanged.AddListener(delegate(bool value)
			{
				if (value)
				{
					Player.player.sync.CmdSetTrigger(emote.name);
				}
			});
		}
		toggles.TrimExcess();
	}

	private void LateUpdate()
	{
		if (!(Player.player == null) && !(Player.player.sync == null))
		{
			toggles[Player.player.sync.GetFaceState()].SetIsOnWithoutNotify(value: true);
		}
	}
}
