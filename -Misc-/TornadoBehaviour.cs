using System;
using UnityEngine;
using UnityEngine.AI;

public class TornadoBehaviour : MonoBehaviour
{
	private Vector3 startingPosition;

	private double hasClockwiseMovement;

	private int timeOffset;

	private bool active;

	private DisplacementMapFlat terrain;

	private void Awake()
	{
		UnityEngine.Object.Destroy(GetComponent<NavMeshObstacle>());
		hasClockwiseMovement = UnityEngine.Random.Range(0, 2);
		timeOffset = UnityEngine.Random.Range(0, 151);
	}

	private void Update()
	{
		if (!terrain)
		{
			terrain = GetComponentInParent<DisplacementMapFlat>();
			startingPosition = base.transform.position;
		}
		double num = (DateTime.UtcNow - DateTime.MinValue).TotalSeconds + (double)timeOffset;
		float x = (float)Math.Cos((num + hasClockwiseMovement * 75.0) / 150.0 * (Math.PI * 2.0)) * 30f;
		float z = (float)Math.Sin(num / 150.0 * (Math.PI * 2.0)) * 30f;
		Vector3 position = startingPosition + new Vector3(x, 0f, z);
		float y = terrain.SampleHeight(position);
		base.transform.position = new Vector3(position.x, y, position.z);
		active = true;
	}

	private void OnTriggerStay(Collider other)
	{
		if (active)
		{
			Attacking.Explosion(base.transform.position, 0f, 30f, null, 2f, visual: false);
			active = false;
		}
	}
}
