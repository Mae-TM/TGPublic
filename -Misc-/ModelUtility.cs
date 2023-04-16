using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class ModelUtility
{
	public static Bounds GetBounds(GameObject gameObject, bool includeInactive = false, bool includeTrigger = false)
	{
		Bounds result = default(Bounds);
		Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>(includeInactive);
		foreach (Collider collider in componentsInChildren)
		{
			if (includeTrigger || !collider.isTrigger)
			{
				if (result.size == Vector3.zero)
				{
					result = collider.bounds;
				}
				else
				{
					result.Encapsulate(collider.bounds);
				}
			}
		}
		return result;
	}

	public static Bounds TransformBounds(Bounds bounds, Vector3 translation, Quaternion rotation)
	{
		bounds.Encapsulate(rotation * bounds.min);
		bounds.Encapsulate(rotation * bounds.max);
		bounds.center += translation;
		return bounds;
	}

	public static Vector3 RandomInBounds(Bounds bounds)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
	}

	public static Vector3 GetExtent(Bounds bounds, Vector3 direction, float addedDistance = 0f)
	{
		return direction * (addedDistance + AbsDot(bounds.extents, direction));
	}

	public static Vector3 GetSpawnPos(Component toSpawn, Component source, Vector3 direction, bool includeRaycast = true, float safetyDistance = 0.1f)
	{
		Bounds bounds = GetBounds(source.gameObject);
		return GetSpawnPos(toSpawn, bounds, direction, includeRaycast, safetyDistance);
	}

	public static Vector3 GetSpawnPos(Component toSpawn, Bounds sourceBounds, Vector3 direction, bool includeRaycast = true, float safetyDistance = 0.1f)
	{
		float num = AbsDot(GetBounds(toSpawn.gameObject).extents, direction);
		float num2 = safetyDistance + num + AbsDot(sourceBounds.extents, direction);
		if (includeRaycast && Physics.Raycast(sourceBounds.center, direction, out var hitInfo, num2 + num))
		{
			return hitInfo.point - num * direction;
		}
		return sourceBounds.center + direction * num2;
	}

	public static void MakeNavMeshObstacle(GameObject gameObject, Bounds totalBounds, bool local = false)
	{
		if (gameObject.GetComponentInChildren<NavMeshObstacle>() == null)
		{
			NavMeshObstacle navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
			navMeshObstacle.carving = true;
			if (local)
			{
				navMeshObstacle.size = totalBounds.size;
				navMeshObstacle.center = totalBounds.center;
			}
			else
			{
				navMeshObstacle.size = Abs(navMeshObstacle.transform.InverseTransformVector(totalBounds.size));
				navMeshObstacle.center = navMeshObstacle.transform.InverseTransformPoint(totalBounds.center);
			}
		}
	}

	public static Vector3 GetBottom(GameObject o)
	{
		Collider[] componentsInChildren = o.GetComponentsInChildren<Collider>();
		if (componentsInChildren.Length == 0)
		{
			return default(Vector3);
		}
		Vector3 negativeInfinity = Vector3.negativeInfinity;
		Vector3 lhs = Vector3.positiveInfinity;
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			Vector3 max = collider.bounds.max;
			negativeInfinity.x = Mathf.Max(negativeInfinity.x, max.x);
			negativeInfinity.z = Mathf.Max(negativeInfinity.z, max.z);
			lhs = Vector3.Min(lhs, collider.bounds.min);
		}
		return new Vector3((negativeInfinity.x + lhs.x) / 2f, lhs.y, (negativeInfinity.z + lhs.z) / 2f);
	}

	public static float AbsDot(Vector3 a, Vector3 b)
	{
		return Mathf.Abs(a.x * b.x) + Mathf.Abs(a.y * b.y) + Mathf.Abs(a.z * b.z);
	}

	public static Vector3 Abs(Vector3 a)
	{
		return new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
	}

	public static Vector3 Divide(Vector3 n, Vector3 d)
	{
		return new Vector3(n.x / d.x, n.y / d.y, n.z / d.z);
	}

	public static IEnumerable<Collider> OverlapCone(Vector3 position, float radius, Vector3 forward, float angle)
	{
		if (angle == 0f || angle == 360f)
		{
			return Physics.OverlapSphere(position, radius);
		}
		List<Collider> list = new List<Collider>();
		Collider[] array = Physics.OverlapSphere(position, radius);
		foreach (Collider collider in array)
		{
			if (Vector3.Angle(forward, collider.transform.position - position) <= angle)
			{
				list.Add(collider);
			}
		}
		return list;
	}
}
