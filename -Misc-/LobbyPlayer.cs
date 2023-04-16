using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour
{
	public GameObject parent;

	public CharacterLook characterLook;

	public ModusPickerComponent modusPicker;

	public HousePickerComponent housePicker;

	public Text TxtUsername;

	public Text TxtClasspect;

	public Button kickButton;

	public Button ownerButton;

	public void SetLobbyOwner(bool isLocalPlayer, bool isThisPlayer)
	{
		kickButton.interactable = isLocalPlayer;
		ownerButton.interactable = !isThisPlayer;
		ownerButton.gameObject.SetActive(isLocalPlayer || isThisPlayer);
	}

	public void SetReadOnly(bool to)
	{
		characterLook.transform.parent.parent.gameObject.SetActive(to);
		housePicker.gameObject.SetActive(to);
		modusPicker.gameObject.SetActive(to);
		TxtClasspect.gameObject.SetActive(to);
	}
}
