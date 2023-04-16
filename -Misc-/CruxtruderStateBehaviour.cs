using UnityEngine;

public class CruxtruderStateBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<Cruxtruder>().Opened();
	}
}
