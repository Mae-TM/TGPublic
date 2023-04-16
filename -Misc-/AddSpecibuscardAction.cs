using UnityEngine;

public class AddSpecibuscardAction : InteractableAction
{
	public int cardCount = 1;

	public bool destroyObject = true;

	public bool destroyAction = true;

	public void Start()
	{
		desc = "Get Strife card";
	}

	public override void Execute()
	{
		Specibus strifeSpecibus = Player.player.sylladex.strifeSpecibus;
		if (strifeSpecibus != null)
		{
			strifeSpecibus.Size += cardCount;
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
