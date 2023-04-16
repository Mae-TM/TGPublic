using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class StatusEffects : NetworkBehaviour, IEnumerable<IStatusEffect>, IEnumerable
{
	public delegate void OnAffectHandler(ref IStatusEffect effect);

	private readonly SyncList<IStatusEffect> effects = new SyncList<IStatusEffect>();

	private readonly PriorityQueue<IStatusEffect> toUpdate = new PriorityQueue<IStatusEffect>();

	[SerializeField]
	[HideInInspector]
	private Attackable owner;

	public event OnAffectHandler OnAffect;

	private void OnValidate()
	{
		if ((object)owner == null)
		{
			owner = GetComponent<Attackable>();
		}
	}

	private void Awake()
	{
		effects.Callback += OnEffectsChanged;
	}

	private void OnEffectsChanged(SyncList<IStatusEffect>.Operation op, int index, IStatusEffect oldEffect, IStatusEffect newEffect)
	{
		switch (op)
		{
		case SyncList<IStatusEffect>.Operation.OP_REMOVEAT:
			oldEffect.Stop(owner);
			break;
		case SyncList<IStatusEffect>.Operation.OP_ADD:
			newEffect.Begin(owner);
			toUpdate.Add(newEffect, Time.time);
			break;
		default:
			Debug.LogWarning($"Operation {op} was used on StatusEffects!");
			break;
		}
	}

	private void Update()
	{
		float num;
		while (toUpdate.Count != 0 && (num = toUpdate.TopPriority()) < Time.time)
		{
			IStatusEffect statusEffect = toUpdate.Pop();
			if (!(statusEffect.EndTime <= Time.time))
			{
				float num2 = statusEffect.Update(owner);
				if (num2 > 0f)
				{
					num += num2;
				}
				else if (num2 < 0f && base.isServer)
				{
					effects.Remove(statusEffect);
				}
				else
				{
					num = Time.time;
				}
				if (num2 >= 0f && statusEffect.EndTime > num)
				{
					toUpdate.Add(statusEffect, num);
				}
			}
		}
		if (base.isServer)
		{
			effects.RemoveAll((IStatusEffect e) => e.EndTime <= Time.time);
		}
	}

	public void OnAttacked(Attack attack)
	{
		effects.RemoveAll((IStatusEffect e) => e.OnAttacked(attack) && base.isServer);
	}

	public void OnAttack(Attack attack)
	{
		effects.RemoveAll((IStatusEffect e) => e.OnAttack(attack) && base.isServer);
	}

	public void AfterAttack(Attack attack)
	{
		effects.RemoveAll((IStatusEffect e) => e.AfterAttack(attack) && base.isServer);
	}

	[Command]
	public void CmdAdd(IStatusEffect effect)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteStatusEffect(effect);
		SendCommandInternal(typeof(StatusEffects), "CmdAdd", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public bool Add<T>(T effect, bool stacking = true) where T : IStatusEffect
	{
		int num = -1;
		if (!stacking)
		{
			num = effects.FindIndex((IStatusEffect e) => e is T);
			if (num != -1 && effects[num].EndTime >= effect.EndTime)
			{
				return false;
			}
		}
		IStatusEffect effect2 = effect;
		this.OnAffect?.Invoke(ref effect2);
		if (effect2 == null)
		{
			return false;
		}
		if (!base.isServer)
		{
			return true;
		}
		if (num != -1 && effect2 is T)
		{
			effects.RemoveAt(num);
		}
		effects.Add(effect2);
		return true;
	}

	public IEnumerable<T> GetEffects<T>() where T : IStatusEffect
	{
		return effects.OfType<T>();
	}

	public bool Remove<T>() where T : IStatusEffect
	{
		int num = effects.FindIndex((IStatusEffect e) => e is T);
		if (num == -1)
		{
			return false;
		}
		if (base.isServer)
		{
			effects.RemoveAt(num);
		}
		return true;
	}

	[ServerCallback]
	public void Remove(IStatusEffect effect)
	{
		if (NetworkServer.active)
		{
			effects.Remove(effect);
		}
	}

	[ServerCallback]
	public void RemoveAll<T>() where T : IStatusEffect
	{
		if (NetworkServer.active)
		{
			effects.RemoveAll((IStatusEffect e) => e is T);
		}
	}

	[ServerCallback]
	public void Clear()
	{
		if (NetworkServer.active)
		{
			effects.RemoveAll((IStatusEffect e) => !float.IsPositiveInfinity(e.EndTime));
		}
	}

	[Server]
	public StatusEffect.Data[] Save()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatusEffect/Data[] StatusEffects::Save()' called when server was not active");
			return null;
		}
		return effects.Select(StatusEffect.Save).ToArray();
	}

	[Server]
	public void Load(StatusEffect.Data[] list)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffects::Load(StatusEffect/Data[])' called when server was not active");
		}
		else if (list != null)
		{
			effects.AddRange(list.Select(StatusEffect.Load));
		}
	}

	public IEnumerator<IStatusEffect> GetEnumerator()
	{
		return effects.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return effects.GetEnumerator();
	}

	public StatusEffects()
	{
		InitSyncObject(effects);
	}

	private void MirrorProcessed()
	{
	}

	public void UserCode_CmdAdd(IStatusEffect effect)
	{
		Add(effect);
	}

	protected static void InvokeUserCode_CmdAdd(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAdd called on client.");
		}
		else
		{
			((StatusEffects)obj).UserCode_CmdAdd(reader.ReadStatusEffect());
		}
	}

	static StatusEffects()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(StatusEffects), "CmdAdd", InvokeUserCode_CmdAdd, requiresAuthority: true);
	}
}
