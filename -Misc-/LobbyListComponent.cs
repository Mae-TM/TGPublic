using System.Collections.Generic;
using UnityEngine;

public class LobbyListComponent : MonoBehaviour
{
	[SerializeField]
	private LobbyBrowserLineComponent lobbyTemplate;

	public void AddLobby(TGPLobby lobby)
	{
		LobbyBrowserLineComponent lobbyBrowserLineComponent = Object.Instantiate(lobbyTemplate, base.transform, worldPositionStays: true);
		lobbyBrowserLineComponent.gameObject.SetActive(value: true);
		lobbyBrowserLineComponent.SetLobby(lobby);
	}

	public void AddSession()
	{
		Debug.LogWarning("Not yet implmeneted");
	}

	public void ClearLobbies()
	{
		if (lobbyTemplate.gameObject.activeSelf)
		{
			lobbyTemplate.gameObject.SetActive(value: false);
		}
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.activeSelf)
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	public void SetSessions(IEnumerable<object> sessions)
	{
	}
}
