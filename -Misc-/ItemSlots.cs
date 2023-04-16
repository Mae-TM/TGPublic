using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ItemSlots : NetworkBehaviour, IEnumerable<ItemSlot>, IEnumerable
{
	public Func<bool, NetworkConnectionToClient, bool> ServerAnimate;

	public Action<bool> OnAnimate;

	private ItemSlot[] slots;

	public bool IsSet => slots != null;

	public int Length => slots.Length;

	public ItemSlot this[int i] => slots[i];

	public void Set(IEnumerable<ItemSlot> to)
	{
		Set(to.ToArray());
	}

	public void Set(params ItemSlot[] to)
	{
		slots = to;
		for (int i = 0; i < slots.Length; i++)
		{
			int index = i;
			slots[i].OnItemChanged = delegate
			{
				OnSlotChanged(index);
			};
		}
	}

	public IEnumerator<ItemSlot> GetEnumerator()
	{
		return slots.AsEnumerable().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return slots.GetEnumerator();
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (initialState)
		{
			ItemSlot[] array = slots;
			foreach (ItemSlot itemSlot in array)
			{
				writer.Write(itemSlot.item);
			}
		}
		return base.OnSerialize(writer, initialState);
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			ItemSlot[] array = slots;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetItemDirect(reader.Read<Item>());
			}
		}
		base.OnDeserialize(reader, initialState);
	}

	private void OnSlotChanged(int index)
	{
		if (base.isServer)
		{
			RpcSetSlot(index, slots[index].item);
		}
		else
		{
			CmdSetSlot(index, slots[index].item);
		}
	}

	[ClientRpc]
	private void RpcSetSlot(int index, Item item)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(index);
		writer.WriteItem(item);
		SendRPCInternal(typeof(ItemSlots), "RpcSetSlot", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command(requiresAuthority = false)]
	private void CmdSetSlot(int index, Item item)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(index);
		writer.WriteItem(item);
		SendCommandInternal(typeof(ItemSlots), "CmdSetSlot", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcAnimate(bool value)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(value);
		SendRPCInternal(typeof(ItemSlots), "RpcAnimate", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdAnimate(bool value, NetworkConnectionToClient conn = null)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(value);
		SendCommandInternal(typeof(ItemSlots), "CmdAnimate", writer, 0, requiresAuthority: false);
		NetworkWriterPool.Recycle(writer);
	}

	protected void OnDestroy()
	{
		if (IsSet)
		{
			ItemSlot[] array = slots;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].item?.Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	private void UserCode_RpcSetSlot(int index, Item item)
	{
		slots[index].SetItemDirect(item);
	}

	protected static void InvokeUserCode_RpcSetSlot(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSlot called on server.");
		}
		else
		{
			((ItemSlots)obj).UserCode_RpcSetSlot(reader.ReadInt(), reader.ReadItem());
		}
	}

	private void UserCode_CmdSetSlot(int index, Item item)
	{
		slots[index].SetItemDirect(item);
		RpcSetSlot(index, item);
	}

	protected static void InvokeUserCode_CmdSetSlot(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSlot called on client.");
		}
		else
		{
			((ItemSlots)obj).UserCode_CmdSetSlot(reader.ReadInt(), reader.ReadItem());
		}
	}

	private void UserCode_RpcAnimate(bool value)
	{
		OnAnimate(value);
	}

	protected static void InvokeUserCode_RpcAnimate(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnimate called on server.");
		}
		else
		{
			((ItemSlots)obj).UserCode_RpcAnimate(reader.ReadBool());
		}
	}

	public void UserCode_CmdAnimate(bool value, NetworkConnectionToClient conn)
	{
		if (ServerAnimate == null || ServerAnimate(value, conn))
		{
			RpcAnimate(value);
			if (!base.isClient)
			{
				OnAnimate(value);
			}
		}
	}

	protected static void InvokeUserCode_CmdAnimate(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAnimate called on client.");
		}
		else
		{
			((ItemSlots)obj).UserCode_CmdAnimate(reader.ReadBool(), senderConnection);
		}
	}

	static ItemSlots()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(ItemSlots), "CmdSetSlot", InvokeUserCode_CmdSetSlot, requiresAuthority: false);
		RemoteCallHelper.RegisterCommandDelegate(typeof(ItemSlots), "CmdAnimate", InvokeUserCode_CmdAnimate, requiresAuthority: false);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ItemSlots), "RpcSetSlot", InvokeUserCode_RpcSetSlot);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ItemSlots), "RpcAnimate", InvokeUserCode_RpcAnimate);
	}
}
