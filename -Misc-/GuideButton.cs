using Steamworks;
using UnityEngine;

public class GuideButton : MonoBehaviour
{
	private const string url = "https://steamcommunity.com/sharedfiles/filedetails/?id=2792280245";

	public void OpenSteamGuide()
	{
		SteamFriends.OpenWebOverlay("https://steamcommunity.com/sharedfiles/filedetails/?id=2792280245");
	}
}
