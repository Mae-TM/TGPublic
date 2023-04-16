using Mirror;

public abstract class SyncedInteractableAction : InteractableAction
{
	private struct Message : NetworkMessage
	{
		public NetworkIdentity identity;
	}

	private NetworkIdentity identity;

	static SyncedInteractableAction()
	{
		NetcodeManager.RegisterStaticHandler<Message>(Receive);
	}

	protected virtual void Awake()
	{
		identity = GetComponentInParent<NetworkIdentity>();
	}

	public sealed override void Execute()
	{
		if (LocalExecute())
		{
			if (identity.netId == 0)
			{
				OfflineExecute();
				return;
			}
			Message message = default(Message);
			message.identity = identity;
			NetworkClient.Send(message);
		}
	}

	private static void Receive(NetworkConnection conn, Message msg)
	{
		msg.identity.GetComponentInChildren<SyncedInteractableAction>(includeInactive: true).ServerExecute(conn.identity.GetComponent<Player>());
	}

	protected virtual bool LocalExecute()
	{
		return true;
	}

	protected abstract void ServerExecute(Player player);

	protected virtual void OfflineExecute()
	{
	}
}
