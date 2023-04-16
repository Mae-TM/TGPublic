using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class LobbyChatComponent : MonoBehaviour
{
	[SerializeField]
	private InputField _chatInput;

	[SerializeField]
	private ChatMessageListComponent _chatMessageList;

	private bool _wasFocused;

	private void OnEnable()
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyMessageReceived += LobbyManagerOnLobbyMessageReceived;
	}

	private void OnDisable()
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyMessageReceived -= LobbyManagerOnLobbyMessageReceived;
		_chatMessageList.ResetChat();
	}

	private void LobbyManagerOnLobbyMessageReceived(Friend friend, string message)
	{
		_chatMessageList.AddMessage(friend.Name, message);
	}

	private void Update()
	{
		if (_wasFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
		{
			AbstractSingletonManager<LobbyManager>.Instance.SendChatMessage(PrettyFilter.CensorString(_chatInput.text));
			_chatInput.text = "";
			_chatInput.Select();
			_chatInput.ActivateInputField();
		}
		_wasFocused = _chatInput.isFocused;
	}
}
