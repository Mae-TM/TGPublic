using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class VersionUtil : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Text>().text = $"{Application.version} (Build {SteamApps.BuildId})";
		Debug.Log($"The Genesis Project, v{Application.version} (Build {SteamApps.BuildId})");
	}

	private void Update()
	{
	}
}
