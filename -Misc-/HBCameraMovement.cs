using UnityEngine;

public class HBCameraMovement : MonoBehaviour
{
	private Vector3 posLerpTo;

	private bool sPressed;

	private bool wPressed;

	private void Update()
	{
		sPressed = Input.GetKey(KeyCode.S);
		wPressed = Input.GetKey(KeyCode.W);
	}

	private void FixedUpdate()
	{
		if (sPressed)
		{
			sPressed = false;
			posLerpTo = base.transform.position;
			posLerpTo -= Vector3.forward * -1f;
		}
		else if (wPressed)
		{
			wPressed = false;
			posLerpTo = base.transform.position;
			posLerpTo -= Vector3.forward;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, posLerpTo, 0.5f);
	}
}
