using UnityEngine;

public class Shadow : MonoBehaviour
{
	public Transform shadow;

	public float dist = 0.1f;

	public LayerMask layers;

	private void Update()
	{
		if (Physics.Raycast(base.transform.position, -base.transform.up, out var hitInfo, 3f, layers))
		{
			shadow.up = hitInfo.normal;
			shadow.position = hitInfo.normal * (dist + Vector3.Dot(hitInfo.point, hitInfo.normal)) + Vector3.ProjectOnPlane(base.transform.position, hitInfo.normal);
			shadow.gameObject.SetActive(value: true);
		}
		else
		{
			shadow.gameObject.SetActive(value: false);
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
