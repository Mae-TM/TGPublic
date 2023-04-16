using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RegionChild))]
public class ItemObject : NetworkBehaviour
{
	[SerializeField]
	[HideInInspector]
	private Rigidbody body;

	[SerializeField]
	[HideInInspector]
	private NetworkTransform networkTransform;

	[SerializeField]
	[HideInInspector]
	private RegionChild regionChild;

	public Item Item { get; private set; }

	public static event Action<ItemObject> OnGetAuthority;

	private void OnValidate()
	{
		if ((object)regionChild == null)
		{
			regionChild = GetComponent<RegionChild>();
		}
		if ((object)networkTransform == null)
		{
			networkTransform = GetComponent<NetworkTransform>();
		}
		networkTransform.clientAuthority = true;
		if ((object)body == null)
		{
			body = GetComponent<Rigidbody>();
		}
		body.isKinematic = true;
	}

	public override void OnStartServer()
	{
		body.isKinematic = false;
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write((HouseData.Item)Item);
		}
		return base.OnSerialize(writer, initialState);
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			if (Item == null)
			{
				Item = Item.Load(reader.Read<HouseData.Item>(), this);
				Item.ItemObject = this;
			}
			else
			{
				reader.Read<HouseData.Item>();
			}
		}
		base.OnDeserialize(reader, initialState);
	}

	public static void Make(Item item, GameObject prefab)
	{
		if (!UnityEngine.Object.Instantiate(prefab).TryGetComponent<ItemObject>(out var component))
		{
			Debug.LogError($"{item} has no ItemObject component!");
			return;
		}
		component.Item = item;
		item.ItemObject = component;
		if (NetworkServer.active)
		{
			NetworkServer.Spawn(component.gameObject);
		}
	}

	[Server]
	public void PutDown(WorldArea area, Vector3 position, Quaternion rotation, NetworkConnection owner)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ItemObject::PutDown(WorldArea,UnityEngine.Vector3,UnityEngine.Quaternion,Mirror.NetworkConnection)' called when server was not active");
			return;
		}
		regionChild.Area = area;
		NetworkServer.Spawn(base.gameObject, owner);
		if (owner != null && base.netIdentity.AssignClientAuthority(owner))
		{
			body.isKinematic = true;
		}
		networkTransform.ServerTeleport(position, rotation);
	}

	[Command(requiresAuthority = false)]
	public void CmdPickUp()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(ItemObject), "CmdPickUp", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdRequestAuthority(NetworkConnectionToClient sender = null)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(ItemObject), "CmdRequestAuthority", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	public override void OnStartAuthority()
	{
		ItemObject.OnGetAuthority?.Invoke(this);
	}

	[Command]
	public void CmdReleaseAuthority()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(ItemObject), "CmdReleaseAuthority", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRecycle()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(ItemObject), "CmdRecycle", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	public void UserCode_CmdPickUp()
	{
		regionChild.Area = null;
	}

	protected static void InvokeUserCode_CmdPickUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUp called on client.");
		}
		else
		{
			((ItemObject)obj).UserCode_CmdPickUp();
		}
	}

	public void UserCode_CmdRequestAuthority(NetworkConnectionToClient sender)
	{
		if (base.netIdentity.AssignClientAuthority(sender))
		{
			body.isKinematic = true;
		}
	}

	protected static void InvokeUserCode_CmdRequestAuthority(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestAuthority called on client.");
		}
		else
		{
			((ItemObject)obj).UserCode_CmdRequestAuthority(senderConnection);
		}
	}

	public void UserCode_CmdReleaseAuthority()
	{
		base.netIdentity.RemoveClientAuthority();
		body.isKinematic = false;
	}

	protected static void InvokeUserCode_CmdReleaseAuthority(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReleaseAuthority called on client.");
		}
		else
		{
			((ItemObject)obj).UserCode_CmdReleaseAuthority();
		}
	}

	public void UserCode_CmdRecycle()
	{
		if (regionChild.Area is House house)
		{
			if (Item is Totem totem && !totem.IsEntry)
			{
				house.atheneum.Add(totem.makeItem);
			}
			else
			{
				house.Owner.Grist.Add(Item.GetCost());
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected static void InvokeUserCode_CmdRecycle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRecycle called on client.");
		}
		else
		{
			((ItemObject)obj).UserCode_CmdRecycle();
		}
	}

	static ItemObject()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(ItemObject), "CmdPickUp", InvokeUserCode_CmdPickUp, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(ItemObject), "CmdRequestAuthority", InvokeUserCode_CmdRequestAuthority, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(ItemObject), "CmdReleaseAuthority", InvokeUserCode_CmdReleaseAuthority, requiresAuthority: true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(ItemObject), "CmdRecycle", InvokeUserCode_CmdRecycle, requiresAuthority: true);
	}
}
