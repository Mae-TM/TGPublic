using System;
using System.Linq;
using System.Text;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class InLobbyComponent : MonoBehaviour
{
	[SerializeField]
	private LobbySceneComponent _lobbySceneComponent;

	[SerializeField]
	private LobbyPlayerList _playerList;

	[SerializeField]
	private Text _lobbyName;

	[SerializeField]
	private Button _btnStart;

	[SerializeField]
	private LobbyNameChangeDialog _lobbyNameChangeDialog;

	[SerializeField]
	private LobbySettingsComponent _lobbySettingsComponent;

	[SerializeField]
	private Material _shiftedMaterial;

	public void OnEnable()
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyDataUpdated += LobbyManagerOnLobbyDataUpdated;
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyGameCreated += LobbyManagerOnLobbyGameCreated;
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyFriendJoined += LobbyManagerOnLobbyFriendJoined;
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyFriendLeft += LobbyManagerOnLobbyFriendLeft;
	}

	public void OnDisable()
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyDataUpdated -= LobbyManagerOnLobbyDataUpdated;
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyGameCreated -= LobbyManagerOnLobbyGameCreated;
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyFriendJoined -= LobbyManagerOnLobbyFriendJoined;
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyFriendLeft -= LobbyManagerOnLobbyFriendLeft;
	}

	public void InvitePlayers()
	{
		SteamFriends.OpenGameInviteOverlay(AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.Id);
	}

	public void SetInteractableStart(bool to)
	{
		_btnStart.interactable = to;
	}

	private void LobbyManagerOnLobbyFriendLeft(Friend friend)
	{
		_playerList.RemovePlayer(friend.Id);
		_lobbySettingsComponent.OverrideGateOrder();
	}

	private void LobbyManagerOnLobbyFriendJoined(Friend friend)
	{
		_playerList.AddPlayer(friend.Id, friend.Name, isLocalPlayer: false);
		_lobbySettingsComponent.OverrideGateOrder();
	}

	private void LobbyManagerOnLobbyDataUpdated(TGPLobby lobby)
	{
		bool flag = lobby.SteamLobby.IsOwnedBy(SteamClient.SteamId);
		bool flag2 = _lobbySettingsComponent.LoadedSessionHasAllPlayers();
		_btnStart.gameObject.SetActive(flag && flag2);
		_lobbyNameChangeDialog.SetStatus(flag);
		_lobbyName.text = lobby.Name;
		_playerList.UpdateView();
		_lobbySettingsComponent.UpdateChangeability();
	}

	private void LobbyManagerOnLobbyGameCreated(SteamId steamId)
	{
		SteamId steamId2 = steamId;
		Debug.Log("Connect to is " + steamId2.ToString());
		StartSession();
	}

	public void SetLobbyData(TGPLobby lobby)
	{
		_lobbyName.text = lobby.Name;
		foreach (Friend item in from info in lobby.PlayerInformation.Values
			where (ulong)info.Friend.Id != (ulong)SteamClient.SteamId
			select info.Friend)
		{
			_playerList.AddPlayer(item.Id, item.Name, isLocalPlayer: false);
		}
	}

	public void StartHostingSession()
	{
		StartSession();
	}

	private string GenerateRandomString(int length)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < length; i++)
		{
			stringBuilder.Append("abcdefghijklmnopqrstuvwxyz0123456789+=_-"[UnityEngine.Random.Range(0, "abcdefghijklmnopqrstuvwxyz0123456789+=_-".Length)]);
		}
		return stringBuilder.ToString();
	}

	private void StartSession()
	{
		TGPLobby currentLobby = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby;
		currentLobby.SetSessionName(_lobbySettingsComponent.SanitizeFileName(_lobbySettingsComponent._sessionName.text));
		if (string.IsNullOrEmpty(_lobbySettingsComponent._randomSeed.text))
		{
			currentLobby.SetRandomSeed(GenerateRandomString(16).GetHashCode());
		}
		else
		{
			currentLobby.SetRandomSeed(_lobbySettingsComponent._randomSeed.text.GetHashCode());
		}
		NetcodeManager.LocalPlayerId = currentLobby.GateOrder.IndexOf(SteamClient.SteamId);
		WorldManager.colors = currentLobby.OrderedPlayerInformation.Select((Func<TGPLobby.PerPlayerInformation, Color>)((TGPLobby.PerPlayerInformation player) => player.Sprite.character.color)).ToArray();
		MultiplayerSettings.playerName = PlayerPrefs.GetString("PesterchumName", SteamClient.Name);
		_shiftedMaterial.SetFloat("_HueShift", PlayerPrefs.GetFloat("CharacterColorH", 0f) * 360f);
		_shiftedMaterial.SetFloat("_Sat", PlayerPrefs.GetFloat("CharacterColorS", 1f));
		_shiftedMaterial.SetFloat("_Val", PlayerPrefs.GetFloat("CharacterColorV", 1f));
		LoadingScreen.LoadScene("HouseBuildingCombo");
	}

	public void LeaveLobby()
	{
		AbstractSingletonManager<LobbyManager>.Instance.LeaveLobby();
		_playerList.RemoveAllPlayers();
		_lobbyName.text = "";
		_lobbySceneComponent.OpenBrowserScreen();
	}

	public void CopyLobbyID()
	{
		GUIUtility.systemCopyBuffer = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.Id.ToString();
	}
}
