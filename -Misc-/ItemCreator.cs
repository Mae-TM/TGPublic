using System.IO;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.UI;

public class ItemCreator : MonoBehaviour, ISaveLoadable
{
	[SerializeField]
	private SaveLoad saveLoad;

	[SerializeField]
	private RawImage image;

	[SerializeField]
	private InputField nameField;

	[SerializeField]
	private InputField codeField;

	[SerializeField]
	private InputField gristField;

	[SerializeField]
	private InputField speedField;

	[SerializeField]
	private Dropdown kindDropDown;

	private void Start()
	{
		for (WeaponKind weaponKind = WeaponKind.None; weaponKind < WeaponKind.Count; weaponKind++)
		{
			kindDropDown.options.Add(new Dropdown.OptionData(weaponKind.ToString(), Resources.Load<Sprite>("WeaponKind/" + weaponKind)));
		}
		kindDropDown.value = 0;
		kindDropDown.RefreshShownValue();
	}

	public void Cancel()
	{
	}

	public void PickFile(string file)
	{
		byte[] data = File.ReadAllBytes(file);
		Texture2D texture2D = new Texture2D(2, 2);
		if (texture2D.LoadImage(data))
		{
			image.texture = texture2D;
			texture2D.name = Path.GetFileNameWithoutExtension(file);
		}
	}

	public void PickSprite()
	{
		saveLoad.run(this, "Load Image", (string.IsNullOrEmpty(saveLoad.path) || saveLoad.path.IndexOf('.') != -1) ? (Application.streamingAssetsPath + Path.DirectorySeparatorChar) : saveLoad.path, "*.png", getDirectoryInsteadOfFile: false);
	}

	public void KindChanged(int to)
	{
		if (to == 0)
		{
			speedField.transform.parent.gameObject.SetActive(value: false);
			gristField.transform.parent.GetComponent<Text>().text = "Grist Cost:";
		}
		else
		{
			speedField.transform.parent.gameObject.SetActive(value: true);
			gristField.transform.parent.GetComponent<Text>().text = "Damage:";
		}
	}

	public void SaveButton()
	{
		codeField.text = codeField.text.PadRight(8, '0');
		if (gristField.text == "")
		{
			gristField.text = "0";
		}
		if (speedField.text == "")
		{
			speedField.text = "1";
		}
		Directory.CreateDirectory(Application.streamingAssetsPath + "/CustomItems");
		File.WriteAllBytes(Application.streamingAssetsPath + "/CustomItems/" + image.texture.name + ".png", ((Texture2D)image.texture).EncodeToPNG());
		LDBItem item = new LDBItem
		{
			Code = codeField.text,
			Name = nameField.text,
			Icon = image.texture.name,
			Grist = int.Parse(gristField.text),
			Strifekind = ((WeaponKind)kindDropDown.value).ToString(),
			Weaponsprite = image.texture.name,
			Custom = true,
			Speed = int.Parse(speedField.text)
		};
		AbstractSingletonManager<DatabaseManager>.Instance.CreateItem("inGameCreated", item);
	}
}
