using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBrowserComponent : MonoBehaviour
{
	public LobbyListComponent lobbyList;

	public LobbySceneComponent lobbySceneComponent;

	public LobbyCreateDialog lobbyCreateDialog;

	public LobbyNameWindow nameWindow;

	private bool isShowingSessions;

	[SerializeField]
	private Text _username;

	[SerializeField]
	private TMP_InputField _searchBox;

	[SerializeField]
	private TMP_InputField _lobbyIDBox;

	[SerializeField]
	private Button _joinByIdButton;

	private void Start()
	{
		_username.text = "Hello, " + SteamClient.Name;
	}

	public void DisplayError(string error)
	{
		lobbySceneComponent.lobbyErrorComponent.Error(error);
	}

	private void LobbyManagerOnOnLobbyListReceived(IEnumerable<TGPLobby> lobbies)
	{
		foreach (TGPLobby lobby in lobbies)
		{
			Debug.Log("Adding lobby to lobby list: " + lobby.Name);
			lobbyList.AddLobby(lobby);
		}
	}

	public void OnEnable()
	{
		AbstractAttachedSingletonManager<RichPresenceManager>.Instance.InMenus();
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyListReceived += LobbyManagerOnOnLobbyListReceived;
		Search();
	}

	public void OnDisable()
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyListReceived -= LobbyManagerOnOnLobbyListReceived;
	}

	public void ShowCreateScreen()
	{
		nameWindow.gameObject.SetActive(value: true);
	}

	public void ShowSessions(bool show)
	{
		isShowingSessions = show;
	}

	public void Search()
	{
		lobbyList.ClearLobbies();
		if (string.IsNullOrEmpty(_searchBox.text))
		{
			AbstractSingletonManager<LobbyManager>.Instance.RequestLobbyList();
		}
		else
		{
			AbstractSingletonManager<LobbyManager>.Instance.SearchLobbies(_searchBox.text);
		}
	}

	public void EnterTutorial()
	{
		TutorialDirector.StartTutorial();
	}

	public void EnterHouseCreator()
	{
		MultiplayerSettings.hosting = true;
		SessionRandom.seed = 0;
		Player.loadedPlayerData = null;
		NetcodeManager.LocalPlayerId = 0;
		WorldManager.colors = new UnityEngine.Color[1] { UnityEngine.Color.black };
		HouseBuilder.saveAs = null;
		BuildExploreSwitcher.cheatMode = true;
		lobbySceneComponent.menu.EnterScene("HouseBuildingCombo");
	}

	public void ShowModFolder()
	{
		Application.OpenURL("file://" + Application.streamingAssetsPath);
	}

	public async void JoinByLobbyID()
	{
		Debug.Log("Attempting to join lobby " + _lobbyIDBox.text);
		_joinByIdButton.enabled = false;
		if (ulong.TryParse(_lobbyIDBox.text, out var lobbyId) && (await new Lobby(lobbyId).RefreshAsync()).HasValue)
		{
			Debug.Log("Lobby is considered valid. Joining the lobby...");
			_joinByIdButton.enabled = true;
			lobbySceneComponent.JoinLobby(lobbyId);
		}
		else
		{
			DisplayError("Invalid Lobby ID. Please try again.");
			_joinByIdButton.enabled = true;
		}
	}
}
