using UnityEngine;

public class SleepAction : InteractableAction
{
	[SerializeField]
	private Transform sleepPosition;

	[SerializeField]
	private string trigger = "Sleep";

	public void Start()
	{
		sprite = Resources.Load<Sprite>("Sleep");
		desc = "Sleep";
	}

	public override void Execute()
	{
		Player.player.sync.Sleep(sleepPosition.position, trigger);
	}
}
