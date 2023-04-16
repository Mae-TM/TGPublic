using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyNameWindow : MonoBehaviour
{
	public InputField txtName;

	public InputField txtPassword;

	public Dropdown visDropdown;

	[FormerlySerializedAs("lobby")]
	public LobbySceneComponent lobbyScene;

	public void OnEnable()
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyJoined += LobbyManagerOnLobbyJoined;
	}

	private void LobbyManagerOnLobbyJoined(TGPLobby lobby)
	{
		if (string.IsNullOrEmpty(txtName.text.Trim()))
		{
			txtName.text = "TGP Session 625";
		}
		lobby.SetName(txtName.text.Trim());
		lobby.SetVisibility((TGPLobby.LobbyVisibility)visDropdown.value);
		if (!string.IsNullOrEmpty(txtPassword.text))
		{
			lobby.SetPassword(txtPassword.text);
		}
		lobbyScene.OpenLobbyScreen();
		AbstractAttachedSingletonManager<RichPresenceManager>.Instance.InOwnLobby();
		base.gameObject.SetActive(value: false);
	}

	public void OnChange()
	{
		AbstractSingletonManager<LobbyManager>.Instance.CreateLobby();
	}

	public void OnDisable()
	{
		AbstractSingletonManager<LobbyManager>.Instance.OnLobbyJoined -= LobbyManagerOnLobbyJoined;
		txtName.text = "";
	}

	public void OnCancel()
	{
		base.gameObject.SetActive(value: false);
	}
}
