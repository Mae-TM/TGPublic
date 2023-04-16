using UnityEngine;

public class AddCaptchamodusAction : InteractableAction
{
	public string modus;

	public bool destroyObject = true;

	public bool destroyAction = true;

	public void Start()
	{
		sprite = Resources.Load<Sprite>("Modi/" + modus + "Modus");
		desc = "Get " + modus + "Modus";
	}

	public override void Execute()
	{
		Sylladex sylladex = Player.player.sylladex;
		if (!sylladex.HasModus(modus))
		{
			sylladex.AddModus(modus);
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
