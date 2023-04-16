using System;
using UnityEngine;

public class DeathBehaviour : StateMachineBehaviour
{
	public event Action<float> OnPlay;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		this.OnPlay?.Invoke(stateInfo.length);
	}
}
