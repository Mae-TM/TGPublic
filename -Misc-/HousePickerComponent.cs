using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HousePickerComponent : MonoBehaviour
{
	public delegate void OnDoneEvent(string house);

	[SerializeField]
	private Toggle houseOption;

	[SerializeField]
	private RawImage housePreview;

	private readonly List<string> houses = new List<string>();

	public OnDoneEvent OnDone;

	private void Awake()
	{
		if (houseOption == null)
		{
			return;
		}
		string @string = PlayerPrefs.GetString("House", null);
		bool flag = false;
		foreach (FileInfo directoryContent in StreamingAssets.GetDirectoryContents("Houses", "*.b??"))
		{
			Toggle toggle = Object.Instantiate(houseOption, houseOption.transform.parent, worldPositionStays: false);
			Text component = toggle.transform.GetChild(2).GetComponent<Text>();
			string houseName = directoryContent.Name;
			if (directoryContent.Extension == ".bin")
			{
				using FileStream input = directoryContent.OpenRead();
				using BinaryReader binaryReader = new BinaryReader(input);
				int num = binaryReader.ReadInt32();
				toggle.interactable = num == 9;
			}
			if (toggle.interactable)
			{
				houses.Add(houseName);
				toggle.onValueChanged.AddListener(delegate(bool to)
				{
					if (to)
					{
						SetLocalHouse(houseName);
					}
				});
				component.text = GetName(houseName);
			}
			else
			{
				component.text = "<color=red>" + GetName(houseName) + "</color>";
			}
			if (houseName == @string || directoryContent.FullName == @string)
			{
				toggle.isOn = true;
				flag = true;
			}
		}
		houseOption.onValueChanged.AddListener(delegate(bool to)
		{
			if (to)
			{
				SetLocalHouse(null);
			}
		});
		if (!flag)
		{
			houseOption.isOn = true;
		}
	}

	private static string GetName(string house)
	{
		return house.Substring(0, house.Length - 4);
	}

	private void SetLocalHouse(string to)
	{
		HouseBuilder.saveAs = to ?? houses[Random.Range(0, houses.Count)];
		OnDone?.Invoke(to);
	}

	public void HouseChange(string to)
	{
		string path;
		if (string.IsNullOrEmpty(to))
		{
			housePreview.enabled = false;
			housePreview.transform.parent.GetChild(1).gameObject.SetActive(value: true);
			housePreview.transform.parent.GetChild(1).GetComponent<Text>().text = "Random House";
		}
		else if (StreamingAssets.TryGetFile("Houses/" + GetName(to) + ".png", out path))
		{
			byte[] data = File.ReadAllBytes(path);
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
			housePreview.enabled = true;
			housePreview.texture = texture2D;
			housePreview.transform.parent.GetChild(1).gameObject.SetActive(value: false);
		}
		else
		{
			housePreview.enabled = false;
			housePreview.transform.parent.GetChild(1).gameObject.SetActive(value: true);
			housePreview.transform.parent.GetChild(1).GetComponent<Text>().text = GetName(to);
		}
	}
}
