using UnityEngine;

public class DoorAction : InteractableAction
{
	public Transform targetRoom;

	public GameObject roomToggle;

	private void Awake()
	{
		sprite = Resources.Load<Sprite>("DoorAction");
		desc = "Use Door";
	}

	public override void Execute()
	{
		if (targetRoom != null && !targetRoom.Equals(null))
		{
			Fader.instance.fadedToBlack = OnFadeFinished;
			Fader.instance.BeginFade(1);
		}
		else
		{
			MonoBehaviour.print("Warning: Door's target room has not been set yet.");
		}
	}

	private void OnFadeFinished()
	{
		Fader.instance.BeginFade(-1);
		Player.player.LeaveStrife();
		Player.player.SetPosition(targetRoom.position);
	}
}
