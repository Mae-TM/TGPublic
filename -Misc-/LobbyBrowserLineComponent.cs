using UnityEngine;
using UnityEngine.UI;

public class LobbyBrowserLineComponent : MonoBehaviour
{
	private TGPLobby lobby;

	[SerializeField]
	private bool hasLobby;

	[SerializeField]
	private Text TxtName;

	[SerializeField]
	private Text CurrentPlayers;

	[SerializeField]
	private Text maxPlayers;

	[SerializeField]
	private GameObject lockIcon;

	[SerializeField]
	private GameObject button;

	[SerializeField]
	private GameObject leaveButton;

	[SerializeField]
	private LobbyBrowserComponent browser;

	public void JoinLobby()
	{
		if (lobby.HasPassword)
		{
			browser.lobbyCreateDialog.OnSubmit += PasswordCheck;
			browser.lobbyCreateDialog.Show();
		}
		else
		{
			browser.lobbySceneComponent.JoinLobby(lobby.SteamLobby.Id);
		}
		void PasswordCheck(string password)
		{
			if (lobby.IsPasswordCorrect(password))
			{
				browser.lobbySceneComponent.JoinLobby(lobby.SteamLobby.Id);
			}
			else
			{
				browser.DisplayError("Wrong password!");
			}
			browser.lobbyCreateDialog.OnSubmit -= PasswordCheck;
		}
	}

	public void LeaveSession()
	{
	}

	public void SetLobby(TGPLobby tgpLobby)
	{
		hasLobby = true;
		lobby = tgpLobby;
		TxtName.text = tgpLobby.Name;
		CurrentPlayers.text = tgpLobby.SteamLobby.MemberCount.ToString();
		lockIcon.SetActive(tgpLobby.HasPassword);
	}

	public void SetSession(object session)
	{
		hasLobby = false;
	}
}
