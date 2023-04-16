using UnityEngine;

public class AnimateAction : SyncedInteractableAction
{
	public string altText;

	public Animator animator;

	public string trigger;

	protected override void ServerExecute(Player player)
	{
		animator.SetTrigger(trigger);
		if (!string.IsNullOrEmpty(altText))
		{
			string text = altText;
			string text2 = desc;
			desc = text;
			altText = text2;
		}
	}
}
