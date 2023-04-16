using System.Collections.Generic;
using Mirror;
using UnityEngine;

internal class PesterchumHandler : MonoBehaviour
{
	public Pesterlog pesterlogPrefab;

	public GameObject pesterchumWindow;

	private readonly Dictionary<string, IPesterlog> logs = new Dictionary<string, IPesterlog>();

	private void Start()
	{
		if (NetworkClient.active)
		{
			NetworkClient.RegisterHandler<PesterchumStatusChange>(OnChatStatusChange);
			NetworkClient.RegisterHandler<PesterchumMessage>(OnMessageReceived);
		}
		if (NetworkServer.active)
		{
			NetworkServer.RegisterHandler<PesterchumStatusChange>(ForwardMessage);
			NetworkServer.RegisterHandler<PesterchumMessage>(ForwardMessage);
		}
		logs.Add(string.Empty, GlobalChat.Pesterlog);
	}

	private static void ForwardMessage<T>(NetworkConnection conn, T msg) where T : struct, NetworkMessage
	{
		NetworkServer.SendToAll(msg);
	}

	private static bool TryGetOtherName(string sender, string receiver, out string chum)
	{
		if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(receiver))
		{
			chum = string.Empty;
		}
		else if (sender == MultiplayerSettings.playerName)
		{
			chum = receiver;
		}
		else
		{
			if (!(receiver == MultiplayerSettings.playerName))
			{
				chum = null;
				return false;
			}
			chum = sender;
		}
		return true;
	}

	private void OnChatStatusChange(PesterchumStatusChange message)
	{
		if (TryGetOtherName(message.sender, message.receiver, out var chum))
		{
			if (!logs.TryGetValue(chum, out var value))
			{
				value = Object.Instantiate(pesterlogPrefab, base.transform);
				logs.Add(chum, value);
			}
			value.OnStatusChanged(chum, message);
		}
	}

	private void OnMessageReceived(PesterchumMessage message)
	{
		if (TryGetOtherName(message.sender, message.receiver, out var chum) && logs.TryGetValue(chum, out var value))
		{
			value.OnMessageReceived(message);
		}
	}

	public static void Pester(string target)
	{
		Debug.Log("Pestering " + target);
		PesterchumStatusChange message = default(PesterchumStatusChange);
		message.sender = MultiplayerSettings.playerName;
		message.receiver = target;
		message.status = true;
		NetworkClient.Send(message);
	}
}
