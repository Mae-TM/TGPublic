using UnityEngine;

public class AnimatorRepetitions : StateMachineBehaviour
{
	private static readonly int hash = Animator.StringToHash("Repetitions");

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetInteger(hash, animator.GetInteger(hash) - 1);
	}

	public static void SetRepetitions(Animator animator, int value)
	{
		animator.SetInteger(hash, value);
	}
}
