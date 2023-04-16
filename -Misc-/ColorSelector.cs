using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Text shortName;

	public InputField nameInput;

	public Image text;

	public Image cruxite;

	public Slider hue;

	public Slider sat;

	public Slider val;

	public Material shiftedMat;

	public Material hueMat;

	public Material satMat;

	public Material valMat;

	public Material hairMat;

	public Material cruxMat;

	public ChangeSpritePart changesprite;

	private static Color[] validColor;

	private static int width;

	private void Start()
	{
	}

	public void Reset()
	{
		ReadValidColors();
		SetSliders();
		nameInput.text = SteamClient.Name;
		nameInput.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
	}

	private void SetColor(Color color)
	{
		uint color2 = ClosestValidColor(color);
		SetColor(color2);
		shortName.color = GetTypingColor(color);
		Color.RGBToHSV(GetCruxiteColor(color), out var H, out var S, out var V);
		cruxMat.SetFloat("_HueShift", H * 360f);
		cruxMat.SetFloat("_Sat", S);
		cruxMat.SetFloat("_Val", V);
	}

	private void SetColor(uint index)
	{
		uint num = ((index % 2u == 0) ? (index + 1) : (index - 1));
		text.transform.localPosition = new Vector2((float)((long)index % (long)width) * (base.transform as RectTransform).sizeDelta.x / (float)width, ((float)((long)index / (long)width) - 0.5f) * (base.transform as RectTransform).sizeDelta.y / 3f);
		cruxite.transform.localPosition = new Vector2((float)((long)num % (long)width) * (base.transform as RectTransform).sizeDelta.x / (float)width, ((float)((long)num / (long)width) - 0.5f) * (base.transform as RectTransform).sizeDelta.y / 3f);
	}

	public void OnPointerClick(PointerEventData data)
	{
		Vector2 vector = (base.transform as RectTransform).InverseTransformPoint(data.position);
		int num = Mathf.FloorToInt(vector.x / (base.transform as RectTransform).sizeDelta.x * (float)width) + Mathf.FloorToInt(1.5f + vector.y / (base.transform as RectTransform).sizeDelta.y * 3f) * width;
		SetColor((uint)num);
		changesprite.local_character.color = validColor[num];
		SetSliders();
	}

	public void SetSliders()
	{
		float h = changesprite.local_character.color.h;
		float s = changesprite.local_character.color.s;
		float v = changesprite.local_character.color.v;
		hue.value = h;
		sat.value = s;
		val.value = v;
	}

	public void ChangeName(string to)
	{
		MultiplayerSettings.playerName = to;
		PlayerPrefs.SetString("PesterchumName", to);
		shortName.text = Pesterlog.GetShortName(to);
	}

	public void HueChange(float n)
	{
		shiftedMat.SetFloat("_HueShift", n * 360f);
		satMat.SetFloat("_HueShift", n * 360f);
		valMat.SetFloat("_HueShift", n * 360f);
		hairMat.SetFloat("_HueShift", n * 360f);
		changesprite.local_character.color.h = n;
		SetColor(changesprite.local_character.color);
	}

	public void SaturationChange(float n)
	{
		shiftedMat.SetFloat("_Sat", n);
		hueMat.SetFloat("_Sat", n);
		valMat.SetFloat("_Sat", n);
		hairMat.SetFloat("_Sat", n);
		changesprite.local_character.color.s = n;
		SetColor(changesprite.local_character.color);
	}

	public void ValueChange(float n)
	{
		shiftedMat.SetFloat("_Val", n);
		hueMat.SetFloat("_Val", n);
		satMat.SetFloat("_Val", n);
		hairMat.SetFloat("_Val", n);
		changesprite.local_character.color.v = n;
		SetColor(changesprite.local_character.color);
	}

	public static Color GetCruxiteColor(Color color)
	{
		uint num = ClosestValidColor(color);
		if (num % 2u != 0)
		{
			return validColor[num - 1];
		}
		return validColor[num + 1];
	}

	public static Color GetTypingColor(Color color)
	{
		uint num = ClosestValidColor(color);
		return validColor[num];
	}

	private static uint ClosestValidColor(Color color)
	{
		if (validColor == null)
		{
			ReadValidColors();
		}
		float num = float.PositiveInfinity;
		uint result = 0u;
		for (uint num2 = 0u; num2 < validColor.Length; num2++)
		{
			float num3 = Mathf.Pow(validColor[num2].r - color.r, 2f) + Mathf.Pow(validColor[num2].g - color.g, 2f) + Mathf.Pow(validColor[num2].b - color.b, 2f);
			if (num3 < num)
			{
				num = num3;
				result = num2;
			}
		}
		return result;
	}

	private static void ReadValidColors()
	{
		Queue<Color> queue = new Queue<Color>();
		Color[] pixels = Resources.Load<Texture2D>("Player/colourpalette").GetPixels();
		foreach (Color item in pixels)
		{
			if (!queue.Contains(item))
			{
				queue.Enqueue(item);
			}
		}
		validColor = queue.ToArray();
		width = validColor.Length / 3;
	}
}
