using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class LobbyManager : AbstractSingletonManager<LobbyManager>
{
	public delegate void LobbyListReceived(IEnumerable<TGPLobby> lobbies);

	public delegate void LobbyDataUpdated(TGPLobby lobby);

	public delegate void LobbyJoined(TGPLobby lobby);

	public delegate void LobbyFriendJoined(Friend friend);

	public delegate void LobbyFriendLeft(Friend friend);

	public delegate void LobbyMessageReceived(Friend friend, string message);

	public delegate void LobbyGameCreated(SteamId steamId);

	public TGPLobby CurrentLobby { get; private set; }

	public event LobbyListReceived OnLobbyListReceived;

	public event LobbyDataUpdated OnLobbyDataUpdated;

	public event LobbyJoined OnLobbyJoined;

	public event LobbyFriendJoined OnLobbyFriendJoined;

	public event LobbyFriendLeft OnLobbyFriendLeft;

	public event LobbyMessageReceived OnLobbyMessageReceived;

	public event LobbyGameCreated OnLobbyGameCreated;

	public LobbyManager()
	{
		if (!AbstractAttachedSingletonManager<SteamManager>.Instance.Initialized)
		{
			Debug.LogWarning("SteamManager was not initialized! Doing that for you...");
		}
		SteamMatchmaking.OnLobbyCreated += SteamMatchmakingOnOnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered += SteamMatchmakingOnOnLobbyEntered;
		SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmakingOnOnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmakingOnOnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyMemberDisconnected += SteamMatchmakingOnOnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyDataChanged += SteamMatchmakingOnOnLobbyDataChanged;
		SteamMatchmaking.OnLobbyMemberDataChanged += SteamMatchmakingOnOnLobbyMemberDataChanged;
		SteamMatchmaking.OnChatMessage += SteamMatchmakingOnOnChatMessage;
		SteamMatchmaking.OnLobbyGameCreated += SteamMatchmakingOnOnLobbyGameCreated;
		CurrentLobby = null;
	}

	private void SteamMatchmakingOnOnLobbyMemberDataChanged(Lobby lobby, Friend friend)
	{
		if (CurrentLobby != null)
		{
			CurrentLobby.DataUpdated(lobby);
			this.OnLobbyDataUpdated?.Invoke(CurrentLobby);
		}
		else
		{
			lobby.Leave();
		}
	}

	private void SteamMatchmakingOnOnLobbyDataChanged(Lobby lobby)
	{
		if (CurrentLobby != null)
		{
			CurrentLobby.DataUpdated(lobby);
			this.OnLobbyDataUpdated?.Invoke(CurrentLobby);
		}
		else
		{
			lobby.Leave();
		}
	}

	private void SteamMatchmakingOnOnChatMessage(Lobby lobby, Friend friend, string message)
	{
		this.OnLobbyMessageReceived?.Invoke(friend, message);
	}

	private void SteamMatchmakingOnOnLobbyMemberLeave(Lobby lobby, Friend friend)
	{
		Debug.Log($"User {friend.Name} left the lobby {lobby.Id}");
		if (CurrentLobby != null)
		{
			CurrentLobby.RemovePlayer(friend.Id);
			this.OnLobbyFriendLeft?.Invoke(friend);
		}
		else
		{
			lobby.Leave();
		}
	}

	private void SteamMatchmakingOnOnLobbyMemberJoined(Lobby lobby, Friend friend)
	{
		Debug.Log($"User {friend.Name} joined lobby {lobby.Id}");
		if (CurrentLobby != null)
		{
			CurrentLobby.AddPlayer(friend);
			this.OnLobbyFriendJoined?.Invoke(friend);
		}
		else
		{
			lobby.Leave();
		}
	}

	private void SteamMatchmakingOnOnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
	{
		Debug.Log($"Lobby game was created, host is: {steamId}");
		this.OnLobbyGameCreated?.Invoke(steamId);
	}

	private void SteamMatchmakingOnOnLobbyEntered(Lobby lobby)
	{
		Debug.Log($"Lobby entered: {lobby.Id}");
		CurrentLobby = TGPLobby.FromLobbyFull(lobby);
		CurrentLobby.SetVisibility(TGPLobby.LobbyVisibility.Public);
		CurrentLobby.SetGameBuild();
		this.OnLobbyJoined?.Invoke(CurrentLobby);
	}

	private void SteamMatchmakingOnOnLobbyCreated(Result result, Lobby lobby)
	{
		if (result == Result.OK)
		{
			Debug.Log($"Lobby created: {lobby.Id}");
		}
	}

	public void CreateLobby()
	{
		SteamMatchmaking.CreateLobbyAsync(12);
	}

	public void JoinLobby(ulong lobbyId)
	{
		SteamId steamId = default(SteamId);
		steamId.Value = lobbyId;
		SteamId lobbyId2 = steamId;
		JoinLobby(lobbyId2);
	}

	public void JoinLobby(SteamId lobbyId)
	{
		SteamMatchmaking.JoinLobbyAsync(lobbyId);
	}

	public void LeaveLobby()
	{
		if (CurrentLobby != null)
		{
			CurrentLobby.SteamLobby.Leave();
			CurrentLobby = null;
		}
	}

	public void SendChatMessage(string message)
	{
		if (CurrentLobby != null)
		{
			CurrentLobby.SteamLobby.SendChatString(message);
		}
	}

	public async void RequestLobbyList()
	{
		Lobby[] array = (await SteamMatchmaking.LobbyList.FilterDistanceWorldwide().WithEqual("gameBuild", SteamApps.BuildId).RequestAsync()) ?? Array.Empty<Lobby>();
		List<TGPLobby> tgpLobbies = new List<TGPLobby>();
		Lobby[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Lobby? lobby = await array2[i].RefreshAsync();
			if (lobby.HasValue)
			{
				TGPLobby item = TGPLobby.FromLobby(lobby.Value);
				tgpLobbies.Add(item);
			}
		}
		this.OnLobbyListReceived?.Invoke(tgpLobbies);
	}

	public async void SearchLobbies(string name)
	{
		Lobby[] array = (await SteamMatchmaking.LobbyList.FilterDistanceWorldwide().WithKeyValue("name", name).RequestAsync()) ?? Array.Empty<Lobby>();
		List<TGPLobby> tgpLobbies = new List<TGPLobby>();
		Lobby[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Lobby? lobby = await array2[i].RefreshAsync();
			if (lobby.HasValue)
			{
				TGPLobby item = TGPLobby.FromLobby(lobby.Value);
				tgpLobbies.Add(item);
			}
		}
		this.OnLobbyListReceived?.Invoke(tgpLobbies);
	}
}
