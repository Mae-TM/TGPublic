using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
	public GameObject debugMenuObject;

	public Text playerCountText;

	public Text logText;

	private string textToLog = "";

	private void Update()
	{
		logText.text += textToLog;
		textToLog = "";
		if (Input.GetKeyDown(KeyCode.F9))
		{
			debugMenuObject.SetActive(!debugMenuObject.activeSelf);
		}
		if (debugMenuObject.activeSelf)
		{
			playerCountText.text = "Player count: " + NetcodeManager.Instance.PlayerCount + "\nNet component count: " + NetworkIdentity.spawned.Count + "\nConnection lost: " + NetcodeManager.Instance.ConnectionLost;
		}
	}
}
