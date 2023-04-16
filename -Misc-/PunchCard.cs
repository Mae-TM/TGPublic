using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PunchCard : Item
{
	private static readonly char[] chars = new char[64]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
		'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
		'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
		'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
		'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
		'y', 'z', '?', '!'
	};

	private static Sprite punchSprite;

	private NormalItem resultItem;

	private NormalItem origItem;

	private StringBuilder code;

	private bool[] bits;

	private bool alchemised;

	public override bool IsEntry => resultItem.IsEntry;

	protected override string Prefab => "Card";

	public string Code
	{
		get
		{
			return code.ToString();
		}
		set
		{
			if (code == null || code.ToString() != value)
			{
				if (code == null)
				{
					code = new StringBuilder(value);
				}
				else
				{
					code.Clear();
					code.Append(value);
				}
				bits = GetBits(value);
			}
		}
	}

	public override string GetItemName()
	{
		return "Punched " + origItem.name;
	}

	public PunchCard(Item result, NormalItem original = null)
		: base(ItemType.Punched)
	{
		resultItem = GetPunchItem(result);
		origItem = original ?? resultItem;
		Code = resultItem.captchaCode;
		base.weight = 0.1f;
		base.sprite = origItem.sprite;
		base.description = "Insert this into the totem lathe to carve a totem.";
		if (punchSprite == null)
		{
			punchSprite = Resources.Load<Sprite>("Punchhole");
		}
	}

	public PunchCard(PunchCard card)
		: base(card)
	{
		bits = card.bits;
		code = card.code;
		resultItem = card.resultItem;
		origItem = card.origItem;
	}

	public override Item Copy()
	{
		return new PunchCard(this);
	}

	public override void Destroy()
	{
		resultItem?.Destroy();
		base.Destroy();
	}

	protected override void FillItemObject()
	{
		base.FillItemObject();
		if (Player.player != null)
		{
			Material material = Player.player.sylladex.modusSettings.GetComponent<Image>().material;
			base.SceneObject.GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(material.GetFloat("_HueShift") / 360f, material.GetFloat("_Sat"), material.GetFloat("_Val"));
		}
		Transform child = base.SceneObject.transform.GetChild(0);
		ApplyToCard(child.GetComponent<SpriteRenderer>());
		base.SceneObject.SetActive(value: false);
	}

	public override void ApplyToImage(Image image)
	{
		origItem.ApplyToImage(image);
		Rect rect = image.rectTransform.rect;
		rect.x += 0.05f * rect.width;
		rect.y += 1f / 14f * rect.height;
		int num = 0;
		int num2 = 0;
		Image image2 = new GameObject("Punch hole").AddComponent<Image>();
		image2.sprite = punchSprite;
		image2.rectTransform.pivot = new Vector2(0f, 1f);
		image2.rectTransform.anchorMin = new Vector2(0f, 1f);
		image2.rectTransform.anchorMax = new Vector2(0f, 1f);
		image2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0.175f * rect.width);
		image2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 3f / 56f * rect.height);
		image2.rectTransform.Translate(rect.x, 0f - rect.y, 0f);
		bool[] array = bits;
		foreach (bool num3 in array)
		{
			if (num2 >= 12)
			{
				num2 -= 12;
				num++;
			}
			if (num3)
			{
				UnityEngine.Object.Instantiate(image2, image.transform).rectTransform.Translate((float)num * rect.width * 8f / 40f, (float)(-num2) * rect.height * 4f / 56f, 0f);
			}
			num2++;
		}
		UnityEngine.Object.Destroy(image2.gameObject);
	}

	public override void ApplyToCard(SpriteRenderer card)
	{
		origItem.ApplyToCard(card);
	}

	public override void ClearCard(SpriteRenderer card)
	{
		origItem.ClearCard(card);
	}

	private bool[] GetBinary(char source)
	{
		bool[] array = new bool[6];
		for (int i = 0; i < chars.Length; i++)
		{
			if (source == chars[i])
			{
				bool[] array2 = (from s in Convert.ToString(i, 2)
					select s.Equals('1')).ToArray();
				array2.CopyTo(array, 6 - array2.Length);
				break;
			}
		}
		return array;
	}

	private bool[] GetBits(string value)
	{
		bool[] array = new bool[value.Length * 6];
		for (int i = 0; i < value.Length; i++)
		{
			bool[] binary = GetBinary(value[i]);
			for (int j = 0; j < 6; j++)
			{
				array[i * 6 + j] = binary[j];
			}
		}
		return array;
	}

	private void SetBit(int index, bool value)
	{
		bits[index] = value;
		int num = index / 6;
		bool[] destinationArray = new bool[6];
		Array.Copy(bits, num * 6, destinationArray, 0, 6);
		code[num] = chars[BitsAsInt(destinationArray)];
	}

	private void SetBits(bool[] value)
	{
		bits = value;
		code = BitsToCode(value);
	}

	private static StringBuilder BitsToCode(bool[] bits)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < bits.Length / 6; i++)
		{
			bool[] destinationArray = new bool[6];
			Array.Copy(bits, i * 6, destinationArray, 0, 6);
			stringBuilder.Append(chars[BitsAsInt(destinationArray)]);
		}
		return stringBuilder;
	}

	public static int BitsAsInt(bool[] bits)
	{
		if (bits.Length != 6)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (bits[i])
			{
				num |= 1 << 5 - i;
			}
		}
		return num;
	}

	public static IEnumerator<AsyncOperation> AND(PunchCard a, PunchCard b, Action<NormalItem> done)
	{
		if (a.resultItem.itemType != ItemType.Custom && b.resultItem.itemType != ItemType.Custom && a.code.ToString() == b.code.ToString())
		{
			done(a.resultItem);
			yield break;
		}
		IEnumerator<AsyncOperation> www = ItemDownloader.Instance.GetRecipeAsync(a.resultItem, b.resultItem, "&&", (string recipe) => ANDcont(a, b, done, recipe));
		while (www.MoveNext())
		{
			yield return www.Current;
		}
	}

	private static IEnumerator<AsyncOperation> ANDcont(PunchCard a, PunchCard b, Action<NormalItem> done, string result = null)
	{
		if (!string.IsNullOrEmpty(result))
		{
			IEnumerator<AsyncOperation> www = ItemDownloader.Instance.Downloaditem(result, done);
			while (www.MoveNext())
			{
				yield return www.Current;
			}
			yield break;
		}
		NormalItem normalItem = a.resultItem & b.resultItem;
		int num = Math.Min(a.bits.Length, b.bits.Length);
		bool[] array = new bool[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = a.bits[i] && b.bits[i];
		}
		normalItem.captchaCode = BitsToCode(array).ToString();
		done(normalItem);
	}

	private static NormalItem GetPunchItem(Item item)
	{
		if (!(item is PunchCard punchCard))
		{
			if (item is NormalItem result)
			{
				return result;
			}
			throw new InvalidOperationException();
		}
		return punchCard.origItem;
	}

	public void OR(Item otherBase)
	{
		NormalItem punchItem = GetPunchItem(otherBase);
		if (IsUnalchemisable(punchItem))
		{
			return;
		}
		string text = AbstractSingletonManager<DatabaseManager>.Instance.GetRecipeResult(resultItem, punchItem, "||")?.Code;
		if (!string.IsNullOrEmpty(text))
		{
			resultItem?.Destroy();
			resultItem = new NormalItem(text);
			Code = text;
			alchemised = true;
			return;
		}
		resultItem.Destroy();
		resultItem |= punchItem;
		bool[] array = GetBits(punchItem.captchaCode);
		int num = Math.Min(bits.Length, array.Length);
		for (int i = 0; i < num; i++)
		{
			bits[i] = bits[i] || array[i];
		}
		code = BitsToCode(bits);
		resultItem.captchaCode = code.ToString();
		alchemised = true;
	}

	public NormalItem GetItem()
	{
		return resultItem;
	}

	public bool IsUnalchemisable(Item otherBase)
	{
		if (alchemised)
		{
			return true;
		}
		if (resultItem.itemType == ItemType.Custom)
		{
			return false;
		}
		NormalItem punchItem = GetPunchItem(otherBase);
		if (punchItem.itemType != ItemType.Custom)
		{
			return Code == punchItem.captchaCode;
		}
		return false;
	}

	public PunchCard(HouseData.PunchCard data)
		: this((NormalItem)Item.Load(data.result), (NormalItem)Item.Load(data.original))
	{
	}

	public override HouseData.Item Save()
	{
		return new HouseData.PunchCard
		{
			result = resultItem.Save(),
			original = origItem.Save()
		};
	}
}
