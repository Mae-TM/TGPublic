using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
	private static MSPAOrthoController cameraToFace;

	public bool overrideSpin;

	private void Start()
	{
		if (cameraToFace == null)
		{
			cameraToFace = MSPAOrthoController.main.GetComponent<MSPAOrthoController>();
		}
	}

	public void LateUpdate()
	{
		Quaternion billboardRotation = cameraToFace.GetBillboardRotation();
		if (overrideSpin)
		{
			float z = base.transform.localEulerAngles.z;
			base.transform.rotation = billboardRotation;
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			localEulerAngles.z = z;
			base.transform.localEulerAngles = localEulerAngles;
		}
		else
		{
			base.transform.rotation = billboardRotation;
		}
	}

	private void OnBecameVisible()
	{
		base.enabled = true;
	}

	private void OnBecameInvisible()
	{
		base.enabled = false;
	}
}
