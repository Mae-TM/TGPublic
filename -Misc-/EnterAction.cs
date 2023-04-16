using Mirror;
using UnityEngine;

public class EnterAction : SyncedInteractableAction, IAttackTrigger
{
	private void Start()
	{
		sprite = Resources.Load<Sprite>("Alchemise");
		desc = "Enter";
	}

	protected override void ServerExecute(Player player)
	{
		player.transform.root.GetComponent<EntryCountdown>().Enter();
	}

	[ServerCallback]
	public void Trigger()
	{
		if (NetworkServer.active)
		{
			base.transform.root.GetComponent<EntryCountdown>().Enter();
		}
	}

	private void OnDestroy()
	{
		if (!GetComponent<RegionChild>())
		{
			Object.Destroy(base.transform.parent.gameObject);
		}
	}
}
