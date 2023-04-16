using UnityEngine;

public class BuildModeAction : InteractableAction
{
	private void Start()
	{
		sprite = Resources.Load<Sprite>("Sburb_Logo");
		desc = "Build";
	}

	public override void Execute()
	{
		BuildExploreSwitcher.Instance.SwitchToBuild(GetComponentInParent<Furniture>());
	}
}
