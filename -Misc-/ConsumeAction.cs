using UnityEngine;

public class ConsumeAction : SyncedInteractableAction
{
	public NormalItem item;

	public float duration = 2f;

	private AudioClip clip;

	private IStatusEffect Effect => new ItemStatusEffect(item, duration);

	protected override void Awake()
	{
		base.Awake();
		sprite = Resources.Load<Sprite>("Consume");
		desc = "Eat";
		clip = Resources.Load<AudioClip>("Music/tgp_eat");
	}

	private void Consume(Attackable consumer)
	{
		item.OnConsume(consumer);
		AudioSource.PlayClipAtPoint(clip, consumer.transform.position);
		if (base.gameObject != null)
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	protected override void ServerExecute(Player player)
	{
		player.Affect(Effect);
		Consume(player);
	}

	protected override void OfflineExecute()
	{
		Player.player.StatusEffects.CmdAdd(Effect);
		Consume(Player.player);
	}
}
