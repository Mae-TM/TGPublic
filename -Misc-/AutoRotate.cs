using System;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
	public GameObject upObject;

	private void Start()
	{
	}

	private void Update()
	{
		if (upObject.GetComponent<CharacterController>().velocity != Vector3.zero)
		{
			Vector3 vector = upObject.transform.position - base.transform.position;
			_ = vector.normalized;
			Vector3 axis = new Vector3(vector.z, 0f, 0f - vector.x);
			_ = axis.magnitude;
			float y = vector.y;
			float num = Mathf.Atan2(axis.magnitude, y) * 180f / (float)Math.PI;
			base.transform.RotateAround(upObject.transform.position, axis, 0f - num);
		}
	}
}
