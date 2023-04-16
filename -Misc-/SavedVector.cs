using System;
using UnityEngine;

[Serializable]
public struct SavedVector
{
	public float x;

	public float y;

	public float z;

	public SavedVector(Vector3 vec)
	{
		x = vec.x;
		y = vec.y;
		z = vec.z;
	}

	public static implicit operator Vector3(SavedVector i)
	{
		return new Vector3(i.x, i.y, i.z);
	}

	public static implicit operator SavedVector(Vector3 i)
	{
		return new SavedVector(i);
	}
}
