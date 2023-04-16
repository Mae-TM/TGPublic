using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyNameChangeDialog : MonoBehaviour
{
	public Button btnCancel;

	public Button btnChange;

	public InputField txtName;

	[FormerlySerializedAs("Lobby")]
	public LobbySceneComponent lobbyScene;

	public void OnNameClick()
	{
		if (base.enabled)
		{
			txtName.text = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.Name;
			base.gameObject.SetActive(value: true);
		}
	}

	public void SetStatus(bool to)
	{
		base.enabled = to;
	}

	public void OnChange()
	{
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetName(txtName.text);
		base.gameObject.SetActive(value: false);
	}

	public void OnCancel()
	{
		base.gameObject.SetActive(value: false);
	}
}
