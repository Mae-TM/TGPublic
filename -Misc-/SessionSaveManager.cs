using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using ProtoBuf;
using Steamworks;
using UnityEngine;

public class SessionSaveManager : MonoBehaviour
{
	private List<int> _tempUnsavedPlayers;

	[SerializeField]
	private Animator _saveIcon;

	private DateTime _startedSaving;

	private ulong[] _gateOrder;

	public string SessionPath { get; set; }

	public event Action<bool> OnSave;

	private void Awake()
	{
		AbstractSingletonManager<WorldManager>.Instance.LoadArea = LoadArea;
	}

	private void Update()
	{
		if (!(_startedSaving == default(DateTime)) && (DateTime.Now - _startedSaving).TotalSeconds > 10.0)
		{
			_startedSaving = default(DateTime);
			this.OnSave?.Invoke(obj: false);
		}
	}

	private string SanitizeFileName(string fileName, char replacementChar = '_')
	{
		HashSet<char> hashSet = new HashSet<char>(Path.GetInvalidFileNameChars());
		char[] array = fileName.ToCharArray();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (hashSet.Contains(array[i]))
			{
				array[i] = replacementChar;
			}
		}
		return new string(array);
	}

	public void RegisterClientHandlers()
	{
		Debug.Log("I am the client, so I listen to my server");
		NetworkClient.RegisterHandler<HostSaveRequest>(OnHostSaveRequest);
		NetworkClient.RegisterHandler<HostLoadRequest>(OnHostLoadRequest);
	}

	public void RegisterServerHandlers()
	{
		Debug.Log("I am the server, so I will save the players");
		StartCoroutine(SavePeriodically(300f));
		NetworkServer.RegisterHandler<ClientSaveResponse>(OnClientSaveResponse);
	}

	private void OnHostLoadRequest(HostLoadRequest request)
	{
		StartCoroutine(WaitAndLoadPlayerData(request));
	}

	private IEnumerator WaitAndLoadPlayerData(HostLoadRequest request)
	{
		while (Player.player == null || Player.player.sylladex == null)
		{
			yield return null;
		}
		Debug.Log("Loading player data on client now...");
		Player.player.sylladex.Load(request.sylladex);
		Exile.Load(request.exile);
	}

	public bool DoesPlayerHaveSave(int id)
	{
		if (string.IsNullOrEmpty(SessionPath))
		{
			return false;
		}
		if (!Directory.Exists(SessionPath))
		{
			return false;
		}
		return File.Exists(Path.Combine(Path.Combine(SessionPath, "Players"), $"{id}.pls"));
	}

	public WorldArea LoadArea(int id)
	{
		if (id < 0)
		{
			return LoadDungeon(id);
		}
		return LoadHouse(id);
	}

	public House LoadHouse(int id)
	{
		if (string.IsNullOrEmpty(SessionPath))
		{
			return null;
		}
		if (!Directory.Exists(SessionPath))
		{
			return null;
		}
		using FileStream source = File.OpenRead(Path.Combine(SessionPath, "Houses", $"{id}.hss"));
		SessionHouse sessionHouse = Serializer.Deserialize<SessionHouse>(source);
		House house = HouseManager.Instance.SpawnHouse(id, sessionHouse.house);
		house.LoadProgress(sessionHouse.progress);
		return house;
	}

	private House LoadHouseWithOwner(int id, Player owner)
	{
		if (string.IsNullOrEmpty(SessionPath))
		{
			return null;
		}
		if (!Directory.Exists(SessionPath))
		{
			return null;
		}
		using FileStream source = File.OpenRead(Path.Combine(SessionPath, "Houses", $"{id}.hss"));
		SessionHouse sessionHouse = Serializer.Deserialize<SessionHouse>(source);
		House house = HouseManager.Instance.SpawnHouse(id, sessionHouse.house);
		house.LoadProgress(sessionHouse.progress, owner);
		return house;
	}

	private Dungeon LoadDungeon(int id)
	{
		if (string.IsNullOrEmpty(SessionPath))
		{
			return null;
		}
		if (!Directory.Exists(SessionPath))
		{
			return null;
		}
		string path = Path.Combine(SessionPath, "Dungeons", $"{id}.dss");
		if (!File.Exists(path))
		{
			return null;
		}
		using FileStream source = File.OpenRead(path);
		SessionDungeon sessionDungeon = Serializer.Deserialize<SessionDungeon>(source);
		Dungeon dungeon = DungeonManager.Instance.Load(sessionDungeon.dungeon, id);
		dungeon.LoadProgress(sessionDungeon.progress);
		return dungeon;
	}

	public void LoadPlayer(int entryId, NetworkConnection conn, Player p)
	{
		if (!string.IsNullOrEmpty(SessionPath) && Directory.Exists(SessionPath))
		{
			string path = Path.Combine(SessionPath, "Players");
			SessionPlayer sessionPlayer = default(SessionPlayer);
			using (FileStream source = File.OpenRead(Path.Combine(path, $"{entryId}.pls")))
			{
				sessionPlayer = Serializer.Deserialize<SessionPlayer>(source);
			}
			PlayerData playerData = default(PlayerData);
			playerData.name = sessionPlayer.name;
			playerData.armor = sessionPlayer.armor.Select(Item.Load).ToArray();
			playerData.aspect = sessionPlayer.aspect;
			playerData.character = sessionPlayer.character;
			playerData.grist = sessionPlayer.grist;
			playerData.experience = sessionPlayer.experience;
			playerData.level = sessionPlayer.level;
			playerData.role = sessionPlayer.role;
			playerData.kernelSprite = sessionPlayer.kernelSprite;
			PlayerData data = playerData;
			p.Load(data);
			LoadHouseWithOwner(entryId, p);
			p.RegionChild.Area = AbstractSingletonManager<WorldManager>.Instance.GetArea(sessionPlayer.currentArea);
			p.transform.localPosition = sessionPlayer.position;
			NetworkServer.AddPlayerForConnection(conn, p.gameObject);
			conn.Send(new HostLoadRequest
			{
				exile = sessionPlayer.exile,
				sylladex = sessionPlayer.sylladex
			});
		}
	}

	public void OnDisable()
	{
		StopAllCoroutines();
		if (NetworkServer.active)
		{
			NetworkClient.UnregisterHandler<ClientSaveResponse>();
			return;
		}
		NetworkServer.UnregisterHandler<HostSaveRequest>();
		NetworkClient.UnregisterHandler<HostLoadRequest>();
	}

	private void OnClientSaveResponse(NetworkConnection conn, ClientSaveResponse response)
	{
		if (string.IsNullOrEmpty(SessionPath))
		{
			return;
		}
		string path = Path.Combine(SessionPath, "Players");
		int playerEntryIndexByConnection = NetcodeManager.Instance.GetPlayerEntryIndexByConnection(conn);
		Debug.Log($"Received save data from player {playerEntryIndexByConnection}");
		using (FileStream fileStream = File.Create(Path.Combine(path, $"{playerEntryIndexByConnection}.pls")))
		{
			Serializer.Serialize(fileStream, response.player);
			fileStream.Flush();
		}
		if (_tempUnsavedPlayers != null && _tempUnsavedPlayers.Count != 0)
		{
			if (_tempUnsavedPlayers.Contains(playerEntryIndexByConnection))
			{
				_tempUnsavedPlayers.Remove(playerEntryIndexByConnection);
			}
			List<int> tempUnsavedPlayers = _tempUnsavedPlayers;
			if (tempUnsavedPlayers != null && tempUnsavedPlayers.Count == 0)
			{
				_saveIcon.SetTrigger("Close");
				this.OnSave?.Invoke(obj: true);
			}
		}
	}

	private void OnHostSaveRequest(HostSaveRequest obj)
	{
		SaveLocalPlayer();
	}

	public void SaveLocalPlayer()
	{
		Debug.Log("As host requested, saving the player...");
		PlayerData playerData = Player.player.Save();
		SessionPlayer sessionPlayer = default(SessionPlayer);
		sessionPlayer.name = playerData.name;
		sessionPlayer.armor = playerData.armor.Select((Item i) => i?.Save()).ToArray();
		sessionPlayer.aspect = playerData.aspect;
		sessionPlayer.character = playerData.character;
		sessionPlayer.exile = Exile.Save();
		sessionPlayer.grist = playerData.grist;
		sessionPlayer.currentArea = Player.player.RegionChild.Area.Id;
		sessionPlayer.experience = playerData.experience;
		sessionPlayer.level = playerData.level;
		sessionPlayer.position = Player.player.GetPosition(local: true);
		sessionPlayer.role = playerData.role;
		sessionPlayer.sylladex = Player.player.sylladex.Save();
		sessionPlayer.kernelSprite = playerData.kernelSprite;
		SessionPlayer player = sessionPlayer;
		ClientSaveResponse message = default(ClientSaveResponse);
		message.player = player;
		NetworkClient.Send(message);
	}

	public IEnumerator SavePeriodically(float interval)
	{
		while (NetworkServer.active)
		{
			yield return new WaitForSeconds(interval);
			SaveSession();
		}
	}

	public void SavePlayer(int id, NetworkConnection conn)
	{
		if (string.IsNullOrEmpty(SessionPath))
		{
			return;
		}
		EnsureSessionDirectories();
		string path = Path.Combine(SessionPath, "Houses");
		try
		{
			conn.Send(default(HostSaveRequest));
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not send save request to client: " + ex.Message);
		}
		House house = AbstractSingletonManager<WorldManager>.Instance.FindHouseByOwner(id);
		SessionHouse sessionHouse = default(SessionHouse);
		sessionHouse.house = house.Save();
		sessionHouse.progress = house.SaveProgress();
		SessionHouse instance = sessionHouse;
		using FileStream fileStream = File.Create(Path.Combine(path, $"{house.Id}.hss"));
		Serializer.Serialize(fileStream, instance);
		fileStream.Flush();
	}

	public void CreateNewSession(string sessionName)
	{
		Debug.Log("Creating new session: " + sessionName);
		SessionPath = Path.Combine(Application.streamingAssetsPath, "SaveData", sessionName);
		_gateOrder = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.GateOrder.Select((SteamId i) => i.Value).ToArray();
	}

	private void EnsureSessionDirectories()
	{
		if (!Directory.Exists(SessionPath))
		{
			Directory.CreateDirectory(SessionPath);
		}
		string path = Path.Combine(SessionPath, "Players");
		string path2 = Path.Combine(SessionPath, "Houses");
		string path3 = Path.Combine(SessionPath, "Dungeons");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		if (!Directory.Exists(path2))
		{
			Directory.CreateDirectory(path2);
		}
		if (!Directory.Exists(path3))
		{
			Directory.CreateDirectory(path3);
		}
	}

	public bool SaveSession()
	{
		if (string.IsNullOrEmpty(SessionPath))
		{
			return false;
		}
		EnsureSessionDirectories();
		string path = Path.Combine(SessionPath, "Houses");
		string path2 = Path.Combine(SessionPath, "Dungeons");
		_startedSaving = DateTime.Now;
		_saveIcon.gameObject.SetActive(value: true);
		_saveIcon.Play("SaveIconOpen");
		SessionData sessionData = default(SessionData);
		sessionData.randomSeed = SessionRandom.seed;
		sessionData.gateOrder = _gateOrder;
		SessionData instance = sessionData;
		_tempUnsavedPlayers = NetcodeManager.Instance.GetPlayerIDs();
		NetworkServer.SendToAll(default(HostSaveRequest));
		foreach (House house in AbstractSingletonManager<WorldManager>.Instance.Houses)
		{
			SessionHouse sessionHouse = default(SessionHouse);
			sessionHouse.house = house.Save();
			sessionHouse.progress = house.SaveProgress();
			SessionHouse instance2 = sessionHouse;
			using FileStream fileStream = File.Create(Path.Combine(path, $"{house.Id}.hss"));
			Serializer.Serialize(fileStream, instance2);
			fileStream.Flush();
		}
		foreach (Dungeon dungeon in AbstractSingletonManager<WorldManager>.Instance.Dungeons)
		{
			SessionDungeon sessionDungeon = default(SessionDungeon);
			sessionDungeon.dungeon = dungeon.Save();
			sessionDungeon.progress = dungeon.SaveProgress();
			SessionDungeon instance3 = sessionDungeon;
			using FileStream destination = File.Create(Path.Combine(path2, $"{dungeon.Id}.dss"));
			Serializer.Serialize(destination, instance3);
		}
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Serializer.Serialize(memoryStream, instance);
			memoryStream.Flush();
		}
		using (FileStream fileStream2 = File.Create(Path.Combine(SessionPath, "save.ses")))
		{
			Serializer.Serialize(fileStream2, instance);
			fileStream2.Flush();
		}
		return true;
	}

	public bool DoSessionFilesExist(string sessionName)
	{
		if (string.IsNullOrEmpty(sessionName.Trim()))
		{
			return false;
		}
		string text = Path.Combine(Application.streamingAssetsPath, "SaveData", sessionName);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (!Directory.Exists(text))
		{
			return false;
		}
		return File.Exists(Path.Combine(text, "save.ses"));
	}

	public void LoadSession(string sessionName)
	{
		string text = Path.Combine(Application.streamingAssetsPath, "SaveData", sessionName);
		if (!string.IsNullOrEmpty(text) && Directory.Exists(text))
		{
			SessionPath = text;
			Debug.Log("Loading session from " + SessionPath);
			SessionData sessionData = default(SessionData);
			using (FileStream source = File.OpenRead(Path.Combine(SessionPath, "save.ses")))
			{
				sessionData = Serializer.Deserialize<SessionData>(source);
			}
			SessionRandom.seed = sessionData.randomSeed;
			_gateOrder = sessionData.gateOrder;
		}
	}
}
