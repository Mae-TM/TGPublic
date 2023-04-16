using UnityEngine;
using UnityEngine.UI;

public class ModusPickerComponent : MonoBehaviour
{
	[SerializeField]
	private ModusPicker modusPicker;

	[SerializeField]
	private string[] options;

	private Image image;

	public static string Modus => PlayerPrefs.GetString("Modus", "Queue");

	private void Awake()
	{
		image = GetComponent<Image>();
		if (!(modusPicker == null))
		{
			string[] array = options;
			foreach (string modus in array)
			{
				modusPicker.AddModus(modus);
			}
			options = null;
			modusPicker.OnPickModus += delegate(string to)
			{
				PlayerPrefs.SetString("Modus", to);
			};
		}
	}

	public void ModusChange(string to)
	{
		image.sprite = Resources.Load<Sprite>("Modi/" + to + "Modus");
	}
}
