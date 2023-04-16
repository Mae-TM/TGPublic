using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kcp2k;
using Mirror;
using QFSW.QC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

public class NetcodeManager : NetworkManager
{
	public SessionSaveManager sessionSaveManager;

	private readonly RandomWrapper random = new RandomWrapper();

	public static NetcodeManager Instance;

	private readonly Dictionary<NetworkConnection, int> playerConnections = new Dictionary<NetworkConnection, int>();

	private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();

	public bool offline;

	public GameObject connectionLostDialog;

	public GameObject quitDialog;

	public GameObject quitClientDialog;

	public Button quitDialogConfirm;

	public Button quitClientDialogConfirm;

	public ErrorMessage errorMessage;

	[SerializeField]
	private Text pingText;

	private Action _onPlanetLoaded;

	public static int LocalPlayerId { get; set; }

	public int PlayerCount => players.Count;

	public static RandomWrapper rng => Instance.random;

	public bool ConnectionLost { get; private set; }

	private static event Action OnServerStart;

	public override void Awake()
	{
		Player.spawnLocation = LocalPlayerId;
		if (AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby == null || SburbConsoleCommands.transportType == SburbConsoleCommands.TransportType.Kcp)
		{
			UnityEngine.Object.Destroy(transport);
			transport = base.gameObject.AddComponent<KcpTransport>();
		}
		base.Awake();
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("More than one netcode manager in the scene!");
		}
	}

	public Player GetPlayerByEntryIndex(int id)
	{
		if (players.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public int GetPlayerEntryIndexByConnection(NetworkConnection conn)
	{
		if (conn == null)
		{
			return -1;
		}
		if (playerConnections.TryGetValue(conn, out var value))
		{
			return value;
		}
		return -1;
	}

	public List<int> GetPlayerIDs()
	{
		return players.Keys.ToList();
	}

	public override void OnStartServer()
	{
		MultiplayerSettings.hosting = true;
		NetworkServer.RegisterHandler<PlayerJoinMessage>(OnPlayerJoin);
		NetcodeManager.OnServerStart?.Invoke();
		if (!TutorialDirector.isTutorial)
		{
			sessionSaveManager.RegisterServerHandlers();
		}
		StartCoroutine(SendRandomSync());
	}

	public Transport GetCurrentTransport()
	{
		return transport;
	}

	public override void OnStartClient()
	{
		if (!NetworkServer.active)
		{
			NetworkClient.RegisterHandler<RandomSync>(IncomingRandomSync);
			MultiplayerSettings.hosting = false;
		}
		if (!TutorialDirector.isTutorial)
		{
			sessionSaveManager.RegisterClientHandlers();
		}
	}

	public override void Start()
	{
		if (AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby != null && SburbConsoleCommands.transportType != SburbConsoleCommands.TransportType.Kcp)
		{
			Debug.Log("Starting Steam transport");
			SessionRandom.seed = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.RandomSeed;
			AbstractAttachedSingletonManager<RichPresenceManager>.Instance.Exploring(NetworkServer.active);
			if (AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.GetGameServer(out var steamId))
			{
				StartClient(new Uri($"steam://{steamId}"));
			}
			else
			{
				if (sessionSaveManager.DoSessionFilesExist(AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SessionName))
				{
					Debug.Log("Loading from file: " + AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SessionName);
					sessionSaveManager.LoadSession(AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SessionName);
				}
				else
				{
					sessionSaveManager.CreateNewSession(AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SessionName);
				}
				StartHost();
				AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SendJoin();
			}
		}
		else if (SburbConsoleCommands.transportType == SburbConsoleCommands.TransportType.Kcp)
		{
			Debug.Log("Starting KCP transport");
			if (!TutorialDirector.isTutorial)
			{
				MultiplayerSettings.playerName = (SburbConsoleCommands.hostGame ? "Host" : "Client");
			}
			else
			{
				MultiplayerSettings.playerName = "ectoBiologist";
			}
			WorldManager.colors = new Color[2]
			{
				Color.black,
				Color.black
			};
			LocalPlayerId = ((!SburbConsoleCommands.hostGame) ? 1 : 0);
			if (SburbConsoleCommands.hostGame)
			{
				QuantumConsole.Instance.LogToConsole("[net] Hosting");
				networkAddress = SburbConsoleCommands.serverAddress;
				((KcpTransport)transport).Port = SburbConsoleCommands.serverPort;
				SburbConsoleCommands.hostGame = false;
				StartHost();
			}
			else if (SburbConsoleCommands.connectAddress != "" && SburbConsoleCommands.connectPort > 0)
			{
				QuantumConsole.Instance.LogToConsole("[net] Connecting to " + SburbConsoleCommands.connectAddress + ":" + SburbConsoleCommands.connectPort);
				networkAddress = SburbConsoleCommands.connectAddress;
				((KcpTransport)transport).Port = SburbConsoleCommands.connectPort;
				SburbConsoleCommands.connectAddress = "";
				SburbConsoleCommands.connectPort = 0;
				StartClient();
			}
			else
			{
				StartClient();
			}
		}
		else
		{
			bool flag = !Environment.CurrentDirectory.EndsWith("clone");
			MultiplayerSettings.playerName = (flag ? "Host" : "Client");
			WorldManager.colors = new Color[2]
			{
				Color.black,
				Color.black
			};
			LocalPlayerId = ((!flag) ? 1 : 0);
			if (flag)
			{
				StartHost();
			}
			else
			{
				StartClient();
			}
		}
		base.Start();
		if (NetworkServer.active)
		{
			pingText.transform.parent.gameObject.SetActive(value: false);
		}
	}

	public void CloseConfirmDialog()
	{
		quitDialog.SetActive(value: false);
		quitDialogConfirm.onClick.RemoveAllListeners();
		quitClientDialog.SetActive(value: false);
		quitClientDialogConfirm.onClick.RemoveAllListeners();
		AbstractAttachedSingletonManager<SteamManager>.Instance.JoinedLobbyID = null;
	}

	public void DisableObject(GameObject toDisable)
	{
		toDisable.SetActive(value: false);
	}

	public void ButtonQuit(bool toMenu)
	{
		if (NetworkServer.active && !errorMessage.gameObject.activeSelf)
		{
			quitDialog.gameObject.SetActive(value: true);
			quitDialogConfirm.onClick.AddListener(delegate
			{
				DoQuit(toMenu);
			});
		}
		else
		{
			quitClientDialog.gameObject.SetActive(value: true);
			quitClientDialogConfirm.onClick.AddListener(delegate
			{
				DoQuit(toMenu);
			});
		}
	}

	public void SaveSession()
	{
		if (NetworkServer.active && !TutorialDirector.isTutorial)
		{
			sessionSaveManager.SaveSession();
		}
	}

	public override void OnApplicationQuit()
	{
		if (NetworkClient.active && !NetworkServer.active)
		{
			sessionSaveManager.SaveLocalPlayer();
		}
		ShutDownNetcode();
		base.OnApplicationQuit();
	}

	private void DoQuit(bool toMenu)
	{
		if (NetworkClient.active && !NetworkServer.active)
		{
			sessionSaveManager.SaveLocalPlayer();
		}
		quitDialogConfirm.onClick.RemoveAllListeners();
		ShutDownNetcode();
		if (toMenu)
		{
			if (AbstractAttachedSingletonManager<SteamManager>.Instance.JoinedLobbyID.HasValue)
			{
				SceneManager.LoadScene("Lobby");
			}
			else
			{
				SceneManager.LoadScene("MainMenuNew");
			}
		}
		else
		{
			Application.Quit();
		}
	}

	public void ShutDownNetcode()
	{
		Debug.Log("Shutting down netcode, NetworkServer.active: " + NetworkServer.active + ", NetworkClient.active: " + NetworkClient.active);
		StopAllCoroutines();
		NetworkServer.Shutdown();
		NetworkClient.Disconnect();
		NetworkClient.Shutdown();
		ConnectionLost = false;
	}

	private void Update()
	{
		if (connectionLostDialog != null)
		{
			connectionLostDialog.SetActive(ConnectionLost);
		}
		if (pingText.isActiveAndEnabled)
		{
			pingText.text = $"{Math.Round(NetworkTime.rtt * 1000.0)}ms";
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		using (AbstractSingletonManager<ReallyBasicProfiler>.Instance.Track(MethodBase.GetCurrentMethod()))
		{
			ConnectionLost = false;
			base.OnClientConnect(conn);
			Debug.Log("Connection complete");
			PlayerJoinMessage message = default(PlayerJoinMessage);
			message.id = LocalPlayerId;
			message.house = HouseManager.LoadHouse(HouseBuilder.saveAs);
			message.data = Player.GetSaveData();
			NetworkClient.Send(message);
			PesterchumStatusChange message2 = default(PesterchumStatusChange);
			message2.sender = MultiplayerSettings.playerName;
			message2.status = false;
			NetworkClient.Send(message2);
		}
	}

	private IEnumerator SendRandomSync()
	{
		WaitForSecondsRealtime delay = new WaitForSecondsRealtime(60f);
		while (NetworkServer.active)
		{
			yield return delay;
			if (!NetworkServer.NoExternalConnections())
			{
				NetworkServer.SendToAll(GetRandomSync());
			}
		}
	}

	public void RegisterPlayer(Player player)
	{
		PlayerSync sync = player.sync;
		int id = sync.np.id;
		if (players.ContainsKey(id))
		{
			return;
		}
		sync.np.playerObject = player.gameObject;
		players.Add(id, player);
		Debug.Log($"New player joined: {id} ({sync.np.name})");
		if (AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby != null && players.Count >= AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.MemberCount)
		{
			Debug.Log($"All {players.Count} made it in, leaving lobby.");
			AbstractSingletonManager<LobbyManager>.Instance.LeaveLobby();
		}
		if (ChumList.Instance != null)
		{
			ChumList.Instance.ScheduleChumUpdate();
		}
		House house = AbstractSingletonManager<WorldManager>.Instance.FindHouseByOwner(id);
		if (!(house != null))
		{
			return;
		}
		house.Owner = player;
		if (id != LocalPlayerId)
		{
			return;
		}
		if (!house.planet)
		{
			Debug.Log("Enabling movement, planet not existing");
			player.GetComponent<PlayerMovement>().enabled = true;
			return;
		}
		Debug.Log("Adding callback for movement, planet exists");
		DisplacementMapFlat displacementMapFlat = house.planet.GetComponent<DisplacementMapFlat>();
		_onPlanetLoaded = delegate
		{
			Debug.Log("Enabling movement, planet loaded");
			displacementMapFlat.PlanetLoaded -= _onPlanetLoaded;
			player.GetComponent<PlayerMovement>().enabled = true;
			_onPlanetLoaded = null;
		};
		displacementMapFlat.PlanetLoaded += _onPlanetLoaded;
	}

	public void DeregisterPlayer(Player player)
	{
		Debug.Log($"Player {player.GetID()} ({player.name}) left.");
		players.Remove(player.GetID());
		if (ChumList.Instance != null)
		{
			ChumList.Instance.ScheduleChumUpdate();
		}
	}

	private void OnPlayerJoin(NetworkConnection conn, PlayerJoinMessage incoming)
	{
		using (AbstractSingletonManager<ReallyBasicProfiler>.Instance.Track(MethodBase.GetCurrentMethod()))
		{
			Debug.Log("Player joining");
			playerConnections[conn] = incoming.id;
			Player component = UnityEngine.Object.Instantiate(playerPrefab).GetComponent<Player>();
			component.sync.np.id = incoming.id;
			if (sessionSaveManager.DoesPlayerHaveSave(incoming.id))
			{
				sessionSaveManager.LoadPlayer(incoming.id, conn, component);
			}
			else
			{
				Debug.Log($"Player {incoming.id} doesn't have a save, creating new one");
				component.Load(incoming.data);
				component.MoveToSpawn(HouseManager.Instance.SpawnHouse(incoming.id, incoming.house));
				NetworkServer.AddPlayerForConnection(conn, component.gameObject);
			}
			conn.Send(GetRandomSync());
		}
	}

	private RandomSync GetRandomSync()
	{
		RandomSync result = default(RandomSync);
		result.random = random.Save();
		return result;
	}

	private void IncomingRandomSync(RandomSync incoming)
	{
		random.Load(incoming.random);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		Debug.LogError("Lost connection!");
		ConnectionLost = true;
	}

	public static void RegisterStaticHandler<T>(Action<NetworkConnection, T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
	{
		if (NetworkServer.active)
		{
			NetworkServer.RegisterHandler(handler, requireAuthentication);
		}
		OnServerStart += delegate
		{
			NetworkServer.RegisterHandler(handler, requireAuthentication);
		};
	}

	public IEnumerable<Player> GetPlayers()
	{
		return players.Values;
	}

	public bool TryGetPlayer(int id, out Player player)
	{
		return players.TryGetValue(id, out player);
	}

	public override void OnServerDisconnect(NetworkConnection connection)
	{
		base.OnServerDisconnect(connection);
		if (playerConnections.TryGetValue(connection, out var value))
		{
			Debug.Log($"Player {value}'s ({players[value].name}) connection closed.");
			playerConnections.Remove(connection);
			sessionSaveManager.SavePlayer(value, connection);
		}
	}
}
