using System;
using System.Linq;
using System.Text.RegularExpressions;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class Pesterlog : MonoBehaviour, IPesterlog
{
	public Text logText;

	public InputField messageField;

	public string receiverName;

	public Text chumName;

	private void Awake()
	{
		logText.text = string.Empty;
	}

	public void SubmitMessage()
	{
		if (Input.GetButtonDown("Submit"))
		{
			Pester();
		}
	}

	public void Pester()
	{
		PesterchumMessage message = default(PesterchumMessage);
		message.sender = MultiplayerSettings.playerName;
		message.receiver = receiverName;
		message.message = PrettyFilter.CensorString(messageField.text);
		message.color = ColorUtility.ToHtmlStringRGB(ColorSelector.GetTypingColor(Player.player.sync.np.character.color));
		NetworkClient.Send(message);
		messageField.text = "";
	}

	private void AddText(string text)
	{
		logText.text += text;
	}

	public void OnStatusChanged(string chum, PesterchumStatusChange msg)
	{
		base.gameObject.SetActive(msg.status);
		receiverName = chum;
		chumName.text = chum;
		AddText("-- " + msg.sender + "[" + GetShortName(msg.sender) + "] " + (msg.status ? "began" : "ceased") + " pestering " + msg.receiver + " [" + GetShortName(msg.receiver) + "] at " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "--\n");
	}

	public void OnMessageReceived(PesterchumMessage message)
	{
		AddText("<color=#" + message.color + ">" + GetShortName(message.sender) + ": " + message.message + "</color>\n");
	}

	public static string GetShortName(string longName)
	{
		return new string((from s in Regex.Replace(longName, "([A-Z])", " $1", RegexOptions.None).Trim().Split(' ')
			where !string.IsNullOrEmpty(s)
			select char.ToUpper(s[0])).ToArray());
	}

	public void Close()
	{
		PesterchumStatusChange message = default(PesterchumStatusChange);
		message.sender = MultiplayerSettings.playerName;
		message.receiver = receiverName;
		message.status = false;
		NetworkClient.Send(message);
	}
}
