using UnityEngine;

public class AlchemiterStateBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.GetComponentInChildren<Alchemiter>().SpawnObject();
	}
}
