using UnityEngine;

public class TotemLatheStateBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponentInChildren<TotemLathe>().FinishCarving();
	}
}
