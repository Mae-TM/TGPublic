using UnityEngine;

public class ChestBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<ItemAcceptorMono>().ShowUI();
	}
}
