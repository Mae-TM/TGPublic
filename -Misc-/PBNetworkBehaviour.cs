using Mirror;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public readonly struct PBNetworkBehaviour<T> where T : NetworkBehaviour
{
	[ProtoMember(1)]
	private readonly uint id;

	private PBNetworkBehaviour(uint id)
	{
		this.id = id;
	}

	public static implicit operator PBNetworkBehaviour<T>(T behaviour)
	{
		return new PBNetworkBehaviour<T>(((Object)behaviour) ? behaviour.netId : 0u);
	}

	public static implicit operator T(PBNetworkBehaviour<T> pb)
	{
		if (!NetworkIdentity.spawned.TryGetValue(pb.id, out var value))
		{
			return null;
		}
		return value.GetComponent<T>();
	}
}
