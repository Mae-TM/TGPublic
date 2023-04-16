using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

public class House : Building
{
	private GameObject background;

	public readonly SyncSet<NormalItem> atheneum = new SyncHashSet<NormalItem>();

	private EntryCountdown countdown;

	private Player owner;

	private readonly Stack<BuildingChanges> undoStack = new Stack<BuildingChanges>();

	private readonly Stack<BuildingChanges> redoStack = new Stack<BuildingChanges>();

	private int storyCountAtFirstEntry = -1;

	private bool spawnLoadCleared;

	private readonly List<Vector3> newFloorCache = new List<Vector3>();

	private readonly List<Enemy> onBuildPopData = new List<Enemy>();

	private readonly List<Enemy> onRandomSpawnPopData = new List<Enemy>();

	private Enemy[] randomEnemies;

	private Enemy[] newTileEnemies;

	private static AssetBundle backgroundBundle;

	public bool IsDestroyed => countdown.timerTime <= 0f;

	public Planet planet { get; private set; }

	public bool HasPlanet => (object)planet != null;

	public Player Owner
	{
		get
		{
			return owner;
		}
		set
		{
			owner = value;
			ownerColor = value.sync.np.character.color;
		}
	}

	public Color ownerColor { get; private set; }

	public Color cruxiteColor => ColorSelector.GetCruxiteColor(ownerColor);

	public bool HasCruxtruder => GetComponentInChildren<Cruxtruder>();

	public bool HasTotemLathe => GetComponentInChildren<TotemLathe>();

	public bool HasAlchemiter => GetComponentInChildren<Alchemiter>();

	public bool HasPunchDesignix => GetComponentInChildren<PunchDesignix>();

	public bool HasPrepunchedCard => GetComponentsInChildren<ItemObject>().Any((ItemObject item) => item.Item.IsEntry);

	private static AssetBundle BackgroundBundle
	{
		get
		{
			if (backgroundBundle == null)
			{
				backgroundBundle = AssetBundleExtensions.Load("background");
			}
			return backgroundBundle;
		}
	}

	public int GetGrist(int tier)
	{
		if (!HasPlanet)
		{
			return 0;
		}
		return planet.GetGrist(tier);
	}

	private void Awake()
	{
		countdown = GetComponent<EntryCountdown>();
	}

	private void Start()
	{
		if (!Owner && WorldManager.colors != null && base.Id < WorldManager.colors.Length)
		{
			ownerColor = WorldManager.colors[base.Id];
		}
	}

	public void Enter(bool fromLoad = false, string file1 = null, string file2 = null, Aspect aspect = Aspect.Count)
	{
		if ((bool)Owner)
		{
			Owner.Entered();
		}
		if (!NetcodeManager.Instance.offline)
		{
			SessionRandom.Seed(base.Id);
		}
		planet = Planet.Build(this, file1, file2, aspect);
		SetBackground(IsFloorLess(background.name) ? "stilts" : "cliff", includeWalls: false);
		ambience.ambientLight = planet.AmbientLight;
		StartSpawnRoutines();
	}

	public override WorldRegion GetRegion(Vector3 position)
	{
		if (!HasPlanet || position.y >= -0.1f)
		{
			return base.GetRegion(position);
		}
		WorldRegion chunk = planet.GetChunk(position);
		if (!chunk)
		{
			return GetStory(0, createStories: false).Outside;
		}
		return chunk;
	}

	public override void LoadStructure(HouseData data)
	{
		base.LoadStructure(data);
		SetBackground(data.background);
	}

	protected override HouseData SaveStructure()
	{
		HouseData result = base.SaveStructure();
		if ((bool)background)
		{
			result.background = background.name;
		}
		return result;
	}

	public void LoadProgress(HouseProgress progress, Player owner = null)
	{
		if (progress.atheneum != null)
		{
			atheneum.UnionWith(progress.atheneum.Select(Item.Load).Cast<NormalItem>());
		}
		countdown.timerTime = progress.timerTime;
		countdown.state = progress.cruxtruderState;
		if (!owner)
		{
			if (NetcodeManager.Instance.TryGetPlayer(base.Id, out var player))
			{
				Owner = player;
			}
		}
		else
		{
			Owner = owner;
		}
		if (countdown.state == CruxtruderState.Entered)
		{
			Enter(fromLoad: true);
		}
	}

	public HouseProgress SaveProgress()
	{
		HouseProgress result = default(HouseProgress);
		result.atheneum = atheneum.Select((NormalItem item) => item.Save()).ToArray();
		result.timerTime = countdown.timerTime;
		result.cruxtruderState = countdown.state;
		return result;
	}

	public bool PayGrist(int cost)
	{
		if (!Owner || Owner.Grist[Grist.SpecialType.Build] < cost)
		{
			return false;
		}
		Owner.Grist[Grist.SpecialType.Build] -= cost;
		return true;
	}

	[Command(requiresAuthority = false)]
	public void CmdUndoCommand()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(House), "CmdUndoCommand", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdRedoCommand()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(House), "CmdRedoCommand", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	private void UndoRedoCommand(Stack<BuildingChanges> fromStack, Stack<BuildingChanges> toStack)
	{
		if (fromStack.Count != 0 && IsAllowed(fromStack.Peek()))
		{
			BuildingChanges buildingChanges = fromStack.Pop();
			if (base.isServerOnly)
			{
				ExecuteCommand(buildingChanges);
			}
			RpcExecuteCommand(buildingChanges);
			buildingChanges.Invert();
			toStack.Push(buildingChanges);
		}
	}

	private void DoCommand(BuildingChanges command)
	{
		command.Finish();
		if (command.changes.Count != 0 && IsAllowed(command))
		{
			if (base.isServerOnly)
			{
				ExecuteCommand(command);
			}
			RpcExecuteCommand(command);
			command.Invert();
			undoStack.Push(command);
			redoStack.Clear();
		}
	}

	private bool IsAllowed(BuildingChanges command)
	{
		foreach (BuildingChanges.Change change in command.changes)
		{
			Story story = GetStory(change.story);
			if (change.changes.Sign)
			{
				if (!story.AreWallChangesAllowed(change.changes, 0, change.room))
				{
					return false;
				}
				continue;
			}
			change.changes.Invert();
			if ((!story.IsGround && change.changes.GetRectangles().Any((RectInt rect) => story.OverlapsFloorFurniture(rect))) || !story.AreWallChangesAllowed(change.changes, change.room, 0))
			{
				change.changes.Invert();
				return false;
			}
			change.changes.Invert();
		}
		foreach (BuildingChanges.RoomTransfer transfer in command.transfers)
		{
			if (!GetStory(transfer.story).AreWallChangesAllowed(transfer.changes, transfer.from, transfer.to))
			{
				return false;
			}
		}
		return PayGrist(command.GetCost());
	}

	[ClientRpc]
	private void RpcExecuteCommand(BuildingChanges command)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_BuildingChanges(writer, command);
		SendRPCInternal(typeof(House), "RpcExecuteCommand", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void ExecuteCommand(BuildingChanges command)
	{
		foreach (BuildingChanges.Change change in command.changes)
		{
			GetStory(change.story).AddFloor(change.changes, change.room);
			AddSpawnPoint(change.story, change.changes);
		}
		foreach (BuildingChanges.RoomTransfer transfer in command.transfers)
		{
			GetStory(transfer.story).SetRoom(transfer.changes, transfer.from, transfer.to);
		}
		foreach (BuildingChanges.Change change2 in command.changes)
		{
			GetStory(change2.story).GenerateOutsideWalls();
		}
		foreach (BuildingChanges.RoomTransfer transfer2 in command.transfers)
		{
			GetStory(transfer2.story).GenerateOutsideWalls();
		}
		SoundEffects.Instance.Kachunk(this);
	}

	[Command(requiresAuthority = false)]
	public void CmdSetFloor(RectInt rect, int y)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteRectInt(rect);
		writer.WriteInt(y);
		SendCommandInternal(typeof(House), "CmdSetFloor", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdSetRoom(RectInt rect, int y, int room)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteRectInt(rect);
		writer.WriteInt(y);
		writer.WriteInt(room);
		SendCommandInternal(typeof(House), "CmdSetRoom", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdRemoveRoom(RectInt rect, int story)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteRectInt(rect);
		writer.WriteInt(story);
		SendCommandInternal(typeof(House), "CmdRemoveRoom", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	private void AddSpawnPoint(int y, AAPoly changes)
	{
		if (changes.Sign)
		{
			newFloorCache.AddRange(from cell in changes.GetCells()
				select Building.GetPosition(new Vector3Int(cell.x, y, cell.y)));
		}
	}

	public void StartSpawnRoutines()
	{
		StartCoroutine(FlushSpawnCache());
	}

	private Vector3 RandomPosition()
	{
		float num = Random.Range(-1f, 1f);
		float x = num * Mathf.Abs(num) * 45f;
		float num2 = Random.Range(-1f, 1f);
		num2 *= Mathf.Abs(num2);
		num2 *= 45f;
		float y = ((!((Random.value < 0.1f) | (base.StoryMax == 0))) ? ((float)(Random.Range(1, base.StoryMax + 1) * 3) * 1.5f) : 0f);
		return new Vector3(x, y, num2);
	}

	private IEnumerable<Vector3> PlausiblePositions(int count)
	{
		Vector3 housePos = base.transform.position;
		for (int i = 0; i < count; i++)
		{
			yield return RandomPosition() + housePos;
		}
	}

	private void DoRandomSpawn(int spotsCount)
	{
		float num = float.MinValue;
		Vector3? vector = null;
		foreach (Vector3 item in PlausiblePositions(spotsCount))
		{
			float shortestDistanceFromPlayer = GetShortestDistanceFromPlayer(item);
			if (shortestDistanceFromPlayer >= num && NavMesh.SamplePosition(item, out var hit, 0.8f, -1))
			{
				num = shortestDistanceFromPlayer;
				vector = hit.position;
			}
		}
		if (vector.HasValue)
		{
			if (onRandomSpawnPopData.Count == 0)
			{
				RefreshRandomPopData();
			}
			SpawnHelper.instance.Spawn(onRandomSpawnPopData[onRandomSpawnPopData.Count - 1], this, vector.Value, GetGrist(0));
			onRandomSpawnPopData.RemoveAt(onRandomSpawnPopData.Count - 1);
		}
	}

	private float SpawnDelay()
	{
		return 2f + 10f * Mathf.Sqrt(Enemy.EnemyCount());
	}

	private IEnumerator FlushSpawnCache()
	{
		if (!base.isServer || !HasPlanet)
		{
			yield break;
		}
		randomEnemies = SpawnHelper.instance.GetCreatures<Enemy>(new string[1] { "Imp" });
		newTileEnemies = SpawnHelper.instance.GetCreatures<Enemy>(new string[3] { "Imp", "Ogre", "Gremlin" });
		yield return new WaitForSeconds(10f);
		if (!spawnLoadCleared)
		{
			newFloorCache.Clear();
			spawnLoadCleared = true;
			DoRandomSpawn(5);
		}
		float nextTime = Time.time + SpawnDelay();
		while (true)
		{
			if (newFloorCache.Count == 0)
			{
				yield return new WaitForSeconds(8f);
				if (Time.time >= nextTime)
				{
					nextTime = Time.time + SpawnDelay();
					DoRandomSpawn(8);
				}
				yield return new WaitForSeconds(3f);
				continue;
			}
			Vector3[] arrayCache = newFloorCache.ToArray();
			newFloorCache.Clear();
			yield return new WaitForSeconds(1f);
			List<NavMeshHit> spawnPointCache = new List<NavMeshHit>();
			Vector3[] array = arrayCache;
			for (int i = 0; i < array.Length; i++)
			{
				if (NavMesh.SamplePosition(array[i] + base.transform.position + 0.1f * Vector3.down, out var hit, 1f, -1))
				{
					spawnPointCache.Add(hit);
				}
			}
			yield return new WaitForSeconds(1f);
			while (spawnPointCache.Count >= 1)
			{
				if (onBuildPopData.Count <= 0)
				{
					RefreshBuildPopData();
				}
				int index = Random.Range(0, spawnPointCache.Count - 1 + 1);
				NavMeshHit hit2 = spawnPointCache[index];
				Vector3 position = hit2.position;
				spawnPointCache.RemoveAt(index);
				if ((double)Random.value >= 0.9 && NavMesh.SamplePosition(position, out hit2, 1f, -1))
				{
					int index2 = Random.Range(0, onBuildPopData.Count - 1 + 1);
					Enemy prefab = onBuildPopData[index2];
					onBuildPopData.RemoveAt(index2);
					SpawnHelper.instance.Spawn(prefab, this, hit2.position, GetGrist(0));
					yield return new WaitForSeconds(3f);
				}
			}
		}
	}

	private static float GetShortestDistanceFromPlayer(Vector3 origin)
	{
		if (!Player.GetAll().Any())
		{
			return float.PositiveInfinity;
		}
		return Player.GetAll().Min((Player p) => (p.transform.position - origin).magnitude);
	}

	private void RefreshRandomPopData()
	{
		onRandomSpawnPopData.AddRange(GetEnemies(randomEnemies, 10));
	}

	private void RefreshBuildPopData()
	{
		for (int i = 1; i <= base.StoryMax + 1; i++)
		{
			int value = Mathf.RoundToInt((float)(i * 2 - 1) * (1f + (float)(i % 5 - 2) / 4f));
			onBuildPopData.AddRange(GetEnemies(newTileEnemies, value));
		}
	}

	private static IEnumerable<Enemy> GetEnemies(Enemy[] enemies, int value)
	{
		while (value > 0)
		{
			foreach (Enemy enemy in enemies)
			{
				int cost = enemy.GetCost();
				if (cost <= value)
				{
					yield return enemy;
					value -= cost;
				}
			}
		}
	}

	public static string[] GetBackgroundOptions()
	{
		return BackgroundBundle.GetAllAssetNames();
	}

	private static bool IsFloorLess(string background)
	{
		return new string[2] { "apartment", "stilts" }.Contains(background);
	}

	public void SetBackground(string prefabName, bool includeWalls = true)
	{
		if ((bool)background)
		{
			Object.Destroy(background);
		}
		Transform transform = GetStory(0).Outside.transform;
		background = new GameObject(prefabName);
		background.transform.SetParent(transform, worldPositionStays: false);
		bool flag = !IsFloorLess(prefabName);
		GetStory(0).IsGround = flag;
		if (includeWalls && flag)
		{
			BoxCollider boxCollider = background.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(90f, 8f, 90f);
			boxCollider.center = new Vector3(0f, -4f, 0f);
			background.AddComponent<NavMeshSourceTag>();
		}
		if (includeWalls)
		{
			MakeBorderWalls();
		}
		GameObject gameObject = ((prefabName == null) ? null : BackgroundBundle.LoadAsset<GameObject>(prefabName));
		if ((bool)gameObject)
		{
			Object.Instantiate(gameObject, background.transform).name = prefabName;
		}
		Visibility.Copy(background, transform.gameObject);
	}

	private void MakeBorderWalls()
	{
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (i != 0 || j != 0)
				{
					BoxCollider boxCollider = new GameObject("World Edge")
					{
						layer = LayerMask.NameToLayer("Ignore Raycast"),
						tag = "Fixed Layer"
					}.AddComponent<BoxCollider>();
					Transform transform = boxCollider.transform;
					transform.SetParent(background.transform, worldPositionStays: false);
					boxCollider.size = new Vector3(90f, 30f, 90f);
					transform.localPosition = new Vector3(90f * (float)i, 7f, 90f * (float)j);
				}
			}
		}
	}

	public House()
	{
		InitSyncObject(atheneum);
	}

	private void MirrorProcessed()
	{
	}

	public void UserCode_CmdUndoCommand()
	{
		UndoRedoCommand(undoStack, redoStack);
	}

	protected static void InvokeUserCode_CmdUndoCommand(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUndoCommand called on client.");
		}
		else
		{
			((House)obj).UserCode_CmdUndoCommand();
		}
	}

	public void UserCode_CmdRedoCommand()
	{
		UndoRedoCommand(redoStack, undoStack);
	}

	protected static void InvokeUserCode_CmdRedoCommand(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRedoCommand called on client.");
		}
		else
		{
			((House)obj).UserCode_CmdRedoCommand();
		}
	}

	private void UserCode_RpcExecuteCommand(BuildingChanges command)
	{
		ExecuteCommand(command);
	}

	protected static void InvokeUserCode_RpcExecuteCommand(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcExecuteCommand called on server.");
		}
		else
		{
			((House)obj).UserCode_RpcExecuteCommand(GeneratedNetworkCode._Read_BuildingChanges(reader));
		}
	}

	public void UserCode_CmdSetFloor(RectInt rect, int y)
	{
		Story story = GetStory(y);
		if (story.ContainsFloor(rect))
		{
			DoCommand(story.SetFloor(rect));
		}
	}

	protected static void InvokeUserCode_CmdSetFloor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetFloor called on client.");
		}
		else
		{
			((House)obj).UserCode_CmdSetFloor(reader.ReadRectInt(), reader.ReadInt());
		}
	}

	public void UserCode_CmdSetRoom(RectInt rect, int y, int room)
	{
		Story story = GetStory(y);
		if (story.ContainsFloor(rect))
		{
			BuildingChanges buildingChanges = story.SetRoom(rect, room);
			buildingChanges.Add(GetStory(y + 1).SetFloor(rect));
			DoCommand(buildingChanges);
		}
	}

	protected static void InvokeUserCode_CmdSetRoom(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetRoom called on client.");
		}
		else
		{
			((House)obj).UserCode_CmdSetRoom(reader.ReadRectInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	public void UserCode_CmdRemoveRoom(RectInt rect, int story)
	{
		DoCommand(GetStory(story).RemoveRoom(rect, GetStory(story - 1), GetStory(story + 1)));
	}

	protected static void InvokeUserCode_CmdRemoveRoom(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveRoom called on client.");
		}
		else
		{
			((House)obj).UserCode_CmdRemoveRoom(reader.ReadRectInt(), reader.ReadInt());
		}
	}

	static House()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(House), "CmdUndoCommand", InvokeUserCode_CmdUndoCommand, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(House), "CmdRedoCommand", InvokeUserCode_CmdRedoCommand, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(House), "CmdSetFloor", InvokeUserCode_CmdSetFloor, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(House), "CmdSetRoom", InvokeUserCode_CmdSetRoom, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(House), "CmdRemoveRoom", InvokeUserCode_CmdRemoveRoom, requiresAuthority: false);
		RemoteCallHelper.RegisterRpcDelegate(typeof(House), "RpcExecuteCommand", InvokeUserCode_RpcExecuteCommand);
	}
}
