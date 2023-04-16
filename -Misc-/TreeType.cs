using System;
using UnityEngine;

[Serializable]
public struct TreeType
{
	public GameObject[] subtypes;

	public int maxDensity;

	private Bounds[] bounds;

	public TreeType(int maxDensity, params GameObject[] subtypes)
	{
		this.subtypes = subtypes;
		this.maxDensity = maxDensity;
		bounds = new Bounds[subtypes.Length];
	}

	public (GameObject, Bounds, bool) GetPrefabBounds(int subtype)
	{
		GameObject gameObject = subtypes[subtype];
		Bounds bounds = this.bounds[subtype];
		if (bounds != default(Bounds))
		{
			return (gameObject, bounds, false);
		}
		gameObject = UnityEngine.Object.Instantiate(gameObject);
		bounds = ModelUtility.GetBounds(gameObject);
		bounds.center -= gameObject.transform.localPosition;
		this.bounds[subtype] = bounds;
		return (gameObject, bounds, true);
	}
}
