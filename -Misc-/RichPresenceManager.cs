using System;
using System.Linq;
using Discord;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RichPresenceManager : AbstractAttachedSingletonManager<RichPresenceManager>
{
	private enum RichPresenceState
	{
		None,
		InMenu,
		InSession,
		InOwnLobby,
		InLobby
	}

	[SerializeField]
	private long discordAppId;

	public string[] whitelistedJoinScenes;

	private global::Discord.Discord _discordInstance;

	private ActivityManager _discordActivityManager;

	private Activity _discordActivity;

	private RichPresenceState _richPresenceState;

	private void OnEnable()
	{
		InitializeDiscord();
	}

	private void OnDisable()
	{
		try
		{
			SteamFriends.ClearRichPresence();
		}
		catch (Exception)
		{
		}
	}

	private void InitializeDiscord()
	{
		try
		{
			_discordInstance = new global::Discord.Discord(discordAppId, 1uL);
			_discordActivityManager = _discordInstance.GetActivityManager();
			_discordActivityManager.RegisterSteam(AbstractAttachedSingletonManager<SteamManager>.Instance.SteamAppId);
			_discordInstance.SetLogHook(LogLevel.Warn, OnDiscordError);
		}
		catch (Exception)
		{
		}
	}

	private void DiscordActivityManagerOnOnActivityJoin(string secret)
	{
		if (!whitelistedJoinScenes.Contains(SceneManager.GetActiveScene().name))
		{
			return;
		}
		try
		{
			SceneManager.LoadScene("Scenes/Lobby");
			LobbySceneComponent lobbySceneComponent = UnityEngine.Object.FindObjectOfType<LobbySceneComponent>();
			string partyId = secret.Split(';')[0];
			lobbySceneComponent.JoinLobbyViaSecret(partyId);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private void OnDiscordError(LogLevel logLevel, string message)
	{
		switch (logLevel)
		{
		case LogLevel.Warn:
			Debug.LogWarning(message);
			break;
		case LogLevel.Error:
			Debug.LogError(message);
			break;
		case LogLevel.Info:
		case LogLevel.Debug:
			Debug.Log(message);
			break;
		}
	}

	private void OnDiscordDisconnected(int errorCode, string message)
	{
		Debug.LogError("Disconnected from Discord with the error code " + errorCode + " and the message " + message + ".");
	}

	private void DiscordUpdate()
	{
		if (_discordInstance == null)
		{
			return;
		}
		try
		{
			_discordInstance.RunCallbacks();
		}
		catch (ResultException ex)
		{
			if (ex.Result == Discord.Result.NotRunning)
			{
				OnDiscordDisconnected(ex.HResult, ex.Message);
			}
			throw;
		}
	}

	private void SteamUpdate()
	{
	}

	public void InMenus()
	{
		try
		{
			RichPresenceState oldState = _richPresenceState;
			_discordActivity = new Activity
			{
				State = "In Menus"
			};
			_richPresenceState = RichPresenceState.InMenu;
			_discordActivityManager.UpdateActivity(_discordActivity, delegate(Discord.Result res)
			{
				Debug.Log($"Rich presence state switched from {oldState} to {_richPresenceState}: {res}");
			});
		}
		catch (Exception)
		{
		}
		try
		{
			SteamFriends.SetRichPresence("steam_display", "#Status_InMenus");
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public void InOwnLobby()
	{
		try
		{
			RichPresenceState oldState = _richPresenceState;
			string join = $"{AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.Id.Value};";
			if (AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.HasPassword)
			{
				join = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.JoinSecret;
			}
			_discordActivity = new Activity
			{
				Assets = new ActivityAssets
				{
					LargeImage = "explore",
					LargeText = "Creating a lobby"
				},
				State = "In Lobby",
				Details = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.Name,
				Party = new ActivityParty
				{
					Size = new PartySize
					{
						CurrentSize = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.PlayerInformation.Count,
						MaxSize = 12
					},
					Id = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.Id.Value.ToString()
				},
				Timestamps = new ActivityTimestamps
				{
					Start = DateTimeOffset.Now.ToUnixTimeSeconds()
				},
				Instance = false,
				Secrets = new ActivitySecrets
				{
					Join = join
				}
			};
			_discordActivityManager.UpdateActivity(_discordActivity, delegate(Discord.Result res)
			{
				Debug.Log($"Rich presence state switched from {oldState} to {_richPresenceState}: {res}");
			});
			_richPresenceState = RichPresenceState.InOwnLobby;
		}
		catch (Exception)
		{
		}
		try
		{
			SteamFriends.SetRichPresence("steam_display", "#Status_InOwnLobby");
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public void InLobby()
	{
		try
		{
			RichPresenceState oldState = _richPresenceState;
			_discordActivity = new Activity
			{
				Assets = new ActivityAssets
				{
					LargeImage = "explore",
					LargeText = "Waiting in a lobby"
				},
				State = "In Lobby",
				Details = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.Name,
				Party = new ActivityParty
				{
					Size = new PartySize
					{
						CurrentSize = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.PlayerInformation.Count,
						MaxSize = 12
					},
					Id = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.Id.Value.ToString()
				},
				Timestamps = new ActivityTimestamps
				{
					Start = DateTimeOffset.Now.ToUnixTimeSeconds()
				},
				Instance = false
			};
			_discordActivityManager.UpdateActivity(_discordActivity, delegate(Discord.Result res)
			{
				Debug.Log($"Rich presence state switched from {oldState} to {_richPresenceState}: {res}");
			});
			_richPresenceState = RichPresenceState.InOwnLobby;
		}
		catch (Exception)
		{
		}
		try
		{
			SteamFriends.SetRichPresence("steam_display", "#Status_InLobby");
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public void Exploring(bool isHosting = false)
	{
		try
		{
			if (_richPresenceState == RichPresenceState.InSession)
			{
				_discordActivity.Details = "Exploring houses";
				_discordActivity.Assets = new ActivityAssets
				{
					LargeImage = "explore",
					LargeText = "Exploring houses"
				};
			}
			else
			{
				_discordActivity = new Activity
				{
					Assets = new ActivityAssets
					{
						LargeImage = "explore",
						LargeText = "Exploring houses"
					},
					State = "In Session",
					Details = "Exploring houses",
					Timestamps = new ActivityTimestamps
					{
						Start = DateTimeOffset.Now.ToUnixTimeSeconds()
					},
					Instance = false
				};
			}
			_discordActivityManager.UpdateActivity(_discordActivity, delegate
			{
			});
			_richPresenceState = RichPresenceState.InSession;
		}
		catch (Exception)
		{
		}
		try
		{
			SteamFriends.SetRichPresence("steam_display", "#Status_Exploring");
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public void Building(bool isHosting = false)
	{
		try
		{
			if (_richPresenceState == RichPresenceState.InSession)
			{
				_discordActivity.Details = "Building houses";
				_discordActivity.Assets = new ActivityAssets
				{
					LargeImage = "build",
					LargeText = "Building houses"
				};
			}
			else
			{
				_discordActivity = new Activity
				{
					Assets = new ActivityAssets
					{
						LargeImage = "build",
						LargeText = "Building houses"
					},
					State = "In Session",
					Details = "Building houses",
					Timestamps = new ActivityTimestamps
					{
						Start = DateTimeOffset.Now.ToUnixTimeSeconds()
					},
					Instance = false
				};
			}
			_discordActivityManager.UpdateActivity(_discordActivity, delegate
			{
			});
			_richPresenceState = RichPresenceState.InSession;
		}
		catch (Exception)
		{
		}
		try
		{
			SteamFriends.SetRichPresence("steam_display", "#Status_Building");
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		DiscordUpdate();
		SteamUpdate();
	}
}
