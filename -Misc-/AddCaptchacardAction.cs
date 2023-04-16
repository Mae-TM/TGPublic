using UnityEngine;

public class AddCaptchacardAction : InteractableAction
{
	public int cardCount = 1;

	public bool destroyObject = true;

	public bool destroyAction = true;

	public void Start()
	{
		desc = "Get Card";
	}

	public override void Execute()
	{
		if (Player.player.sylladex.AddCaptchaCard(cardCount))
		{
			if (destroyObject)
			{
				Object.Destroy(base.gameObject);
			}
			else if (destroyAction)
			{
				GetComponent<Interactable>().RemoveOption(this);
				Object.Destroy(this);
			}
		}
	}
}
