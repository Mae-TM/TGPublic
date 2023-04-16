using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamManager : AbstractAttachedSingletonManager<SteamManager>
{
	[SerializeField]
	private uint steamAppId;

	private Steamworks.Ugc.Item[] subscribedItems = Array.Empty<Steamworks.Ugc.Item>();

	public uint SteamAppId => steamAppId;

	public bool Initialized { get; private set; }

	public ulong? StartedLobbyID { get; set; }

	public ulong? JoinedLobbyID { get; set; }

	public IEnumerable<Steamworks.Ugc.Item> SubscribedItems => subscribedItems;

	private new void Awake()
	{
		base.Awake();
		try
		{
			SteamClient.Init(steamAppId);
			Initialized = true;
			SteamFriends.OnGameLobbyJoinRequested += SteamFriendsOnOnGameLobbyJoinRequested;
			SteamApps.OnNewLaunchParameters += SteamAppsOnOnNewLaunchParameters;
			Dispatch.OnException = Debug.LogException;
			DownloadWorkshopItems();
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			Application.Quit();
		}
	}

	private void SteamFriendsOnOnGameLobbyJoinRequested(Lobby lobby, SteamId friend)
	{
		if (NetcodeManager.Instance != null)
		{
			NetcodeManager.Instance.ButtonQuit(toMenu: true);
			return;
		}
		JoinedLobbyID = lobby.Id.Value;
		SceneManager.LoadScene("Lobby");
	}

	private void SteamAppsOnOnNewLaunchParameters()
	{
		string commandLine = SteamApps.CommandLine;
		if (commandLine.Contains("+connect_lobby"))
		{
			string value = Regex.Match(commandLine, "+connect_lobby (\\d+)").Groups[0].Value;
			StartedLobbyID = ulong.Parse(value);
			SceneManager.LoadScene("Lobby");
		}
	}

	private void Start()
	{
		string commandLine = SteamApps.CommandLine;
		Debug.Log("Steam started with: " + commandLine);
		if (commandLine.Contains("+connect_lobby"))
		{
			string value = Regex.Match(commandLine, "+connect_lobby (\\d+)").Groups[0].Value;
			StartedLobbyID = ulong.Parse(value);
			SceneManager.LoadScene("Lobby");
		}
	}

	private void Update()
	{
		SteamClient.RunCallbacks();
	}

	private void OnDisable()
	{
		SteamClient.Shutdown();
	}

	private async void DownloadWorkshopItems()
	{
		int page = 1;
		while (true)
		{
			ResultPage? resultPage = await Query.Items.WhereUserSubscribed().GetPageAsync(page);
			if (resultPage.HasValue && resultPage.Value.ResultCount != 0)
			{
				subscribedItems = subscribedItems.Concat(resultPage.Value.Entries).ToArray();
				Steamworks.Ugc.Item[] array = subscribedItems;
				foreach (Steamworks.Ugc.Item item in array)
				{
					item.Download();
				}
				int i = page + 1;
				page = i;
				continue;
			}
			break;
		}
	}
}
