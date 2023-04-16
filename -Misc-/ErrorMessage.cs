using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour
{
	[SerializeField]
	private Text title;

	[SerializeField]
	private Text content;

	public void ShowMessage(string title, string content)
	{
		this.title.text = title;
		this.content.text = content;
		base.gameObject.SetActive(value: true);
	}
}
