using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
	private static Tooltip instance;

	public Text text;

	private void Awake()
	{
		instance = this;
		Hide();
	}

	private void LateUpdate()
	{
		base.transform.position = Input.mousePosition - new Vector3(0f, 16f, 0f);
	}

	public static void Show(string text)
	{
		if (instance.text.text != text)
		{
			instance.text.text = text;
		}
		instance.gameObject.SetActive(value: true);
	}

	public static void Hide()
	{
		instance.gameObject.SetActive(value: false);
	}
}
