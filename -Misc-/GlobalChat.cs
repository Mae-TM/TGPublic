using System;
using Assets.Multiplayer.Scripts.commands;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Util;

public class GlobalChat : MonoBehaviour, IPesterlog
{
	public static GlobalChat instance;

	[SerializeField]
	private Text logText;

	public InputField messageField;

	[SerializeField]
	private Image playerBall;

	[SerializeField]
	private Image locationBall;

	[SerializeField]
	private Animator showButton;

	[SerializeField]
	private GameObject chatWindow;

	[SerializeField]
	private AudioSource bleep;

	private bool recentlyClosed;

	private bool ready;

	private string commandToConfirm;

	public static IPesterlog Pesterlog => instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	public static void SetupChat()
	{
		instance.SetupChatInternal();
	}

	private void SetupChatInternal()
	{
		KeyboardControl.UIControls.OpenChat.performed += PesterButton;
		CommandBase.RegisterAllCommands();
		Color color = Player.player.sync.np.character.color;
		messageField.textComponent.color = ColorSelector.GetTypingColor(color);
		Transform parent = locationBall.transform.parent;
		locationBall.transform.SetParent(null, worldPositionStays: false);
		locationBall.color = color;
		int num = 0;
		if (Player.player.RegionChild.Area is House house)
		{
			num = house.Id;
		}
		if (WorldManager.colors != null)
		{
			for (int i = 0; i < WorldManager.colors.Length; i++)
			{
				if (i == NetcodeManager.LocalPlayerId)
				{
					WorldManager.colors[i] = color;
				}
				Button button = AddPlayer(i, WorldManager.colors[i]);
				button.interactable = i == NetcodeManager.LocalPlayerId;
				if (i == num)
				{
					parent = button.transform;
				}
			}
		}
		else
		{
			parent = AddPlayer(NetcodeManager.LocalPlayerId, color).transform;
		}
		locationBall.transform.SetParent(parent, worldPositionStays: false);
		showButton.gameObject.SetActive(value: true);
		ready = true;
	}

	private void OnDestroy()
	{
		if (NetworkClient.active)
		{
			PesterchumStatusChange message = default(PesterchumStatusChange);
			message.sender = MultiplayerSettings.playerName;
			message.status = true;
			NetworkClient.Send(message);
		}
	}

	public void OpenChat()
	{
		if (ready)
		{
			chatWindow.SetActive(value: true);
			showButton.gameObject.SetActive(value: false);
		}
	}

	public void CloseChat()
	{
		showButton.gameObject.SetActive(value: true);
		chatWindow.SetActive(value: false);
	}

	public void SubmitMessage()
	{
		if (Input.GetButtonDown("Submit"))
		{
			if (messageField.text == "")
			{
				CloseChat();
				recentlyClosed = true;
				return;
			}
			Pester(messageField.text);
			DoCommand(messageField.text);
			messageField.text = "";
			messageField.ActivateInputField();
		}
	}

	private void DoCommand(string text)
	{
		if (text[0] == '/')
		{
			string text2 = text.Substring(1).Split(' ')[0];
			if (CommandBase.commands.ContainsKey(text2))
			{
				if (text2 != "help")
				{
					if (CommandBase.commands[text2].safe)
					{
						Pester(MultiplayerSettings.playerName + " was a naughty cheater and ran the command " + text2);
						CommandBase.commands[text2].RunCommand(text.Substring(1));
					}
					else
					{
						WriteCommandMessage(text2 + " is intended for testing and is very likely to break your session. Are you sure you want to do this? (y/n)");
						commandToConfirm = text.Substring(1);
					}
				}
				else
				{
					CommandBase.commands[text2].RunCommand(text.Substring(1));
				}
			}
			else
			{
				WriteCommandMessage("<color=red>No such command: " + text2 + " </color>\n");
			}
		}
		else if (commandToConfirm != null)
		{
			if (text[0] == 'y' || text[0] == 'Y')
			{
				string text3 = commandToConfirm.Split(' ')[0];
				Pester(MultiplayerSettings.playerName + " decided to run the seriously dangerous command " + text3);
				CommandBase.commands[text3].RunCommand(commandToConfirm);
			}
			else
			{
				WriteCommandMessage("Cancelled command.");
			}
			commandToConfirm = null;
		}
	}

	public static void Pester(string message, bool server = false)
	{
		PesterchumMessage message2 = default(PesterchumMessage);
		message2.sender = (server ? "SERVER" : MultiplayerSettings.playerName);
		message2.message = PrettyFilter.CensorString(message);
		message2.color = ColorUtility.ToHtmlStringRGB(ColorSelector.GetTypingColor(Player.player.sync.np.character.color));
		NetworkClient.Send(message2);
	}

	public static void WriteCommandMessage(string message)
	{
		instance.AddText(message);
	}

	private void AddText(string text)
	{
		if (string.IsNullOrEmpty(logText.text))
		{
			logText.text = text;
		}
		else
		{
			UnityEngine.Object.Instantiate(logText, logText.transform.parent).text = text;
		}
		if (!chatWindow.activeSelf)
		{
			showButton.Play("NotEnoughGrist");
			bleep.Play();
		}
	}

	private void PesterButton(InputAction.CallbackContext context)
	{
		if (!QuantumConsoleHooks.IsOpen)
		{
			if (Input.GetButtonUp("Submit") && recentlyClosed)
			{
				recentlyClosed = false;
				return;
			}
			OpenChat();
			EventSystem.current.SetSelectedGameObject(messageField.gameObject);
			messageField.ActivateInputField();
		}
	}

	public void OnMessageReceived(PesterchumMessage message)
	{
		AddText("<color=#" + message.color + ">" + GetShortName(message.sender) + ": " + message.message + "</color>");
	}

	public void OnStatusChanged(string chum, PesterchumStatusChange message)
	{
		if (!message.status)
		{
			AddText("-- " + message.sender + "[" + GetShortName(message.sender) + "] joined at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute.ToString("D2") + "--");
		}
	}

	private static string GetShortName(string longName)
	{
		return global::Pesterlog.GetShortName(longName);
	}

	private void AddPlayer(NetworkPlayer np)
	{
		Button button = AddPlayer(np.id, np.character.color);
		button.onClick.AddListener(delegate
		{
			PesterchumHandler.Pester(np.name);
		});
		button.interactable = true;
	}

	private Button AddPlayer(int id, Color color)
	{
		Transform transform = playerBall.transform.parent.Find("PlayerBall" + id);
		if (transform == null)
		{
			if (locationBall.transform.parent == playerBall.transform)
			{
				transform = playerBall.transform;
				locationBall.transform.SetParent(null, worldPositionStays: false);
			}
			Image image = UnityEngine.Object.Instantiate(playerBall, playerBall.transform.parent);
			image.name = "PlayerBall" + id;
			image.color = color;
			image.transform.localPosition = new Vector3(id * 16, 0f, 0f);
			Button component = image.GetComponent<Button>();
			component.onClick.RemoveAllListeners();
			playerBall = image;
			if (transform != null)
			{
				locationBall.transform.SetParent(transform, worldPositionStays: false);
			}
			return component;
		}
		transform.GetComponent<Image>().color = color;
		return transform.GetComponent<Button>();
	}

	private void RemovePlayer(NetworkPlayer np)
	{
		if (!(playerBall == null))
		{
			Transform transform = playerBall.transform.parent.Find("PlayerBall" + np.id);
			if (!(transform == null))
			{
				AddText("-- " + np.name + "[" + GetShortName(np.name) + "] left at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute.ToString("D2") + "--");
				transform.GetComponent<Button>().interactable = false;
			}
		}
	}

	public static void MoveLocationBall(int id)
	{
		Transform transform = instance.playerBall.transform.parent.Find("PlayerBall" + id);
		if (transform != null)
		{
			instance.locationBall.transform.SetParent(transform, worldPositionStays: false);
		}
	}

	public static void ScheduleAddPlayer(NetworkPlayer np)
	{
		instance.AddPlayer(np);
	}

	public static void ScheduleRemovePlayer(NetworkPlayer np)
	{
		instance.RemovePlayer(np);
	}

	public static void ToggleActive()
	{
		instance.gameObject.SetActive(!instance.gameObject.activeSelf);
	}

	public static void SetActive(bool value = true)
	{
		instance.gameObject.SetActive(value);
	}
}
