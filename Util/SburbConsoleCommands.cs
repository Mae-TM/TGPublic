using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using kcp2k;
using QFSW.QC;
using QFSW.QC.Suggestors.Tags;
using Steamworks;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Util;

public static class SburbConsoleCommands
{
	public enum TransportType
	{
		Steam,
		Kcp
	}

	private static readonly HttpClient _client = new HttpClient();

	public static TransportType transportType = TransportType.Steam;

	public static string serverAddress = "localhost";

	public static ushort serverPort = 1025;

	public static bool hostGame = false;

	public static string connectAddress = "";

	public static ushort connectPort = 0;

	[Command("t_timescale", "Gets/sets the scale at which time is passing by. Default is 1.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{
			Time.timeScale = value;
		}
	}

	[Command("r_fullscreen", "Gets/sets the fullscreen state of the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static bool Fullscreen
	{
		get
		{
			return Screen.fullScreen;
		}
		set
		{
			Screen.fullScreen = value;
		}
	}

	[Command("r_maxfps", "Gets/sets the maximum FPS imposed on the game. Set to -1 for unlimited.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static int MaxFPS
	{
		get
		{
			return Application.targetFrameRate;
		}
		set
		{
			Application.targetFrameRate = value;
		}
	}

	[Command("r_vsync", "Gets/sets vsync for the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static bool VSync
	{
		get
		{
			return QualitySettings.vSyncCount > 0;
		}
		set
		{
			QualitySettings.vSyncCount = (value ? 1 : 0);
		}
	}

	[Command("r_msaa", "Gets/sets the number of MSAA samples in use. Valid values are 0, 2, 4 and 8.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static int MSAA
	{
		get
		{
			return QualitySettings.antiAliasing;
		}
		set
		{
			QualitySettings.antiAliasing = value;
		}
	}

	[Command("r_resolution", "Changes the resolution of the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SetResolution(int width, int height)
	{
		Screen.SetResolution(width, height, Screen.fullScreen);
	}

	[Command("sm_loadscene", "Loads a scene by name.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void LoadScene([SceneName] string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	[Command("sm_getscenes", "Retrieves the name and index of every scene included in the build.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static Dictionary<int, string> GetAllScenes()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		int sceneCountInBuildSettings = SceneManager.sceneCountInBuildSettings;
		for (int i = 0; i < sceneCountInBuildSettings; i++)
		{
			int num = i;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(num));
			dictionary.Add(num, fileNameWithoutExtension);
		}
		return dictionary;
	}

	[Command("http_get", "Sends a GET request to the specified URL.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static async Task<string> Get(string url)
	{
		return await (await _client.GetAsync(url)).Content.ReadAsStringAsync();
	}

	[Command("http_post", "Sends a POST request to the specified URL. A body may be sent with the request, with a default mediaType of text/plain.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static async Task<string> Post(string url, string content = "", string mediaType = "text/plain")
	{
		HttpContent content2 = new StringContent(content, Encoding.Default, mediaType);
		return await (await _client.PostAsync(url, content2)).Content.ReadAsStringAsync();
	}

	[Command("item_list", "Lists all items in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string ListItems()
	{
		string text = "Default Items:\n";
		foreach (LDBItem defaultItem in AbstractSingletonManager<DatabaseManager>.Instance.DefaultItems)
		{
			text = text + defaultItem.Code + " - " + defaultItem.Name + "\n";
		}
		text += "\nCustom Items:\n";
		foreach (KeyValuePair<string, List<LDBItem>> overlayItem in AbstractSingletonManager<DatabaseManager>.Instance.OverlayItems)
		{
			text = text + overlayItem.Key + ":\n";
			foreach (LDBItem item in overlayItem.Value)
			{
				text = text + item.Code + " - " + item.Name + "\n";
			}
		}
		return text.TrimEnd('\n');
	}

	[Command("item_count", "Returns the amount of items in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static int CountItems()
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.AllItems.Count;
	}

	[Command("item_count", "Returns the amount of items in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static int CountItems(string overlay)
	{
		if (overlay == "default")
		{
			return AbstractSingletonManager<DatabaseManager>.Instance.DefaultItems.Count;
		}
		if (AbstractSingletonManager<DatabaseManager>.Instance.OverlayItems.ContainsKey(overlay))
		{
			return AbstractSingletonManager<DatabaseManager>.Instance.OverlayItems[overlay].Count;
		}
		return -1;
	}

	[Command("item_get", "Gets the item with the specified code.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static LDBItem GetItem([ItemCode] string code)
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.AllItems.FirstOrDefault((LDBItem x) => x.Code == code);
	}

	[Command("recipe_list", "Lists all recipes in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string ListRecipes()
	{
		string text = "Default Recipes:\n";
		foreach (LDBRecipe defaultRecipe in AbstractSingletonManager<DatabaseManager>.Instance.DefaultRecipes)
		{
			string text2 = ((defaultRecipe.Method == LDBRecipe.Methods.AND) ? "&&" : "||");
			text = text + defaultRecipe.ItemA + " " + text2 + " " + defaultRecipe.ItemB + " -> " + defaultRecipe.Result.Code + "\n";
		}
		text += "\nCustom Recipes:\n";
		foreach (KeyValuePair<string, List<LDBRecipe>> overlayRecipe in AbstractSingletonManager<DatabaseManager>.Instance.OverlayRecipes)
		{
			text = text + overlayRecipe.Key + ":\n";
			foreach (LDBRecipe item in overlayRecipe.Value)
			{
				string text3 = ((item.Method == LDBRecipe.Methods.AND) ? "&&" : "||");
				text = text + item.ItemA + " " + text3 + " " + item.ItemB + " -> " + item.Result.Code + "\n";
			}
		}
		return text.TrimEnd('\n');
	}

	[Command("recipe_count", "Returns the amount of recipes in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static int CountRecipes()
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.AllRecipes.Count;
	}

	private static LDBItem SimulateRecipe([ItemCode] string itemA, string method, [ItemCode] string itemB)
	{
		LDBItem lDBItem = AbstractSingletonManager<DatabaseManager>.Instance.AllItems.FirstOrDefault((LDBItem i) => i.Code == itemA);
		LDBItem lDBItem2 = AbstractSingletonManager<DatabaseManager>.Instance.AllItems.FirstOrDefault((LDBItem i) => i.Code == itemB);
		if (lDBItem == null || lDBItem2 == null)
		{
			return null;
		}
		if (method != "&&" && method != "||")
		{
			return null;
		}
		return AbstractSingletonManager<DatabaseManager>.Instance.GetRecipeResult(lDBItem, lDBItem2, method);
	}

	[Command("steam_id", "Returns the Steam ID of the user.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static ulong GetSteamId()
	{
		return SteamClient.SteamId.Value;
	}

	[Command("steam_name", "Returns the Steam name of the user.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string GetSteamName()
	{
		return SteamClient.Name;
	}

	[Command("steam_connect_lobby", "Connects to the specified lobby.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void ConnectToLobby(ulong lobbyId)
	{
		AbstractAttachedSingletonManager<SteamManager>.Instance.JoinedLobbyID = lobbyId;
		SceneManager.LoadScene("Lobby");
	}

	[Command("steam_lobby_id", "Prints the current lobby to the console.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string PrintLobbyID()
	{
		if (AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby == null)
		{
			return "Not connected to a lobby";
		}
		return AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.Id.ToString();
	}

	[Command("c_give", "Gives the specified item to the user. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void GiveItem([ItemCode] string code)
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.sylladex.AddItem(code);
		}
	}

	[Command("c_xp", "Gives the specified amount of XP to the user. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void GiveXP(int amount)
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.Experience += amount;
		}
	}

	[Command("c_boon", "Gives the specified boondollars to the user. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void GiveBoon(int amount)
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.boonBucks += amount;
		}
	}

	[Command("c_grist", "Gives the specified grist to the user. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void GiveGrist(int amount)
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.Grist[0] += amount;
		}
	}

	[Command("c_grist", "Gives the specified grist to the user. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void GiveGrist(int gristType, int amount)
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.Grist[gristType] += amount;
		}
	}

	[Command("c_fly", "Enables flying for the user. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void ToggleFlying()
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.GetComponent<PlayerMovement>().ToggleFly();
		}
	}

	[Command("c_build", "Enters the building mode for the user. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void ToggleBuilding()
	{
		if (!(NetcodeManager.Instance == null))
		{
			BuildExploreSwitcher.Instance.SwitchToBuild(null, Player.player.RegionChild.Area as House);
		}
	}

	[Command("c_kill", "Kills all entities of a specified faction in the game. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void KillFaction([FactionName] string faction)
	{
		if (NetcodeManager.Instance == null)
		{
			return;
		}
		IEnumerable<Attackable> enumerable = ((Faction)faction)?.GetMembers();
		if (enumerable == null)
		{
			return;
		}
		foreach (Attackable item in new List<Attackable>(enumerable))
		{
			item.Kill();
		}
	}

	[Command("c_suicide", "Kills the current player", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void KillPlayer()
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.Kill();
		}
	}

	[Command("c_spawn", "Spawn a creature given its name. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SpawnCreature([CreatureName] string creature)
	{
		if (!(NetcodeManager.Instance == null))
		{
			SpawnHelper.instance.Spawn(creature, Player.player.RegionChild.Area, Player.player.GetPosition());
		}
	}

	[Command("c_spawn", "Spawn a creature given its name. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SpawnCreature([CreatureName] string creature, uint count)
	{
		if (!(NetcodeManager.Instance == null))
		{
			for (uint num = 0u; num < count; num++)
			{
				SpawnHelper.instance.Spawn(creature, Player.player.RegionChild.Area, Player.player.GetPosition());
			}
		}
	}

	[Command("c_enter", "Enters the player into the session. Will screw up the session due to a lack of certain variables set. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void EnterSession()
	{
		if (!(NetcodeManager.Instance == null) && Player.player.RegionChild.Area is House house)
		{
			house.Enter();
		}
	}

	[Command("c_enter", "Enters the player into the session. Will screw up the session due to a lack of certain variables set. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void EnterSession(string buzzword1, string buzzword2, [AspectName] string aspect)
	{
		if (!(NetcodeManager.Instance == null) && Enum.TryParse<Aspect>(aspect, ignoreCase: false, out var result) && Player.player.RegionChild.Area is House house)
		{
			house.Enter(fromLoad: false, buzzword1, buzzword2, result);
		}
	}

	[Command("c_dungeon", "Generates and teleports the player into a dungeon. Incredibly dangerous and can screw things up. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string GenerateDungeon(int level = 10, int chunk = 0)
	{
		if (NetcodeManager.Instance == null)
		{
			return "";
		}
		if (!(Player.player.RegionChild.Area is House world))
		{
			return "Can only generate a dungeon when currently in a house area.";
		}
		Dungeon dungeon = DungeonManager.Build(world, chunk, level);
		Player.player.MoveToSpawn(dungeon);
		return $"Generated dungeon {dungeon.Id}.";
	}

	[Command("c_classpect", "Changes the player's classpect. This is only meant for testing and will break multiplayer sessions. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SetSelfClasspect([ClassName] string className, [AspectName] string aspectName)
	{
		if (!(NetcodeManager.Instance == null) && Enum.TryParse<Aspect>(aspectName, ignoreCase: false, out var result) && Enum.TryParse<Class>(className, ignoreCase: false, out var result2))
		{
			Player.player.SetClasspect(result, result2);
		}
	}

	[Command("c_prototype", "Prototypes the sprite. Only the host can use this command. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SetPrototype(string[] names)
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.KernelSprite.SetPrototype(names);
		}
	}

	[Command("c_home", "Teleports the player to the current spawn point. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void TPHome()
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.MoveToSpawn(Player.player.RegionChild.Area);
		}
	}

	[Command("c_homer", "Teleports the player to the current worlds spawn point (if you are in a dungeon, then the world the dungeon is in). Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void TPHomer()
	{
		if (!(NetcodeManager.Instance == null))
		{
			WorldArea worldArea = Player.player.RegionChild.Area;
			if (worldArea is Dungeon dungeon)
			{
				worldArea = dungeon.World;
			}
			Player.player.MoveToSpawn(worldArea);
		}
	}

	[Command("c_pos", "Prints the current local player position. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string PrintPos()
	{
		if (NetcodeManager.Instance == null)
		{
			return "";
		}
		return Player.player.GetPosition(local: true).ToString();
	}

	[Command("c_tp", "Teleports the player to the position indicated. Only works in the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void TPPlayer(float x, float y, float z)
	{
		if (!(NetcodeManager.Instance == null))
		{
			Player.player.SetPosition(new Vector3(x, y, z));
		}
	}

	[Command("c_dim", "If given no parameters, prints the current dimension of the player. Teleports the player to the specified dimension otherwise. Only works in the game", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string PlayerDimension()
	{
		if (NetcodeManager.Instance == null)
		{
			return "";
		}
		return Player.player.RegionChild.Area.Id.ToString();
	}

	[Command("c_dim", "If given no parameters, prints the current dimension of the player. Teleports the player to the specified dimension otherwise. Only works in the game", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static string PlayerDimension(int dimension)
	{
		if (NetcodeManager.Instance == null)
		{
			return "";
		}
		Player.player.RegionChild.Area = AbstractSingletonManager<WorldManager>.Instance.GetArea(dimension);
		return "Dimension successfully set";
	}

	[Command("d_save_session", "Saves the current session.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SaveSession()
	{
		if (!(NetcodeManager.Instance == null))
		{
			NetcodeManager.Instance.SaveSession();
		}
	}

	[Command("d_save_player", "Saves the current player.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SavePlayer()
	{
		if (!(NetcodeManager.Instance == null))
		{
			NetcodeManager.Instance.sessionSaveManager.SaveLocalPlayer();
		}
	}

	[Command("d_toggle_profiling", "Enables the profiler for some parts of the game.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void ToggleProfiling()
	{
		AbstractSingletonManager<ReallyBasicProfiler>.Instance.Enabled = !AbstractSingletonManager<ReallyBasicProfiler>.Instance.Enabled;
		QuantumConsole.Instance.LogToConsole("Profiling is now turned " + (AbstractSingletonManager<ReallyBasicProfiler>.Instance.Enabled ? "on" : "off") + ".");
	}

	[Command("d_profiling_report", "Generates a profiling report, if the profiler was turned on.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static List<MethodProfile> ProfilingReport()
	{
		QuantumConsole.Instance.LogToConsole("Generating report, this may take a while...");
		List<MethodProfile> result = AbstractSingletonManager<ReallyBasicProfiler>.Instance.GenerateReport();
		AbstractSingletonManager<ReallyBasicProfiler>.Instance.Clear();
		return result;
	}

	[Command("d_random_seed", "Shows the current Random seed", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static int ShowRandomSeed()
	{
		return SessionRandom.CurrentState;
	}

	[Command("net_transport", "Sets the transport type for the game. Not persistent!", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void SetTransportType(TransportType type)
	{
		transportType = type;
		QuantumConsole.Instance.LogToConsole("Transport type set to " + type);
	}

	[Command("net_transport", "Sets the transport type for the game. Not persistent!", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static TransportType GetTransportType()
	{
		return transportType;
	}

	[Command("net_host", "Hosts the game via Kcp.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void HostKcp(string address = "localhost", ushort port = 1025)
	{
		serverAddress = address;
		serverPort = port;
		hostGame = true;
		transportType = TransportType.Kcp;
		QuantumConsole.Instance.LogToConsole("Hosting game on port " + port);
		SceneManager.LoadScene("HouseBuildingCombo");
	}

	[Command("net_connect", "Connects to the specified server via Kcp.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void Connect(string ip, ushort port)
	{
		connectAddress = ip;
		connectPort = port;
		transportType = TransportType.Kcp;
		QuantumConsole.Instance.LogToConsole("Connecting to " + ip + ":" + port);
		SceneManager.LoadScene("HouseBuildingCombo");
	}

	[Command("net_disconnect", "Disconnects from the current server or stops hosting. Warning: This will not save the player.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void Disconnect()
	{
		if (!(NetcodeManager.Instance == null))
		{
			QuantumConsole.Instance.LogToConsole("Disconnecting from server.");
			NetcodeManager.Instance.ShutDownNetcode();
			SceneManager.LoadScene("MainMenuNew");
		}
	}

	[Command("net_debug", "Shows debug information about the network.", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	private static void ShowNetworkDebugInfo()
	{
		if (!(NetcodeManager.Instance == null))
		{
			QuantumConsole.Instance.LogToConsole($"Network active?: {NetcodeManager.Instance.isNetworkActive}");
			QuantumConsole.Instance.LogToConsole("Address: " + NetcodeManager.Instance.networkAddress);
			if (!(NetcodeManager.Instance.GetCurrentTransport() as KcpTransport == null))
			{
				KcpTransport kcpTransport = NetcodeManager.Instance.GetCurrentTransport() as KcpTransport;
				QuantumConsole.Instance.LogToConsole($"Port: {kcpTransport.Port}");
			}
		}
	}
}
