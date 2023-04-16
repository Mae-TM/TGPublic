using UnityEngine;

public class SleepBehaviour : StateMachineBehaviour
{
	private Attacking self;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		self = animator.GetComponentInParent<Attacking>();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		self.Health += self.HealthRegen * 4f * Time.deltaTime;
		self.Vim += self.VimRegen * 4f * Time.deltaTime;
	}
}
