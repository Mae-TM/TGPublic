using System.Collections.Generic;
using UnityEngine;

public class Prototype : MonoBehaviour
{
	public string protoName;

	public static List<Prototype> total = new List<Prototype>();

	private void OnEnable()
	{
		total.Add(this);
	}

	private void OnDisable()
	{
		total.Remove(this);
	}
}
