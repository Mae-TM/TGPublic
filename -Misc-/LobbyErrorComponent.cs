using UnityEngine;
using UnityEngine.UI;

public class LobbyErrorComponent : MonoBehaviour
{
	[SerializeField]
	private Text TxtError;

	public void Error(string error)
	{
		base.gameObject.SetActive(value: true);
		TxtError.text = error;
	}
}
