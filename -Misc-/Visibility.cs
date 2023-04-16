using UnityEngine;

public static class Visibility
{
	public static bool Get(GameObject gameObject)
	{
		return gameObject.layer != 8;
	}

	public static void Set(GameObject gameObject, bool value)
	{
		if (Get(gameObject) == value)
		{
			return;
		}
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!transform.CompareTag("Fixed Layer"))
			{
				transform.gameObject.layer = ((!value) ? 8 : 0);
			}
		}
	}

	public static void Copy(GameObject to, GameObject from)
	{
		Set(to, Get(from));
	}

	public static void SetParent(Component comp, Component parent)
	{
		comp.transform.SetParent(parent.transform, worldPositionStays: false);
		Copy(comp.gameObject, parent.gameObject);
	}
}
