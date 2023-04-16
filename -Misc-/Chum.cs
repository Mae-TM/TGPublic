using System;
using UnityEngine;

[Serializable]
public struct Chum
{
	public string username;

	public Chum(string username)
	{
		Debug.Log(username);
		this.username = username;
	}
}
