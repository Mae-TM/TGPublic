using UnityEngine;

public class SlideBehaviour : StateMachineBehaviour
{
	private PlayerMovement controller;

	private Vector3 movement;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		controller = animator.GetComponentInParent<PlayerMovement>();
		Player component = controller.GetComponent<Player>();
		movement = component.sync.GetForward() * component.Speed * 2f / stateInfo.speedMultiplier;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		controller.Move(movement);
	}
}
