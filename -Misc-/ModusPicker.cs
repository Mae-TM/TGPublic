using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModusPicker : MonoBehaviour
{
	private static Dictionary<string, string> modusDescription;

	[SerializeField]
	private Image modusOption;

	public event Action<string> OnPickModus;

	private static void ReadDescriptions()
	{
		if (modusDescription != null)
		{
			return;
		}
		modusDescription = new Dictionary<string, string>();
		foreach (string item in StreamingAssets.ReadLines("fetchmodi.txt"))
		{
			string[] array = item.Split(new char[1] { ':' }, 2);
			modusDescription[array[0]] = array[1].TrimStart();
		}
	}

	public void AddModus(string modus)
	{
		ReadDescriptions();
		Image image = ((modusOption.sprite == null) ? modusOption : UnityEngine.Object.Instantiate(modusOption, modusOption.transform.parent));
		image.sprite = Resources.Load<Sprite>("Modi/" + modus + "Modus");
		image.name = modus + "Modus";
		Button.ButtonClickedEvent onClick = image.GetComponent<Button>().onClick;
		onClick.RemoveAllListeners();
		onClick.AddListener(delegate
		{
			this.OnPickModus(modus);
		});
		image.transform.GetChild(0).GetComponent<Text>().text = modus;
		if (modusDescription.TryGetValue(modus, out var value))
		{
			image.transform.GetChild(1).GetChild(0).GetComponent<Text>()
				.text = value;
		}
		else
		{
			image.transform.GetChild(1).GetChild(0).gameObject.SetActive(value: false);
		}
	}
}
