using System.Collections;
using UnityEngine;

public class Platform : MonoBehaviour
{
	private static readonly int upHash = Animator.StringToHash("up");

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private GameObject collider;

	private WaitForSeconds animationDelay;

	private void Awake()
	{
		float length = animator.GetCurrentAnimatorStateInfo(0).length;
		animationDelay = new WaitForSeconds(length);
	}

	public bool GetState()
	{
		return animator.GetBool(upHash);
	}

	public void SetState(bool state)
	{
		animator.SetBool(upHash, state);
		if (state)
		{
			StartCoroutine(ActivateDelayed());
		}
		else
		{
			collider.SetActive(value: false);
		}
		IEnumerator ActivateDelayed()
		{
			yield return animationDelay;
			collider.SetActive(value: true);
		}
	}
}
