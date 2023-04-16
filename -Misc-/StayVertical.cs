using UnityEngine;

public class StayVertical : MonoBehaviour
{
	private void Start()
	{
		float x = base.gameObject.transform.rotation.eulerAngles.x;
		float z = base.gameObject.transform.rotation.eulerAngles.z;
		base.gameObject.transform.Rotate(0f - x, 0f, 0f - z);
	}

	private void Update()
	{
	}
}
