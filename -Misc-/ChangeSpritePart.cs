using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSpritePart : MonoBehaviour, ISaveLoadable
{
	public delegate void OnDoneEvent();

	private class CustomCharacterLoader : ISaveLoadable
	{
		private ChangeSpritePart spritepart;

		public CustomCharacterLoader(ChangeSpritePart sp)
		{
			spritepart = sp;
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
				if (texture2D.width > texture2D.height)
				{
					int num = Mathf.Min(32, texture2D.width);
					texture2D = ImageEffects.ResizeTexture(texture2D, num, num * texture2D.height / texture2D.width);
				}
				else
				{
					int num2 = Mathf.Min(32, texture2D.height);
					texture2D = ImageEffects.ResizeTexture(texture2D, num2 * texture2D.width / texture2D.height, num2);
				}
				Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
				spritepart.local_character.customSymbol = texture2D.EncodeToPNG();
				spritepart.local_character.symbol = -1;
				spritepart.Refresh();
			}
		}
	}

	public Toggle whiteTogg;

	public Toggle blackTogg;

	public ColorSelector colorSelector;

	public Toggle humanTogg;

	public Toggle robotTogg;

	public Toggle[] hairHighLightTogg;

	public CharacterLook CharacterLook;

	public CharacterOptionsGridComponent CharacterOptionsGrid;

	public Material modusMat;

	public SaveLoad saveLoad;

	private bool saving;

	public static CharacterSettings character = new CharacterSettings("aanone", "aanone", "aanone", "aanone", "aanone", HairHighlights.Pale, 0, 1f, default(Color), isRobot: false, loadSymbol: false);

	public static readonly string[] clothes = new string[5];

	public CharacterSettings local_character = new CharacterSettings("aanone", "aanone", "aanone", "aanone", "aanone", HairHighlights.Pale, 0, 1f, default(Color), isRobot: false, loadSymbol: false);

	private readonly string[] localClothes = new string[5];

	public OnDoneEvent OnDone;

	private CustomCharacterLoader customCharacterLoader;

	private void Awake()
	{
		reset();
		Refresh();
		customCharacterLoader = new CustomCharacterLoader(this);
	}

	public void reset()
	{
		LoadCharacterFromPrefs();
		colorSelector.Reset();
	}

	private void Start()
	{
		CharacterOptionsGrid.ClothesChange += CallbackClothesChange;
		CharacterOptionsGrid.HairUpperChange += CallbackHairUpperChange;
		CharacterOptionsGrid.ShirtChange += CallbackShirtChange;
		CharacterOptionsGrid.EyesChange += CallbackEyesChange;
		CharacterOptionsGrid.MouthChange += CallbackMouthChange;
		CharacterOptionsGrid.HairLowerChange += CallbackHairLowerChange;
		CharacterOptionsGrid.SymbolChange += SymbolChange;
		CharacterLook.OnAssetError += CallBackAssetError;
	}

	private void CallbackClothesChange(object sender, CharacterOptionsGridComponent.SimpleWWWItemEventArgs args)
	{
		localClothes[(int)args.kind] = args.code;
		Refresh();
	}

	private void CallbackHairLowerChange(object sender, CharacterOptionsGridComponent.SimpleStringEventArgs e)
	{
		local_character.hairbottom = e.data;
		Refresh();
	}

	private void CallbackMouthChange(object sender, CharacterOptionsGridComponent.SimpleStringEventArgs e)
	{
		local_character.mouth = e.data;
		Refresh();
	}

	private void CallbackEyesChange(object sender, CharacterOptionsGridComponent.SimpleStringEventArgs e)
	{
		local_character.eyes = e.data;
		Refresh();
	}

	private void CallbackHairUpperChange(object sender, CharacterOptionsGridComponent.SimpleStringEventArgs e)
	{
		local_character.hairtop = e.data;
		Refresh();
	}

	private void CallbackShirtChange(object sender, CharacterOptionsGridComponent.SimpleStringEventArgs e)
	{
		local_character.shirt = e.data;
		Refresh();
	}

	private void CallBackAssetError(object sender, CharacterLook.SimpleStringEventArgs e)
	{
		Debug.LogError("Could not find " + e.value + " which was a " + e.type);
		string type = e.type;
		if (type != null && type == "hairLower")
		{
			local_character.hairbottom = "0";
			Commit();
		}
		else
		{
			Debug.LogError("Unknown type " + e.type);
		}
	}

	private void Refresh()
	{
		for (ArmorKind armorKind = ArmorKind.Hat; armorKind < ArmorKind.Count; armorKind++)
		{
			CharacterLook.ChangeArmor(armorKind, localClothes[(int)armorKind]);
		}
		CharacterLook.ChangeHair(top: true, local_character.hairtop, local_character.hairHighlights);
		CharacterLook.ChangeHair(top: false, local_character.hairbottom, local_character.hairHighlights);
		CharacterLook.EyesChange(local_character.eyes);
		CharacterLook.MouthChange(local_character.mouth);
		CharacterLook.SymbolChange(local_character.symbol, local_character.customSymbol);
		CharacterLook.ChangeShirt(local_character.shirt);
		whiteTogg.isOn = local_character.whiteHair == 1f;
		humanTogg.isOn = !local_character.isRobot;
		hairHighLightTogg[(int)local_character.hairHighlights].isOn = true;
		CharacterOptionsGrid.SetHairHighLights(local_character.hairHighlights);
	}

	public void SymbolChange(object sender, CharacterOptionsGridComponent.SymbolChangeEventArgs e)
	{
		local_character.symbol = e.id;
		local_character.customSymbol = e.texture.EncodeToPNG();
		Refresh();
	}

	public void LoadCustomSymbol()
	{
		saveLoad.run(customCharacterLoader, "Load Custom Symbol", (string.IsNullOrEmpty(saveLoad.path) || saveLoad.path.IndexOf('.') != -1) ? (Application.streamingAssetsPath + Path.DirectorySeparatorChar) : saveLoad.path, "*.png", getDirectoryInsteadOfFile: false);
	}

	public void HairColorChangeWhite(bool b)
	{
		if (b)
		{
			local_character.whiteHair = 1f;
			CharacterLook.HairColorChange(b: true);
		}
	}

	public void HairColorChangeBlack(bool b)
	{
		if (b)
		{
			local_character.whiteHair = 0f;
			CharacterLook.HairColorChange(b: false);
		}
	}

	public void RobotOn(bool b)
	{
		if (b)
		{
			local_character.isRobot = true;
			CharacterLook.RobotChange(b: true);
		}
	}

	public void RobotOff(bool b)
	{
		if (b)
		{
			local_character.isRobot = false;
			CharacterLook.RobotChange(b: false);
		}
	}

	public void HairHighLightSet(int to)
	{
		if (hairHighLightTogg[to].isOn)
		{
			local_character.hairHighlights = (HairHighlights)to;
			Refresh();
		}
	}

	public void Done()
	{
		Commit();
		base.gameObject.transform.parent.gameObject.SetActive(value: false);
		OnDone?.Invoke();
	}

	public void Cancel()
	{
		base.gameObject.transform.parent.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		base.gameObject.transform.parent.gameObject.SetActive(value: true);
		reset();
		Refresh();
	}

	public void PickFile(string file)
	{
		file = Regex.Replace(file, "\\.[^\\\\/]*$", "") + ".bin";
		if (saving)
		{
			using (FileStream stream = File.OpenWrite(file))
			{
				HouseLoader.writeProtoBuf(stream, local_character);
				string[] array = localClothes;
				for (int i = 0; i < array.Length; i++)
				{
					HouseLoader.writeString(array[i] ?? string.Empty, stream);
				}
				return;
			}
		}
		using FileStream stream2 = File.OpenRead(file);
		local_character = HouseLoader.readProtoBuf<CharacterSettings>(stream2);
		for (int j = 0; j < localClothes.Length; j++)
		{
			localClothes[j] = HouseLoader.readString(stream2, 9);
		}
		Refresh();
	}

	public PlayerSpriteData SaveToBuffer()
	{
		PlayerSpriteData result = default(PlayerSpriteData);
		result.character = local_character;
		result.hat = localClothes[0] ?? string.Empty;
		result.glasses = localClothes[1] ?? string.Empty;
		result.coat = localClothes[2] ?? string.Empty;
		result.pants = localClothes[3] ?? string.Empty;
		result.shoes = localClothes[4] ?? string.Empty;
		return result;
	}

	public void LoadFromBuffer(PlayerSpriteData data)
	{
		local_character = data.character;
		localClothes[0] = data.hat;
		localClothes[1] = data.glasses;
		localClothes[2] = data.coat;
		localClothes[3] = data.pants;
		localClothes[4] = data.shoes;
		Refresh();
	}

	public void LoadCharacterFile()
	{
		saving = false;
		saveLoad.run(this, "Load Character", (string.IsNullOrEmpty(saveLoad.path) || saveLoad.path.IndexOf('.') != -1) ? (Application.streamingAssetsPath + Path.DirectorySeparatorChar) : saveLoad.path, "*.bin", getDirectoryInsteadOfFile: false);
	}

	public void SaveCharacterFile()
	{
		saving = true;
		saveLoad.run(this, "Save Character", (string.IsNullOrEmpty(saveLoad.path) || saveLoad.path.IndexOf('.') != -1) ? (Application.streamingAssetsPath + Path.DirectorySeparatorChar) : saveLoad.path, "*.bin", getDirectoryInsteadOfFile: false);
	}

	public void SetRandomCharacter()
	{
		string[] array = ItemDownloader.GetOptions(ItemDownloader.Instance.eyesBundle).ToArray();
		local_character.eyes = array[Random.Range(0, array.Length)];
		array = ItemDownloader.GetOptions(ItemDownloader.Instance.mouthBundle).ToArray();
		local_character.mouth = array[Random.Range(0, array.Length)];
		array = ItemDownloader.GetOptions(ItemDownloader.Instance.shirtBundle).ToArray();
		local_character.shirt = array[Random.Range(0, array.Length)];
		local_character.hairHighlights = (HairHighlights)Random.Range(0, 5);
		array = ItemDownloader.GetOptions(ItemDownloader.GetHairUpperBundle(local_character.hairHighlights)).ToArray();
		local_character.hairtop = array[Random.Range(0, array.Length)];
		array = ItemDownloader.GetOptions(ItemDownloader.GetHairLowerBundle(local_character.hairHighlights)).ToArray();
		local_character.hairbottom = array[Random.Range(0, array.Length)];
		local_character.symbol = Random.Range(0, CharacterOptionsGrid.symbolSprt.Length);
		local_character.whiteHair = Mathf.Round(Random.value);
		local_character.color.h = Random.Range(0f, 1f);
		local_character.color.s = Random.Range(0f, 1f);
		local_character.color.v = Random.Range(0f, 1f);
		for (ArmorKind armorKind = ArmorKind.Hat; armorKind < ArmorKind.Count; armorKind++)
		{
			localClothes[(int)armorKind] = CharacterOptionsGrid.GetRandomClothing(armorKind);
		}
		Refresh();
	}

	public void SetDefaultOnError()
	{
		local_character = new CharacterSettings("aanone", "aanone", "aanone", "aanone", "aanone", HairHighlights.Pale, 0, 1f, new PBColor(0f, 1f, 1f), isRobot: false, loadSymbol: true);
		for (int i = 0; i < localClothes.Length; i++)
		{
			localClothes[i] = "";
		}
	}

	private static void LoadCharacterBase(out CharacterSettings character, string[] clothes)
	{
		character = new CharacterSettings(PlayerPrefs.GetString("CharacterEyes", "aanone"), PlayerPrefs.GetString("CharacterMouth", "aanone"), PlayerPrefs.GetString("CharacterShirt", "aanone"), PlayerPrefs.GetString("CharacterHairUpper", "0"), PlayerPrefs.GetString("CharacterHairLower", "0"), (HairHighlights)PlayerPrefs.GetInt("CharacterHairHighlights", 0), PlayerPrefs.GetInt("CharacterSymbol", 0), PlayerPrefs.GetFloat("CharacterWhiteHair", 1f), new PBColor(PlayerPrefs.GetFloat("CharacterColorH", 0f), PlayerPrefs.GetFloat("CharacterColorS", 1f), PlayerPrefs.GetFloat("CharacterColorV", 1f)), PlayerPrefs.GetInt("CharacterRobot", 0) == 1, loadSymbol: true);
		clothes[0] = PlayerPrefs.GetString("CharacterHat", "");
		clothes[1] = PlayerPrefs.GetString("CharacterGlasses", "");
		clothes[2] = PlayerPrefs.GetString("CharacterCoat", "");
		clothes[3] = PlayerPrefs.GetString("CharacterPants", "");
		clothes[4] = PlayerPrefs.GetString("CharacterShoes", "");
	}

	public static void LoadCharacter(string eyes, string mouth, string shirt, string hairTop, string hairBottom, HairHighlights highlights, int symbol, float whiteHair, PBColor color, bool isRobot, string hat, string glasses, string coat, string pants, string shoes)
	{
		character = new CharacterSettings(eyes, mouth, shirt, hairTop, hairBottom, highlights, symbol, whiteHair, color, isRobot, loadSymbol: true);
		clothes[0] = hat;
		clothes[1] = glasses;
		clothes[2] = coat;
		clothes[3] = pants;
		clothes[4] = shoes;
	}

	public static CharacterSettings LoadCharacterStatic()
	{
		LoadCharacterBase(out character, clothes);
		return character;
	}

	public void LoadCharacterFromPrefs()
	{
		LoadCharacterBase(out local_character, localClothes);
	}

	private void Commit()
	{
		PlayerPrefs.SetString("CharacterMouth", local_character.mouth);
		PlayerPrefs.SetString("CharacterHairUpper", local_character.hairtop);
		PlayerPrefs.SetString("CharacterHairLower", local_character.hairbottom);
		PlayerPrefs.SetString("CharacterEyes", local_character.eyes);
		PlayerPrefs.SetString("CharacterHat", localClothes[0]);
		PlayerPrefs.SetString("CharacterGlasses", localClothes[1]);
		PlayerPrefs.SetInt("CharacterSymbol", local_character.symbol);
		PlayerPrefs.SetInt("CharacterHairHighlights", (int)local_character.hairHighlights);
		PlayerPrefs.SetFloat("CharacterWhiteHair", local_character.whiteHair);
		PlayerPrefs.SetString("CharacterCoat", localClothes[2]);
		PlayerPrefs.SetString("CharacterShirt", local_character.shirt);
		PlayerPrefs.SetString("CharacterPants", localClothes[3]);
		PlayerPrefs.SetString("CharacterShoes", localClothes[4]);
		PlayerPrefs.SetFloat("CharacterColorH", local_character.color.h);
		PlayerPrefs.SetFloat("CharacterColorS", local_character.color.s);
		PlayerPrefs.SetFloat("CharacterColorV", local_character.color.v);
		PlayerPrefs.SetInt("CharacterRobot", local_character.isRobot ? 1 : 0);
		PlayerPrefs.SetString("House", HouseBuilder.saveAs);
		if (local_character.symbol == -1)
		{
			File.WriteAllBytes(Application.streamingAssetsPath + "/symbol.png", local_character.customSymbol);
		}
	}
}
