using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Exile : MonoBehaviour
{
	public enum Action
	{
		start,
		guardian,
		victory,
		disc,
		sburb,
		furniture,
		cruxtruder,
		dowel,
		sprite,
		artifact,
		entry,
		Count
	}

	private const float SHOW_TIME = 4f;

	private const float SEND_TIME = 4f;

	private const float LOOP_TIME = 30f;

	private const float START_TIME = 10f;

	private static float timeLeft = 10f;

	private static uint type = uint.MaxValue;

	private static uint action = 0u;

	private static uint index = 0u;

	private static bool talking = false;

	private static bool repeating = true;

	private readonly string[][] text = new string[11][];

	[SerializeField]
	private GameObject prefab;

	private Text textComponent;

	private void Start()
	{
		FileInfo[] array = StreamingAssets.GetDirectoryContents("Exiles", "*.txt").ToArray();
		if (type == uint.MaxValue)
		{
			type = (uint)UnityEngine.Random.Range(0, array.Length);
		}
		textComponent = prefab.transform.GetChild(0).GetChild(0).GetChild(0)
			.GetComponent<Text>();
		using (StreamReader stream = array[type].OpenText())
		{
			LoadExile(stream);
		}
		BuildExploreSwitcher instance = BuildExploreSwitcher.Instance;
		instance.OnSwitchToExplore = (System.Action)Delegate.Combine(instance.OnSwitchToExplore, (System.Action)delegate
		{
			StopAction(Action.sburb);
			StopAction(Action.entry);
		});
	}

	private void OnDestroy()
	{
		type = uint.MaxValue;
		action = 0u;
		index = 0u;
	}

	private void Update()
	{
		if (timeLeft <= 0f)
		{
			if (!talking || text[action] == null || text[action].Length == 0 || BuildExploreSwitcher.cheatMode)
			{
				return;
			}
			UpdateAction();
			Talk(text[action][index]);
			index++;
			if (index >= text[action].Length)
			{
				index = 0u;
				if (repeating)
				{
					timeLeft += 30f;
				}
				else
				{
					talking = false;
				}
			}
			else
			{
				timeLeft += 4f;
			}
		}
		else
		{
			timeLeft -= Time.deltaTime;
		}
	}

	private void UpdateAction()
	{
		if (!(Player.player == null) && !(Player.player.transform.parent == null))
		{
			Guardian componentInChildren = Player.player.transform.parent.GetComponentInChildren<Guardian>();
			if (componentInChildren != null && (componentInChildren.transform.position - Player.player.transform.position).sqrMagnitude < 36f)
			{
				SetAction(Action.guardian);
			}
			else
			{
				StopAction(Action.guardian);
			}
		}
	}

	public static bool SetAction(Action to, bool instant = true, bool repeat = true)
	{
		if (action == (uint)to)
		{
			talking = true;
		}
		else if (action < (uint)to)
		{
			action = (uint)to;
			index = 0u;
			talking = true;
			repeating = repeat;
			if (instant)
			{
				timeLeft = 0f;
			}
			else
			{
				timeLeft = 10f;
			}
			return true;
		}
		return false;
	}

	public static void StopAction(Action act)
	{
		if (action == (uint)act)
		{
			talking = false;
		}
	}

	private void Talk(string text)
	{
		textComponent.text = text;
		GameObject obj = UnityEngine.Object.Instantiate(prefab, base.transform);
		obj.transform.localPosition = RandomPosition();
		UnityEngine.Object.Destroy(obj, 4f);
	}

	private Vector3 RandomPosition()
	{
		Rect rect = (base.transform as RectTransform).rect;
		return new Vector3(UnityEngine.Random.Range(rect.xMin, rect.xMax), UnityEngine.Random.Range(rect.yMin, rect.yMax));
	}

	public static ExileData Save()
	{
		ExileData result = default(ExileData);
		result.action = (Action)action;
		result.type = type;
		result.isTalking = talking;
		return result;
	}

	public static void Load(ExileData data)
	{
		action = (uint)data.action;
		type = data.type;
		talking = data.isTalking;
	}

	public static void Save(Stream stream)
	{
		HouseLoader.writeUint(type, stream);
		HouseLoader.writeUint(action, stream);
		HouseLoader.writeBool(talking, stream);
	}

	public static void Load(Stream stream)
	{
		type = HouseLoader.readUint(stream);
		action = HouseLoader.readUint(stream);
		talking = HouseLoader.readBool(stream);
	}

	private void LoadExile(StreamReader stream)
	{
		string text = stream.ReadLine();
		Font font = Resources.Load<Font>("Font/" + text);
		string[] oSInstalledFontNames = Font.GetOSInstalledFontNames();
		if (font == null && Array.IndexOf(oSInstalledFontNames, text) != -1)
		{
			font = Font.CreateDynamicFontFromOSFont(text, textComponent.fontSize);
		}
		else if (font == null)
		{
			font = Resources.Load<Font>("Font/FONTSTUCK");
		}
		textComponent.font = font;
		for (uint num = 0u; num < 11; num++)
		{
			List<string> list = new List<string>();
			string text2 = stream.ReadLine();
			while (text2 != null && !text2.StartsWith("Upon"))
			{
				int num2 = text2.IndexOf("//");
				if (num2 != -1)
				{
					text2 = text2.Substring(0, num2);
				}
				if (text2.Length != 0)
				{
					list.Add(text2);
				}
				text2 = stream.ReadLine();
			}
			this.text[num] = list.ToArray();
		}
	}
}
