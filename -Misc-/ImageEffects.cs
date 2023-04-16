using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class ImageEffects
{
	public class FadingShadow : MonoBehaviour
	{
		public SpriteRenderer[] sprites;

		public float delta;

		public Color color;

		private void Update()
		{
			color.a -= delta * Time.deltaTime;
			if (color.a <= 0f)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			SpriteRenderer[] array = sprites;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (!spriteRenderer.Equals(null))
				{
					spriteRenderer.color = color;
				}
			}
		}
	}

	private static Shader hsv;

	private static Shader hsvwb;

	public static Texture2D ResizeTexture(Texture2D pSource, int xWidth, int xHeight)
	{
		Color[] pixels = pSource.GetPixels(0);
		Vector2 vector = new Vector2(pSource.width, pSource.height);
		int num = xWidth * xHeight;
		Color[] array = new Color[num];
		Vector2 vector2 = default(Vector2);
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i % (float)xWidth;
			float num3 = Mathf.Floor((float)i / (float)xWidth);
			vector2.x = num2 / (float)xWidth * vector.x;
			vector2.y = num3 / (float)xHeight * vector.y;
			vector2.x = Mathf.Round(vector2.x);
			vector2.y = Mathf.Round(vector2.y);
			int num4 = (int)(vector2.y * vector.x + vector2.x);
			array[i] = pixels[num4];
		}
		Texture2D texture2D = new Texture2D(xWidth, xHeight, TextureFormat.RGBA32, mipChain: false);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D Recolor(Texture2D image, Color color, int x = 0, int y = 0, int blockWidth = 0, int blockHeight = 0)
	{
		Color[] array = ((blockWidth == 0) ? image.GetPixels() : image.GetPixels(x, y, blockWidth, blockHeight));
		for (uint num = 0u; num < array.Length; num++)
		{
			array[num] *= color;
		}
		if (blockWidth == 0)
		{
			image.SetPixels(array);
		}
		else
		{
			image.SetPixels(x, y, blockWidth, blockHeight, array);
		}
		return image;
	}

	public static Texture2D Overlay(Texture2D image, Texture2D overlay, int x = 0, int y = 0, int blockWidth = 0, int blockHeight = 0)
	{
		if (blockWidth == 0)
		{
			blockWidth = image.width;
		}
		if (blockHeight == 0)
		{
			blockHeight = image.height;
		}
		float num = Mathf.Min((float)overlay.width / (float)blockWidth, 2f);
		float num2 = Mathf.Min((float)overlay.height / (float)blockHeight, 2f);
		int x2 = Mathf.FloorToInt((float)overlay.width / 2f - num * (float)blockWidth / 2f);
		int y2 = Mathf.FloorToInt((float)overlay.height / 2f - num2 * (float)blockHeight / 2f);
		Color[] pixels = image.GetPixels(x, y, blockWidth, blockHeight);
		Color[] pixels2 = overlay.GetPixels(x2, y2, Mathf.CeilToInt(num * (float)blockWidth), Mathf.CeilToInt(num2 * (float)blockHeight));
		for (int i = 0; i < blockWidth; i++)
		{
			for (int j = 0; j < blockHeight; j++)
			{
				Color color = pixels[i + blockWidth * j];
				if (color.a > 0.5f && color != Color.black)
				{
					Color color2 = pixels2[Mathf.RoundToInt((float)i * num) + blockWidth * Mathf.RoundToInt((float)j * num2)];
					if (color2.a > 0.5f)
					{
						pixels[i + blockWidth * j] = color2;
					}
				}
			}
		}
		image.SetPixels(x, y, blockWidth, blockHeight, pixels);
		return image;
	}

	public static Texture2D Glitchify(Texture2D image, int x = 0, int y = 0, int blockWidth = 0, int blockHeight = 0)
	{
		Color[] array = ((blockWidth == 0) ? image.GetPixels() : image.GetPixels(x, y, blockWidth, blockHeight));
		for (uint num = 0u; num < array.Length; num++)
		{
			if (Random.Range(0, (int)(15f - array[num].a * 10f)) == 0)
			{
				array[num] = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);
			}
		}
		if (blockWidth == 0)
		{
			image.SetPixels(array);
		}
		else
		{
			image.SetPixels(x, y, blockWidth, blockHeight, array);
		}
		return image;
	}

	public static Texture2D ShiftHue(Color color, Texture2D inTex)
	{
		Texture2D texture2D = new Texture2D(inTex.width, inTex.height)
		{
			name = inTex.name,
			filterMode = inTex.filterMode
		};
		Color[] pixels = inTex.GetPixels(0, 0, inTex.width, inTex.height);
		Color.RGBToHSV(color, out var H, out var S, out var V);
		if (V > 1f)
		{
			V /= 255f;
		}
		for (int i = 0; i < pixels.Length; i++)
		{
			if (pixels[i] != Color.clear && pixels[i] != Color.white)
			{
				float a = pixels[i].a;
				Color.RGBToHSV(pixels[i], out var H2, out var S2, out var V2);
				H2 += H;
				S2 *= S;
				V2 *= V;
				while (H2 > 1f)
				{
					H2 -= 1f;
				}
				for (; H2 < 0f; H2 += 1f)
				{
				}
				pixels[i] = Color.HSVToRGB(H2, S2, V2);
				pixels[i].a = a;
			}
		}
		texture2D.SetPixels(0, 0, inTex.width, inTex.height, pixels);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D SetColor(Color color, Texture2D inTex)
	{
		Texture2D texture2D = new Texture2D(inTex.width, inTex.height);
		Color[] pixels = inTex.GetPixels(0, 0, inTex.width, inTex.height);
		Color.RGBToHSV(color, out var H, out var S, out var V);
		if (V > 1f)
		{
			V /= 255f;
		}
		for (int i = 0; i < pixels.Length; i++)
		{
			if (pixels[i] != Color.clear)
			{
				float a = pixels[i].a;
				Color.RGBToHSV(pixels[i], out var H2, out var S2, out var V2);
				H2 = H;
				S2 = S;
				V2 = V;
				pixels[i] = Color.HSVToRGB(H2, S2, V2);
				pixels[i].a = a;
			}
		}
		texture2D.SetPixels(0, 0, inTex.width, inTex.height, pixels);
		texture2D.Apply();
		return texture2D;
	}

	public static Material SetShiftColor(Material mat, PBColor color)
	{
		mat.SetFloat("_HueShift", color.h * 360f);
		mat.SetFloat("_Sat", color.s);
		mat.SetFloat("_Val", color.v);
		return mat;
	}

	public static Material SetShiftColors(Material mat, PBColor color, PBColor color2)
	{
		mat = SetShiftColor(mat, color);
		mat.SetFloat("_HueShift2", color2.h * 360f);
		mat.SetFloat("_Sat2", color2.s);
		mat.SetFloat("_Val2", color2.v);
		return mat;
	}

	public static void Imagify(Transform source, Transform target)
	{
		if ((object)hsv == null || (object)hsvwb == null)
		{
			hsv = Shader.Find("Custom/HSVShader");
			hsvwb = Shader.Find("Custom/HSV+white2black");
		}
		MaxHeap<Transform> maxHeap = new MaxHeap<Transform>((Transform a, Transform b) => Comparer<float>.Default.Compare(a.localPosition.z, b.localPosition.z));
		SpriteRenderer[] componentsInChildren = source.GetComponentsInChildren<SpriteRenderer>(includeInactive: false);
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			Sprite sprite = spriteRenderer.sprite;
			if (sprite == null || !spriteRenderer.enabled)
			{
				continue;
			}
			GameObject gameObject = new GameObject(spriteRenderer.name);
			Image image = gameObject.AddComponent<Image>();
			image.sprite = sprite;
			Material sharedMaterial = spriteRenderer.sharedMaterial;
			if (sharedMaterial.shader.name.StartsWith("Custom/litHSV"))
			{
				if (sharedMaterial.shader.name.EndsWith("+white2black"))
				{
					image.material = new Material(hsvwb);
					image.material.SetFloat("_Black", sharedMaterial.GetFloat("_Black"));
				}
				else
				{
					image.material = new Material(hsv);
				}
				image.material.SetFloat("_HueShift", sharedMaterial.GetFloat("_HueShift"));
				image.material.SetFloat("_Sat", sharedMaterial.GetFloat("_Sat"));
				image.material.SetFloat("_Val", sharedMaterial.GetFloat("_Val"));
			}
			else if (sharedMaterial.name == "Diffuse")
			{
				image.material = image.defaultMaterial;
			}
			else
			{
				image.material = sharedMaterial;
			}
			image.SetNativeSize();
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			rectTransform.pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.localPosition = Vector3.ProjectOnPlane(source.InverseTransformPoint(spriteRenderer.transform.position), Vector3.forward) * 100f + Vector3.back * spriteRenderer.sortingOrder;
			if (spriteRenderer.transform != source)
			{
				rectTransform.localEulerAngles = spriteRenderer.transform.localEulerAngles;
				rectTransform.localScale = new Vector3((!spriteRenderer.flipX) ? 1 : (-1), (!spriteRenderer.flipY) ? 1 : (-1), 1f);
			}
			maxHeap.Add(rectTransform);
		}
		while (maxHeap.Count != 0)
		{
			maxHeap.ExtractDominating().SetParent(target, worldPositionStays: false);
		}
	}

	public static GameObject ShadowImage(Transform source, Sprite sprite, float duration = float.PositiveInfinity, float alpha = 1f)
	{
		Transform transform = new GameObject(source.name + " Shadow").transform;
		transform.position = source.position;
		transform.parent = source.parent;
		transform.gameObject.AddComponent<SortingGroup>();
		transform.gameObject.AddComponent<BillboardSprite>().LateUpdate();
		Queue<SpriteRenderer> queue = new Queue<SpriteRenderer>();
		SpriteRenderer[] componentsInChildren = source.GetComponentsInChildren<SpriteRenderer>(includeInactive: false);
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.sprite != null && spriteRenderer.enabled && spriteRenderer.GetComponent<SpriteMask>() == null)
			{
				GameObject gameObject = new GameObject(spriteRenderer.name);
				SpriteRenderer spriteRenderer2 = gameObject.AddComponent<SpriteRenderer>();
				spriteRenderer2.sprite = sprite;
				spriteRenderer2.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
				spriteRenderer2.sortingOrder = spriteRenderer.sortingOrder;
				spriteRenderer2.flipX = spriteRenderer.flipX;
				spriteRenderer2.flipY = spriteRenderer.flipY;
				gameObject.AddComponent<SpriteMask>().sprite = spriteRenderer.sprite;
				gameObject.transform.SetParent(transform, worldPositionStays: false);
				gameObject.transform.position = spriteRenderer.transform.position;
				gameObject.transform.rotation = spriteRenderer.transform.rotation;
				gameObject.transform.localScale = spriteRenderer.transform.lossyScale;
				queue.Enqueue(spriteRenderer2);
			}
		}
		if (!float.IsPositiveInfinity(duration))
		{
			FadingShadow fadingShadow = transform.gameObject.AddComponent<FadingShadow>();
			fadingShadow.sprites = queue.ToArray();
			fadingShadow.delta = alpha / duration;
			fadingShadow.color = new Color(1f, 1f, 1f, alpha);
		}
		return transform.gameObject;
	}
}
