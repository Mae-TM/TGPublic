using System;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using QuadTrees.QTreeRect;
using UnityEngine;

public abstract class Furniture : NetworkBehaviour, IRectQuadStorable
{
	private struct SpawnMessage : NetworkMessage
	{
		public string furniture;

		public NetworkBehaviour building;
	}

	[SerializeField]
	[Min(0f)]
	private int cost;

	[SerializeField]
	private bool unique;

	[SyncVar(hook = "OnActiveChange")]
	private bool isActive = true;

	[SyncVar(hook = "OnBuildingChange")]
	protected Building building;

	[SyncVar(hook = "OnCoordsChange")]
	protected Vector3Int coords;

	[SyncVar(hook = "OnOrientationChange")]
	protected Orientation orientation;

	private NetworkBehaviourSyncVar ___buildingNetId;

	[field: SerializeField]
	public Vector2Int Size { get; protected set; }

	public virtual bool AllowMovement { get; set; } = true;


	public RectInt Rect => GetRect(coords, Size);

	public bool NetworkisActive
	{
		get
		{
			return isActive;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref isActive))
			{
				bool from = isActive;
				SetSyncVar(value, ref isActive, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					OnActiveChange(from, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public Building Networkbuilding
	{
		get
		{
			return GetSyncVarNetworkBehaviour(___buildingNetId, ref building);
		}
		[param: In]
		set
		{
			if (!SyncVarNetworkBehaviourEqual(value, ___buildingNetId))
			{
				Building networkbuilding = Networkbuilding;
				SetSyncVarNetworkBehaviour(value, ref building, 2uL, ref ___buildingNetId);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, value: true);
					OnBuildingChange(networkbuilding, value);
					setSyncVarHookGuard(2uL, value: false);
				}
			}
		}
	}

	public Vector3Int Networkcoords
	{
		get
		{
			return coords;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref coords))
			{
				Vector3Int oldValue = coords;
				SetSyncVar(value, ref coords, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, value: true);
					OnCoordsChange(oldValue, value);
					setSyncVarHookGuard(4uL, value: false);
				}
			}
		}
	}

	public Orientation Networkorientation
	{
		get
		{
			return orientation;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref orientation))
			{
				Orientation oldValue = orientation;
				SetSyncVar(value, ref orientation, 8uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
				{
					setSyncVarHookGuard(8uL, value: true);
					OnOrientationChange(oldValue, value);
					setSyncVarHookGuard(8uL, value: false);
				}
			}
		}
	}

	public static event Action<Furniture> OnGetAuthority;

	protected static RectInt GetRect(Vector3Int coords, Vector2Int size)
	{
		return new RectInt(new Vector2Int(coords.x, coords.z), size);
	}

	private static Furniture GetInstance(string name, Building building, NetworkConnection owner)
	{
		Furniture furniturePrefab = HouseManager.Instance.GetFurniturePrefab(name);
		if ((object)furniturePrefab == null)
		{
			Debug.LogWarning("Furniture with name '" + name + "' not found!");
			return null;
		}
		if (owner != null && building is House house && !house.PayGrist(furniturePrefab.cost))
		{
			return null;
		}
		return UnityEngine.Object.Instantiate(furniturePrefab);
	}

	[Server]
	private static Furniture Make(string name, Building building, Vector3Int coords, Orientation orientation, NetworkConnection owner = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Furniture Furniture::Make(System.String,Building,UnityEngine.Vector3Int,Orientation,Mirror.NetworkConnection)' called when server was not active");
			return null;
		}
		Furniture furniture = (name.StartsWith("Prefabs/") ? CreatureSpawn.GetInstance(name, building, coords) : GetInstance(name, building, owner));
		if ((object)furniture == null)
		{
			return null;
		}
		furniture.name = name;
		furniture.Networkcoords = coords;
		furniture.Networkorientation = orientation;
		furniture.Networkbuilding = building;
		return furniture;
	}

	public static void Make(string name, Furniture template)
	{
		NetworkServer.Spawn(Make(name, template.Networkbuilding, template.coords, template.orientation).gameObject);
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (!initialState)
		{
			return base.OnSerialize(writer, initialState: false);
		}
		writer.Write(Networkbuilding);
		writer.Write(coords);
		writer.Write(orientation);
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (!initialState)
		{
			base.OnDeserialize(reader, initialState: false);
			return;
		}
		if ((bool)Networkbuilding && isActive)
		{
			BeforeMoving(Networkbuilding, coords, this.orientation);
		}
		Orientation orientation = this.orientation;
		Networkbuilding = reader.Read<NetworkBehaviour>() as Building;
		Networkcoords = reader.Read<Vector3Int>();
		Networkorientation = reader.Read<Orientation>();
		if ((int)(this.orientation - orientation) % 2 != 0)
		{
			Size = new Vector2Int(Size.y, Size.x);
		}
		AfterMoving(Networkbuilding, coords, this.orientation);
	}

	private void OnEnable()
	{
		if ((bool)Networkbuilding && !base.isServer)
		{
			AfterMoving(Networkbuilding, coords, orientation);
		}
	}

	private void OnDisable()
	{
		if ((bool)Networkbuilding && !base.isServer)
		{
			BeforeMoving(Networkbuilding, coords, orientation);
		}
	}

	private void OnDestroy()
	{
		if ((bool)Networkbuilding && base.isServer && isActive)
		{
			BeforeMoving(Networkbuilding, coords, orientation);
		}
	}

	protected virtual void OnValidate()
	{
		if (Size == default(Vector2Int))
		{
			CalculateSize();
		}
	}

	protected abstract void CalculateSize();

	protected abstract void BeforeMoving(Building oldBuilding, Vector3Int oldCoords, Orientation oldOrientation);

	protected abstract void AfterMoving(Building newBuilding, Vector3Int newCoords, Orientation newOrientation);

	private void OnBuildingChange(Building oldValue, Building newValue)
	{
		if (isActive && !base.isServer)
		{
			if ((bool)oldValue)
			{
				BeforeMoving(oldValue, coords, orientation);
			}
			AfterMoving(newValue, coords, orientation);
		}
	}

	private void OnCoordsChange(Vector3Int oldValue, Vector3Int newValue)
	{
		if (isActive && (bool)Networkbuilding && !base.isServer)
		{
			BeforeMoving(Networkbuilding, oldValue, orientation);
		}
		base.transform.localPosition = Building.GetPosition(GetRect(newValue, Size), newValue.y);
		if (isActive && (bool)Networkbuilding && !base.isServer)
		{
			AfterMoving(Networkbuilding, newValue, orientation);
		}
	}

	private void OnOrientationChange(Orientation oldValue, Orientation newValue)
	{
		if (isActive && (bool)Networkbuilding && !base.isServer)
		{
			BeforeMoving(Networkbuilding, coords, oldValue);
		}
		base.transform.Rotate(Vector3.up, 90f * ((float)(int)newValue - (float)(int)oldValue), Space.World);
		if ((int)(newValue - oldValue) % 2 != 0)
		{
			Size = new Vector2Int(Size.y, Size.x);
			base.transform.localPosition = Building.GetPosition(GetRect(coords, Size), coords.y);
		}
		if (isActive && (bool)Networkbuilding && !base.isServer)
		{
			AfterMoving(Networkbuilding, coords, newValue);
		}
	}

	private void CheckOccupied(NetworkConnection owner)
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		int[] array = new int[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject = componentsInChildren[i].gameObject;
			array[i] = gameObject.layer;
			gameObject.layer = 2;
		}
		if (IsValidPlace())
		{
			if (!isActive)
			{
				base.gameObject.SetActive(value: true);
				NetworkisActive = true;
				NetworkServer.Spawn(base.gameObject, owner);
			}
			AfterMoving(Networkbuilding, coords, orientation);
		}
		else if (isActive)
		{
			NetworkisActive = false;
			base.gameObject.SetActive(value: false);
		}
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			componentsInChildren[num].gameObject.layer = array[num];
		}
	}

	protected abstract bool IsValidPlace();

	public bool SetCoords(Vector3Int to)
	{
		if (coords != to)
		{
			CmdSetCoords(to);
		}
		return isActive;
	}

	[Command]
	private void CmdSetCoords(Vector3Int to, NetworkConnectionToClient sender = null)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3Int(to);
		SendCommandInternal(typeof(Furniture), "CmdSetCoords", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRotate(bool isClockwise, NetworkConnectionToClient sender = null)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isClockwise);
		SendCommandInternal(typeof(Furniture), "CmdRotate", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnActiveChange(bool from, bool to)
	{
		base.gameObject.SetActive(to);
	}

	[Command(requiresAuthority = false)]
	public void CmdRequestAuthority(NetworkConnectionToClient sender = null)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(Furniture), "CmdRequestAuthority", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	public override void OnStartAuthority()
	{
		Furniture.OnGetAuthority?.Invoke(this);
	}

	[Command]
	public void CmdReleaseAuthority()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(Furniture), "CmdReleaseAuthority", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRecycle()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(Furniture), "CmdRecycle", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void Recycle()
	{
		UnityEngine.Object.Destroy(base.gameObject);
		if (Networkbuilding is House house)
		{
			house.PayGrist(-cost);
		}
	}

	static Furniture()
	{
		NetcodeManager.RegisterStaticHandler<SpawnMessage>(Spawn);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Furniture), "CmdSetCoords", InvokeUserCode_CmdSetCoords, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Furniture), "CmdRotate", InvokeUserCode_CmdRotate, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Furniture), "CmdRequestAuthority", InvokeUserCode_CmdRequestAuthority, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Furniture), "CmdReleaseAuthority", InvokeUserCode_CmdReleaseAuthority, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Furniture), "CmdRecycle", InvokeUserCode_CmdRecycle, requiresAuthority: true);
	}

	public static void Spawn(string furnitureName, Building building)
	{
		SpawnMessage message = default(SpawnMessage);
		message.furniture = furnitureName;
		message.building = building;
		NetworkClient.Send(message);
	}

	private static void Spawn(NetworkConnection sender, SpawnMessage message)
	{
		Furniture furniture = Make(message.furniture, message.building as Building, default(Vector3Int), Orientation.NORTH, sender);
		if ((object)furniture != null)
		{
			NetworkServer.Spawn(furniture.gameObject, sender);
		}
	}

	public HouseData.Furniture Save()
	{
		HouseData.Furniture result = default(HouseData.Furniture);
		result.name = base.name;
		result.x = coords.x;
		result.z = coords.z;
		result.orientation = orientation;
		result.items = (TryGetComponent<ItemSlots>(out var component) ? component.Select((ItemSlot slot) => slot.item?.Save()).ToArray() : null);
		return result;
	}

	public static void Load(HouseData.Furniture data, Building building, int y)
	{
		if (data.name == "Player")
		{
			return;
		}
		Furniture furniture = Make(coords: new Vector3Int(data.x, y, data.z), name: data.name, building: building, orientation: data.orientation);
		if ((object)furniture == null)
		{
			return;
		}
		if (data.items != null)
		{
			ItemSlots component = furniture.GetComponent<ItemSlots>();
			for (short num = 0; num < Math.Min(component.Length, data.items.Length); num = (short)(num + 1))
			{
				component[num].SetItemDirect(data.items[num]);
			}
		}
		NetworkServer.Spawn(furniture.gameObject);
	}

	public static int GetCost(string furniture)
	{
		return HouseManager.Instance.GetFurniturePrefab(furniture).cost;
	}

	private void MirrorProcessed()
	{
	}

	private void UserCode_CmdSetCoords(Vector3Int to, NetworkConnectionToClient sender)
	{
		if (isActive)
		{
			BeforeMoving(Networkbuilding, coords, orientation);
		}
		Networkcoords = to;
		CheckOccupied(sender);
	}

	protected static void InvokeUserCode_CmdSetCoords(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetCoords called on client.");
		}
		else
		{
			((Furniture)obj).UserCode_CmdSetCoords(reader.ReadVector3Int(), senderConnection);
		}
	}

	public void UserCode_CmdRotate(bool isClockwise, NetworkConnectionToClient sender)
	{
		if (isActive)
		{
			BeforeMoving(Networkbuilding, coords, orientation);
		}
		Networkorientation = (Orientation)(((int)orientation + (isClockwise ? 1 : 3)) % 4);
		CheckOccupied(sender);
	}

	protected static void InvokeUserCode_CmdRotate(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRotate called on client.");
		}
		else
		{
			((Furniture)obj).UserCode_CmdRotate(reader.ReadBool(), senderConnection);
		}
	}

	public void UserCode_CmdRequestAuthority(NetworkConnectionToClient sender)
	{
		base.netIdentity.AssignClientAuthority(sender);
	}

	protected static void InvokeUserCode_CmdRequestAuthority(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestAuthority called on client.");
		}
		else
		{
			((Furniture)obj).UserCode_CmdRequestAuthority(senderConnection);
		}
	}

	public void UserCode_CmdReleaseAuthority()
	{
		base.netIdentity.RemoveClientAuthority();
		if (!isActive)
		{
			Recycle();
		}
	}

	protected static void InvokeUserCode_CmdReleaseAuthority(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReleaseAuthority called on client.");
		}
		else
		{
			((Furniture)obj).UserCode_CmdReleaseAuthority();
		}
	}

	public void UserCode_CmdRecycle()
	{
		Recycle();
	}

	protected static void InvokeUserCode_CmdRecycle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRecycle called on client.");
		}
		else
		{
			((Furniture)obj).UserCode_CmdRecycle();
		}
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isActive);
			writer.WriteNetworkBehaviour(Networkbuilding);
			writer.WriteVector3Int(coords);
			GeneratedNetworkCode._Write_Orientation(writer, orientation);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isActive);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteNetworkBehaviour(Networkbuilding);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteVector3Int(coords);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			GeneratedNetworkCode._Write_Orientation(writer, orientation);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = isActive;
			NetworkisActive = reader.ReadBool();
			if (!SyncVarEqual(flag, ref isActive))
			{
				OnActiveChange(flag, isActive);
			}
			NetworkBehaviourSyncVar __buildingNetId = ___buildingNetId;
			Building networkbuilding = Networkbuilding;
			___buildingNetId = reader.ReadNetworkBehaviourSyncVar();
			if (!SyncVarEqual(__buildingNetId, ref ___buildingNetId))
			{
				OnBuildingChange(networkbuilding, Networkbuilding);
			}
			Vector3Int vector3Int = coords;
			Networkcoords = reader.ReadVector3Int();
			if (!SyncVarEqual(vector3Int, ref coords))
			{
				OnCoordsChange(vector3Int, coords);
			}
			Orientation orientation = this.orientation;
			Networkorientation = GeneratedNetworkCode._Read_Orientation(reader);
			if (!SyncVarEqual(orientation, ref this.orientation))
			{
				OnOrientationChange(orientation, this.orientation);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = isActive;
			NetworkisActive = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref isActive))
			{
				OnActiveChange(flag2, isActive);
			}
		}
		if ((num & 2L) != 0L)
		{
			NetworkBehaviourSyncVar __buildingNetId2 = ___buildingNetId;
			Building networkbuilding2 = Networkbuilding;
			___buildingNetId = reader.ReadNetworkBehaviourSyncVar();
			if (!SyncVarEqual(__buildingNetId2, ref ___buildingNetId))
			{
				OnBuildingChange(networkbuilding2, Networkbuilding);
			}
		}
		if ((num & 4L) != 0L)
		{
			Vector3Int vector3Int2 = coords;
			Networkcoords = reader.ReadVector3Int();
			if (!SyncVarEqual(vector3Int2, ref coords))
			{
				OnCoordsChange(vector3Int2, coords);
			}
		}
		if ((num & 8L) != 0L)
		{
			Orientation orientation2 = this.orientation;
			Networkorientation = GeneratedNetworkCode._Read_Orientation(reader);
			if (!SyncVarEqual(orientation2, ref this.orientation))
			{
				OnOrientationChange(orientation2, this.orientation);
			}
		}
	}
}
