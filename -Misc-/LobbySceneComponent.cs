using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

public class LobbySceneComponent : MonoBehaviour
{
	[FormerlySerializedAs("LobbyBrowser")]
	public LobbyBrowserComponent lobbyBrowser;

	public StartMenu menu;

	public InLobbyComponent inLobby;

	public LobbyErrorComponent lobbyErrorComponent;

	public void Start()
	{
		if (!AbstractAttachedSingletonManager<SteamManager>.Instance.Initialized)
		{
			Debug.LogWarning("SteamManager was not initialized, doing that for you");
		}
		HandleSteamJoins();
	}

	public void HandleSteamJoins()
	{
		if (AbstractAttachedSingletonManager<SteamManager>.Instance.StartedLobbyID.HasValue)
		{
			ulong value = AbstractAttachedSingletonManager<SteamManager>.Instance.StartedLobbyID.Value;
			AbstractAttachedSingletonManager<SteamManager>.Instance.StartedLobbyID = null;
			Debug.Log("Joining lobby " + value);
			JoinLobby(new SteamId
			{
				Value = value
			});
		}
		else if (AbstractAttachedSingletonManager<SteamManager>.Instance.JoinedLobbyID.HasValue)
		{
			ulong value2 = AbstractAttachedSingletonManager<SteamManager>.Instance.JoinedLobbyID.Value;
			AbstractAttachedSingletonManager<SteamManager>.Instance.JoinedLobbyID = null;
			Debug.Log("Joining lobby " + value2);
			JoinLobby(new SteamId
			{
				Value = value2
			});
		}
	}

	public void JoinLobby(SteamId lobbyId)
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyJoined += TempLobbyJoined;
		AbstractSingletonManager<LobbyManager>.Instance.JoinLobby(lobbyId);
		void TempLobbyJoined(TGPLobby lobby)
		{
			OpenLobbyScreen();
			inLobby.SetLobbyData(lobby);
			AbstractAttachedSingletonManager<RichPresenceManager>.Instance.InLobby();
			AbstractSingletonManager<LobbyManager>.Instance.OnLobbyJoined -= TempLobbyJoined;
		}
	}

	public void JoinLobbyViaSecret(string partyId)
	{
		JoinLobby(new SteamId
		{
			Value = ulong.Parse(partyId)
		});
	}

	public void OpenLobbyScreen()
	{
		inLobby.gameObject.SetActive(value: true);
		lobbyBrowser.gameObject.SetActive(value: false);
	}

	public void OpenBrowserScreen()
	{
		inLobby.gameObject.SetActive(value: false);
		lobbyBrowser.gameObject.SetActive(value: true);
	}
}
