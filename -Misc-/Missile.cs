using System;
using UnityEngine;

public class Missile : MonoBehaviour
{
	private static Missile prefab;

	private Vector3 origin;

	private Vector3 offset;

	private Transform target;

	private float spawn;

	private float time;

	private Action action;

	public static Missile Make(MonoBehaviour origin, Attackable target, float time, Color color, float width, float length, Action action)
	{
		Collider collider = target.Collider;
		return Make(origin.transform.position + new Vector3(0f, collider.bounds.extents.y, 0f), collider, time, color, width, length, action);
	}

	public static Missile Make(Vector3 origin, Collider target, float time, Color color, float width, float length, Action action)
	{
		if (prefab == null)
		{
			prefab = Resources.Load<Missile>("Trail");
		}
		Missile missile = UnityEngine.Object.Instantiate(prefab, origin, Quaternion.identity);
		missile.origin = origin;
		missile.target = target.transform;
		missile.offset = new Vector3(0f, target.bounds.extents.y, 0f);
		missile.spawn = Time.time;
		missile.time = time;
		missile.action = action;
		TrailRenderer component = missile.GetComponent<TrailRenderer>();
		component.startColor = color;
		component.endColor = new Color(color.r, color.g, color.b, 0f);
		component.widthMultiplier = width;
		component.time = length;
		return missile;
	}

	private void Update()
	{
		if (target == null || target.Equals(null))
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		float num = (Time.time - spawn) / time;
		if (num < 1f)
		{
			base.transform.position = Vector3.Lerp(origin, target.position + offset, num * num);
			return;
		}
		action();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
