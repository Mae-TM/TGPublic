using UnityEngine;

public class OpenPesterchum : InteractableAction
{
	public GameObject pesterchumUi;

	private void Start()
	{
		sprite = Resources.Load<Sprite>("pesterchum");
		desc = "Pester";
	}

	public override void Execute()
	{
		if (pesterchumUi == null)
		{
			pesterchumUi = Object.FindObjectOfType<PesterchumHandler>().pesterchumWindow;
		}
		pesterchumUi.SetActive(value: true);
	}
}
