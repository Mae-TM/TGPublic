using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLook : MonoBehaviour
{
	public class SimpleStringEventArgs : EventArgs
	{
		public string type;

		public string value;
	}

	public Image HairU;

	public Image CoatBack;

	public Image LArm;

	public Image LSleeve;

	public Image LHand;

	public Image Body;

	public Image Shirt;

	public Image Symbol;

	public Image Shoes;

	public Image Pants;

	public Image ShoesOverlay;

	public Image PantsOverlay;

	public Image Coat;

	public Image RArm;

	public Image RSleeve;

	public Image RHand;

	public Image Head;

	public Image GlassesBack;

	public Image Mouth;

	public Image Eyes;

	public Image HairL;

	public Image Glasses;

	public Image Hat;

	public Material shiftedMat;

	public Material hairMat;

	private bool coatSleeves;

	private Sprite[] shirtSleeves;

	public Sprite humanHead;

	public Sprite humanLArm;

	public Sprite humanRArm;

	public Sprite humanLHand;

	public Sprite humanRHand;

	public Sprite humanBody;

	public Sprite robotHead;

	public Sprite robotLArm;

	public Sprite robotRArm;

	public Sprite robotLHand;

	public Sprite robotRHand;

	public Sprite robotBody;

	public event EventHandler<SimpleStringEventArgs> OnAssetError;

	public void MakeNewMaterials()
	{
		shiftedMat = new Material(shiftedMat);
		Symbol.material = shiftedMat;
		hairMat = new Material(hairMat);
		HairU.material = hairMat;
		HairL.material = hairMat;
	}

	private void DisableArmor(ArmorKind armorType)
	{
		switch (armorType)
		{
		case ArmorKind.Hat:
			Hat.enabled = false;
			HairU.enabled = HairU.sprite != null;
			break;
		case ArmorKind.Face:
			Glasses.enabled = false;
			GlassesBack.enabled = false;
			break;
		case ArmorKind.Shirt:
			CoatBack.enabled = false;
			Coat.enabled = false;
			coatSleeves = false;
			ChangeSleeves(shirtSleeves);
			break;
		case ArmorKind.Pants:
			Pants.enabled = false;
			PantsOverlay.enabled = false;
			break;
		case ArmorKind.Shoes:
			Shoes.enabled = false;
			ShoesOverlay.enabled = false;
			break;
		}
	}

	public void ChangeArmor(ArmorKind armortype, NormalItem item)
	{
		if (item == null || item.armor != armortype)
		{
			DisableArmor(armortype);
			return;
		}
		switch (armortype)
		{
		case ArmorKind.Hat:
			Hat.sprite = item.equipSprites[0][0];
			Hat.enabled = true;
			Hat.SetNativeSize();
			Hat.material = (IsHueShiftable(item) ? shiftedMat : null);
			HairU.enabled = !IsHelmet(item) && HairU.sprite != null;
			break;
		case ArmorKind.Face:
		{
			Sprite[] array3 = item.equipSprites[0];
			Glasses.sprite = array3[0];
			Glasses.enabled = true;
			Glasses.SetNativeSize();
			if (array3.Length > 7)
			{
				GlassesBack.sprite = array3[7];
				GlassesBack.enabled = true;
				GlassesBack.SetNativeSize();
			}
			else
			{
				GlassesBack.enabled = false;
			}
			if (IsHueShiftable(item))
			{
				Glasses.material = shiftedMat;
				GlassesBack.material = shiftedMat;
			}
			else
			{
				Glasses.material = null;
				GlassesBack.material = null;
			}
			break;
		}
		case ArmorKind.Shirt:
		{
			Coat.sprite = item.equipSprites[0][0];
			Coat.enabled = true;
			Coat.SetNativeSize();
			bool flag = IsHueShiftable(item);
			Coat.material = (flag ? shiftedMat : null);
			Material material = Coat.material;
			if (item.equipSprites[1].Length != 0)
			{
				CoatBack.sprite = item.equipSprites[1][0];
				CoatBack.enabled = true;
				CoatBack.SetNativeSize();
				CoatBack.material = material;
			}
			else
			{
				CoatBack.enabled = false;
			}
			Sprite[] array2 = item.equipSprites[2];
			if (array2.Length != 0)
			{
				ChangeSleeves(array2, flag);
				coatSleeves = true;
			}
			else
			{
				coatSleeves = false;
				ChangeSleeves(shirtSleeves);
			}
			break;
		}
		case ArmorKind.Pants:
			Pants.sprite = item.equipSprites[0][0];
			Pants.enabled = true;
			Pants.SetNativeSize();
			if (item.equipSprites[1].Length != 0)
			{
				PantsOverlay.sprite = item.equipSprites[1][0];
				PantsOverlay.enabled = true;
				PantsOverlay.SetNativeSize();
			}
			else
			{
				PantsOverlay.enabled = false;
			}
			if (IsHueShiftable(item))
			{
				Pants.material = shiftedMat;
				PantsOverlay.material = shiftedMat;
			}
			else
			{
				Pants.material = null;
				PantsOverlay.material = null;
			}
			break;
		case ArmorKind.Shoes:
		{
			Sprite[] array = item.equipSprites[0];
			Shoes.sprite = array[2];
			ShoesOverlay.sprite = array[0];
			Shoes.enabled = true;
			Shoes.SetNativeSize();
			ShoesOverlay.SetNativeSize();
			if (IsHueShiftable(item))
			{
				Shoes.material = shiftedMat;
				ShoesOverlay.material = shiftedMat;
			}
			else
			{
				Shoes.material = null;
				ShoesOverlay.material = null;
			}
			break;
		}
		}
	}

	internal void ChangeShirt(string shirt)
	{
		Shirt.sprite = ItemDownloader.Instance.shirtBundle.LoadAsset<Sprite>(shirt);
		if (Shirt.sprite == null)
		{
			shirt = "aaNone";
			Shirt.sprite = ItemDownloader.Instance.shirtBundle.LoadAsset<Sprite>(shirt);
		}
		if (IsHueShiftable(Shirt.sprite))
		{
			Shirt.material = shiftedMat;
		}
		else
		{
			Shirt.material = null;
		}
		if (!coatSleeves)
		{
			Sprite[] sleeves = ItemDownloader.GetSleeves(shirt);
			if (sleeves.Length == 0)
			{
				sleeves = ItemDownloader.GetSleeves("aanone");
			}
			ChangeSleeves(sleeves);
			shirtSleeves = sleeves;
		}
	}

	private void ChangeSleeves(Sprite[] sprites)
	{
		ChangeSleeves(sprites, sprites != null && IsHueShiftable(sprites[6]));
	}

	private void ChangeSleeves(Sprite[] sprites, bool hueShift)
	{
		if (sprites != null)
		{
			LSleeve.enabled = true;
			RSleeve.enabled = true;
			LSleeve.sprite = sprites[6];
			RSleeve.sprite = sprites[5];
			LSleeve.SetNativeSize();
			RSleeve.SetNativeSize();
			if (hueShift)
			{
				LSleeve.material = shiftedMat;
				RSleeve.material = shiftedMat;
			}
			else
			{
				LSleeve.material = null;
				RSleeve.material = null;
			}
		}
		else
		{
			LSleeve.enabled = false;
			RSleeve.enabled = false;
		}
	}

	public void ChangeHair(bool top, string name, HairHighlights highLights)
	{
		if (top)
		{
			HairU.sprite = ItemDownloader.GetHairUpperBundle(highLights).GetAssetWithFallback<Sprite>(name, "0", delegate
			{
				this.OnAssetError?.Invoke(this, new SimpleStringEventArgs
				{
					type = "hairUpper",
					value = name
				});
			});
			HairU.SetNativeSize();
		}
		else
		{
			HairL.sprite = ItemDownloader.GetHairLowerBundle(highLights).GetAssetWithFallback<Sprite>(name, "0", delegate
			{
				this.OnAssetError?.Invoke(this, new SimpleStringEventArgs
				{
					type = "hairLower",
					value = name
				});
			});
			HairL.SetNativeSize();
		}
	}

	public void EyesChange(string name)
	{
		Eyes.sprite = ItemDownloader.Instance.eyesBundle.LoadAsset<Sprite>(name);
		if (Eyes.sprite == null)
		{
			Eyes.sprite = ItemDownloader.Instance.eyesBundle.LoadAsset<Sprite>("aanone");
		}
	}

	public void MouthChange(string name)
	{
		Mouth.sprite = ItemDownloader.Instance.mouthBundle.LoadAsset<Sprite>(name);
		if (Mouth.sprite == null)
		{
			name = "aaNone";
			Mouth.sprite = ItemDownloader.Instance.mouthBundle.LoadAsset<Sprite>(name);
		}
		if (IsHueShiftable(Mouth.sprite))
		{
			Mouth.material = shiftedMat;
		}
		else
		{
			Mouth.material = null;
		}
	}

	public static bool IsHueShiftable(Sprite sprt)
	{
		bool result = false;
		Color[] pixels = sprt.texture.GetPixels((int)sprt.rect.x, (int)sprt.rect.y, (int)sprt.rect.width, (int)sprt.rect.height);
		for (int i = 0; i < pixels.Length; i++)
		{
			Color rgbColor = pixels[i];
			if (rgbColor.a >= 0f)
			{
				Color.RGBToHSV(rgbColor, out var H, out var S, out var V);
				if (H > 0.1f && H < 0.9f && S >= 0.5f && V >= 0.5f)
				{
					return false;
				}
				if ((H <= 0.1f || H >= 0.9f) && S >= 0.5f && V >= 0.5f)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static bool IsHueShiftable(NormalItem item)
	{
		return item.HasTag(NormalItem.Tag.Colored);
	}

	public static bool IsHelmet(NormalItem item)
	{
		return item.equipSprites[0][0].texture.name.EndsWith("helmet");
	}

	public void RobotChange(bool b)
	{
		if (b)
		{
			LArm.sprite = robotLArm;
			Body.sprite = robotBody;
			RArm.sprite = robotRArm;
			Head.sprite = robotHead;
			LHand.sprite = robotLHand;
			RHand.sprite = robotRHand;
		}
		else
		{
			LArm.sprite = humanLArm;
			Body.sprite = humanBody;
			RArm.sprite = humanRArm;
			Head.sprite = humanHead;
			LHand.sprite = humanLHand;
			RHand.sprite = humanRHand;
		}
	}

	public void SymbolChange(int id, byte[] customSymbol)
	{
		if (id != -1)
		{
			Sprite[] array = Resources.LoadAll<Sprite>("Player/symbol/symb");
			Symbol.sprite = array[id];
			Symbol.SetNativeSize();
			return;
		}
		Texture2D texture2D = new Texture2D(2, 2);
		if (texture2D.LoadImage(customSymbol))
		{
			Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			Symbol.sprite = sprite;
			Symbol.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 32f);
			Symbol.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 32f);
		}
	}

	public void HairColorChange(bool b)
	{
		hairMat.SetFloat("_Black", 1f - (b ? 1f : 0f));
	}

	public void LoadFromBuffer(PlayerSpriteData data)
	{
		CharacterSettings character = data.character;
		ChangeShirt(character.shirt);
		ChangeArmor(ArmorKind.Hat, data.hat);
		ChangeArmor(ArmorKind.Face, data.glasses);
		ChangeArmor(ArmorKind.Shirt, data.coat);
		ChangeArmor(ArmorKind.Pants, data.pants);
		ChangeArmor(ArmorKind.Shoes, data.shoes);
		ChangeHair(top: true, character.hairtop, character.hairHighlights);
		ChangeHair(top: false, character.hairbottom, character.hairHighlights);
		HairColorChange(character.whiteHair == 1f);
		ImageEffects.SetShiftColor(hairMat, character.color);
		ImageEffects.SetShiftColor(shiftedMat, character.color);
		EyesChange(character.eyes);
		MouthChange(character.mouth);
		SymbolChange(character.symbol, character.customSymbol);
		RobotChange(character.isRobot);
	}
}
