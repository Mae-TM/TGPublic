using UnityEngine;
using UnityEngine.UI;

public class ChatMessageListComponent : MonoBehaviour
{
	public GameObject Template;

	public Color Color1 = new Color(1f, 1f, 1f);

	public Color Color2 = new Color(0f, 0f, 0f);

	private GameObject _lastMessage;

	private void Start()
	{
		Template.SetActive(value: false);
	}

	public void ResetChat()
	{
		foreach (Transform item in base.transform)
		{
			if (item.gameObject != Template)
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private void AddNewMessage(string username, string message)
	{
		GameObject gameObject = Object.Instantiate(Template, base.transform, worldPositionStays: true);
		gameObject.SetActive(value: true);
		ChatMessageComponent component = gameObject.GetComponent<ChatMessageComponent>();
		component.Username.text = username;
		component.Text.text = message;
		component.Background.color = ((_lastMessage == null || _lastMessage.GetComponent<ChatMessageComponent>().Background.color == Color2) ? Color1 : Color2);
		_lastMessage = gameObject;
	}

	public void AddMessage(string username, string message)
	{
		if (_lastMessage == null)
		{
			AddNewMessage(username, message);
			return;
		}
		ChatMessageComponent component = _lastMessage.GetComponent<ChatMessageComponent>();
		if (component.Username.text == username)
		{
			Text text = component.Text;
			text.text = text.text + "\n" + message;
		}
		else
		{
			AddNewMessage(username, message);
		}
	}
}
