using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Dialogue : MonoBehaviour
{
	private static Dialogue instance;

	private Text dialogueText;

	private readonly List<Button> options = new List<Button>();

	private Image box;

	private string[] dialogue;

	private int index;

	private int indent;

	private ISet<string> localVars;

	private readonly HashSet<string> vars = new HashSet<string>();

	private BlurOptimized blur;

	private static string lastFile;

	private static FileInfo[] lastResults;

	public static event Action<ISet<string>> OnDone;

	public void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
		dialogueText = base.transform.GetChild(1).GetChild(0).GetComponent<Text>();
		options.Add(base.transform.GetChild(1).GetChild(1).GetComponent<Button>());
		box = base.transform.GetChild(1).GetComponent<Image>();
		box.gameObject.SetActive(value: true);
		blur = MSPAOrthoController.main.GetComponent<BlurOptimized>();
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && !options[0].gameObject.activeSelf)
		{
			index++;
			UpdateDialogue();
		}
	}

	private void UpdateDialogue()
	{
		foreach (Button option in options)
		{
			option.gameObject.SetActive(value: false);
		}
		index--;
		int num;
		string text = ReadNext(out num);
		if (text == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		if (num > indent)
		{
			if (!text.StartsWith(">"))
			{
				return;
			}
			dialogueText.gameObject.SetActive(value: false);
			indent = num;
			int num2 = 0;
			while (num >= indent && index < dialogue.Length)
			{
				int nextIndex = index + 1;
				AddDialogueOption(num2, text.Substring(1), isGrey: false, delegate
				{
					index = nextIndex;
					UpdateDialogue();
				});
				while (num >= indent)
				{
					text = ReadNext(out num);
					if (text == null || (num == indent && text.StartsWith(">")))
					{
						break;
					}
				}
				num2++;
			}
			return;
		}
		indent = num;
		if (text.StartsWith(">"))
		{
			int num3 = text.IndexOf(':');
			if (num3 == -1)
			{
				while (num >= indent)
				{
					if (ReadNext(out num) == null)
					{
						base.gameObject.SetActive(value: false);
						return;
					}
				}
				indent = num;
				UpdateDialogue();
				return;
			}
			int num4 = 0;
			while (text != null && text.StartsWith(">"))
			{
				num3 = text.IndexOf(':');
				if (num3 == -1)
				{
					break;
				}
				string label = text.Substring(1, num3 - 1);
				AddDialogueOption(num4, text.Substring(num3 + 1), Check(label), delegate
				{
					Seek(label);
				});
				text = ReadNext(out num);
				num4++;
			}
			dialogueText.gameObject.SetActive(value: false);
		}
		else if (!text.StartsWith(":") || !ReverseSeek(text.Substring(1)))
		{
			ApplyText(text);
		}
	}

	private bool Check(string condition)
	{
		if (!vars.Contains(condition))
		{
			return localVars.Contains(condition);
		}
		return true;
	}

	private string ReadNext(out int indent)
	{
		string text;
		string text2;
		do
		{
			if (++index >= dialogue.Length)
			{
				indent = int.MinValue;
				return null;
			}
			text = dialogue[index];
			text2 = text.TrimStart('\t');
			if (!text2.StartsWith("if ("))
			{
				continue;
			}
			string[] array = text2.Substring("if (".Length).Split(new char[1] { ')' }, 2);
			bool flag = true;
			if (array[0].StartsWith("!"))
			{
				flag = false;
				array[0] = array[0].Substring("!".Length);
			}
			if (Check(array[0]) != flag)
			{
				uint num = (array[1].Contains("{") ? 1u : 0u);
				do
				{
					if (++index >= dialogue.Length)
					{
						Debug.LogError("If condition for '" + array[0] + "' without final '}'");
						base.gameObject.SetActive(value: false);
						indent = int.MinValue;
						return null;
					}
					text2 = dialogue[index].TrimStart('\t');
					if (text2.Contains("{"))
					{
						num++;
					}
					if (text2.Contains("}"))
					{
						num--;
					}
				}
				while (num != 0);
			}
			text2 = "";
		}
		while (text2 == "" || text2 == "}");
		indent = text.Length - text2.Length;
		return text2;
	}

	private void Seek(string label)
	{
		localVars.Add(label);
		while (index < dialogue.Length)
		{
			if (dialogue[index] == label + ":")
			{
				index++;
				UpdateDialogue();
				return;
			}
			index++;
		}
		base.gameObject.SetActive(value: false);
	}

	private bool ReverseSeek(string label)
	{
		if (label == "end")
		{
			base.gameObject.SetActive(value: false);
			return true;
		}
		int num = index;
		while (--num >= 0)
		{
			if (dialogue[num].StartsWith(">" + label + ":"))
			{
				while (dialogue[--num].StartsWith(">"))
				{
				}
				index = num + 1;
				UpdateDialogue();
				return true;
			}
		}
		return false;
	}

	private void AddDialogueOption(int i, string text, bool isGrey, UnityAction call)
	{
		if (i >= options.Count)
		{
			options.Add(UnityEngine.Object.Instantiate(options[0], options[0].transform.parent));
		}
		ApplyText(options[i].GetComponent<Text>(), text);
		options[i].onClick.RemoveAllListeners();
		options[i].onClick.AddListener(call);
		ColorBlock colors = options[i].colors;
		colors.colorMultiplier = (isGrey ? 0.5f : 1f);
		options[i].colors = colors;
	}

	private void ApplyText(string text)
	{
		ApplyText(dialogueText, text);
	}

	private void ApplyText(Text to, string text)
	{
		to.gameObject.SetActive(value: true);
		to.text = text.Replace("[name]", Player.player.sylladex.PlayerName).Replace("[class]", Player.player.classpect.role.ToString()).Replace("[aspect]", Player.player.classpect.aspect.ToString());
	}

	private void OnEnable()
	{
		AutoClose.activeCount++;
	}

	private void OnDisable()
	{
		foreach (Transform item in instance.transform.GetChild(0))
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		AutoClose.activeCount--;
		blur.enabled = false;
		Dialogue.OnDone?.Invoke(localVars);
		Dialogue.OnDone = null;
		localVars = null;
	}

	public static bool SetVariable(string name, bool to = true)
	{
		if (to)
		{
			return instance.vars.Add(name);
		}
		return instance.vars.Remove(name);
	}

	public static string GetDialoguePath(string file)
	{
		if (file != lastFile)
		{
			if (StreamingAssets.TryGetFile("Dialogue/" + file + ".txt", out var path))
			{
				return path;
			}
			lastFile = file;
			lastResults = StreamingAssets.GetDirectoryContents("Dialogue/" + file, "*.txt").ToArray();
		}
		if (lastResults == null || lastResults.Length == 0)
		{
			return null;
		}
		return lastResults[UnityEngine.Random.Range(0, lastResults.Length)].FullName;
	}

	private static string[] LoadDialogue(string file)
	{
		if (!File.Exists(file))
		{
			return null;
		}
		Queue<string> queue;
		using (StreamReader streamReader = File.OpenText(file))
		{
			queue = new Queue<string>();
			while (!streamReader.EndOfStream)
			{
				queue.Enqueue(streamReader.ReadLine().Replace("    ", "\t"));
			}
		}
		return queue.ToArray();
	}

	public static void StartDialogue(string dialogue, Material material, Transform sprite, ISet<string> vars = null)
	{
		string[] array = LoadDialogue(dialogue);
		if (array != null)
		{
			StartDialogue(array, material, sprite, vars);
		}
	}

	private static void StartDialogue(string[] dialogue, Material material, Transform sprite, ISet<string> vars)
	{
		instance.index = 0;
		instance.indent = 0;
		instance.dialogue = dialogue;
		instance.box.material = material;
		instance.gameObject.SetActive(value: true);
		instance.blur.enabled = true;
		instance.localVars = vars ?? new HashSet<string>();
		instance.UpdateDialogue();
		ImageEffects.Imagify(sprite, instance.transform.GetChild(0));
	}
}
