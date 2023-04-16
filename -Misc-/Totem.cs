using System;
using UnityEngine;
using UnityEngine.UI;

public class Totem : Item
{
	public NormalItem makeItem;

	private readonly Color color;

	private readonly Material mat;

	private static Material shiftMat;

	private static Material solidMat;

	private const int ANGULAR_PRECISION = 16;

	private const int HEIGHT_PRECISION = 3;

	private const float MAX_RADIUS = 0.225f;

	private const float MIN_RADIUS = 0.074999996f;

	private const float HEIGHT = 0.33f;

	public override bool IsEntry => makeItem.IsEntry;

	public bool IsDowel
	{
		get
		{
			if (makeItem.captchaCode == "00000000")
			{
				return makeItem.itemType == ItemType.Normal;
			}
			return false;
		}
	}

	protected override string Prefab
	{
		get
		{
			if (!IsDowel)
			{
				return "Totem";
			}
			return "CruxiteDowel";
		}
	}

	public override string GetItemName()
	{
		if (!IsDowel)
		{
			return "Cruxite Totem";
		}
		return "Cruxite Dowel";
	}

	public override bool Equals(object obj)
	{
		if (obj is Totem totem)
		{
			return makeItem.Equals(totem.makeItem);
		}
		return false;
	}

	private int reverse(int n)
	{
		return (n & 1) * 32 + (n & 2) * 8 + (n & 4) * 2 + (n & 8) / 2 + (n & 0x10) / 8 + (n & 0x20) / 32;
	}

	private int decodeChar(char c)
	{
		int n = (('0' <= c && c <= '9') ? (c - 48) : (('A' <= c && c <= 'Z') ? (c - 65 + 10) : (('a' <= c && c <= 'z') ? (c - 97 + 36) : ((c != '?') ? 63 : 62))));
		return reverse(n);
	}

	private int[] getcaptchaInts(string s)
	{
		char[] array = s.ToCharArray();
		int[] array2 = new int[8];
		for (int i = 0; i < 8; i++)
		{
			array2[i] = decodeChar(array[i]);
		}
		return array2;
	}

	private float subInterpolate(int first, int last, float x)
	{
		return (3f * x * x - 2f * x * x * x) * (float)(last - first) + (float)first;
	}

	private float interpolate(int[] points, int i, int mm)
	{
		int num = i / mm;
		if (i % mm == 0)
		{
			return points[num];
		}
		float x = (float)(i % mm) * 1f / (float)mm;
		return subInterpolate(points[num], points[num + 1], x);
	}

	private float getRad(float x)
	{
		return 0.225f + x / 63f * -0.15f;
	}

	private void setShape()
	{
		int num = 22;
		int[] array = getcaptchaInts(makeItem.captchaCode);
		int num2 = int.MaxValue;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] < num2)
			{
				num2 = array[i];
			}
		}
		float rad = getRad(num2);
		base.SceneObject.GetComponent<BoxCollider>().size = new Vector3(2f * rad, 2f * rad, 0.66f);
		Vector3[] array2 = new Vector3[16 * (num + 2) + 2];
		int[] array3 = new int[6 * num * 16];
		for (int j = 0; j < num; j++)
		{
			float z = 0.33f * ((float)j * 2f / (float)(num - 1) - 1f);
			float rad2 = getRad(interpolate(array, j, 3));
			for (int k = 0; k < 16; k++)
			{
				float f = (float)(k * 2) * (float)Math.PI / 16f;
				array2[16 * j + k] = new Vector3(rad2 * Mathf.Cos(f), rad2 * Mathf.Sin(f), z);
				if (j < num - 1)
				{
					array3[6 * (16 * j + k)] = 16 * j + k;
					array3[6 * (16 * j + k) + 1] = 16 * j + (k + 1) % 16;
					array3[6 * (16 * j + k) + 2] = 16 * (j + 1) + k;
					array3[6 * (16 * j + k) + 3] = 16 * j + k;
					array3[6 * (16 * j + k) + 4] = 16 * (j + 1) + k;
					array3[6 * (16 * j + k) + 5] = 16 * (j + 1) + (16 + k - 1) % 16;
				}
			}
		}
		float rad3 = getRad(array[0]);
		float rad4 = getRad(array[7]);
		for (int l = 0; l < 16; l++)
		{
			float f2 = (float)(l * 2) * (float)Math.PI / 16f;
			array2[16 * num + l] = new Vector3(rad3 * Mathf.Cos(f2), rad3 * Mathf.Sin(f2), -0.33f);
			array2[16 * (num + 1) + l] = new Vector3(rad4 * Mathf.Cos(f2), rad4 * Mathf.Sin(f2), 0.33f);
			array3[6 * ((num - 1) * 16 + l)] = 16 * num + l;
			array3[6 * ((num - 1) * 16 + l) + 1] = 16 * (num + 2);
			array3[6 * ((num - 1) * 16 + l) + 2] = 16 * num + (l + 1) % 16;
			array3[6 * ((num - 1) * 16 + l) + 3] = 16 * (num + 1) + l;
			array3[6 * ((num - 1) * 16 + l) + 4] = 16 * (num + 1) + (l + 1) % 16;
			array3[6 * ((num - 1) * 16 + l) + 5] = 16 * (num + 2) + 1;
		}
		array2[16 * (num + 2)] = new Vector3(0f, 0f, -0.33f);
		array2[16 * (num + 2) + 1] = new Vector3(0f, 0f, 0.33f);
		Mesh mesh = base.SceneObject.GetComponent<MeshFilter>().mesh;
		mesh.vertices = array2;
		mesh.triangles = array3;
		mesh.RecalculateNormals();
	}

	public override Material GetMaterial()
	{
		return mat ?? solidMat;
	}

	public override Color GetColor()
	{
		if ((object)mat != null)
		{
			return Color.white;
		}
		return color;
	}

	public override void ApplyToImage(Image image)
	{
		image.enabled = true;
		if (mat == null)
		{
			image.sprite = makeItem.sprite;
			image.color = color;
			image.material = solidMat;
		}
		else
		{
			image.sprite = base.sprite;
			image.material = mat;
		}
	}

	public override Item Copy()
	{
		return new Totem(this);
	}

	public Totem(Totem totem)
		: base(totem)
	{
		makeItem = totem.makeItem;
		mat = totem.mat;
		color = totem.color;
	}

	public Totem(NormalItem item, Color color)
		: base(ItemType.Totem)
	{
		makeItem = item;
		base.weight = 0.3f;
		if (IsDowel)
		{
			base.sprite = ItemDownloader.GetSprite("Dowel");
			base.description = "Insert this into the totem lathe to carve a totem.";
			if (shiftMat == null)
			{
				shiftMat = new Material(Shader.Find("Custom/HSVShader"));
				mat = shiftMat;
			}
			else
			{
				mat = new Material(shiftMat);
			}
			mat = ImageEffects.SetShiftColor(mat, color);
		}
		else
		{
			base.sprite = item.sprite;
			base.description = "Insert this into the alchemiter to make the item.";
			if (solidMat == null)
			{
				solidMat = new Material(Shader.Find("Custom/monocolor"));
			}
		}
		this.color = color;
	}

	protected override void FillItemObject()
	{
		base.FillItemObject();
		if (!IsDowel)
		{
			setShape();
		}
		base.SceneObject.GetComponent<MeshRenderer>().material.color = color;
		base.SceneObject.SetActive(value: false);
	}

	public Totem(HouseData.Totem data)
		: this((NormalItem)Item.Load(data.result), new Color(data.color.x, data.color.y, data.color.z))
	{
	}

	public override HouseData.Item Save()
	{
		return new HouseData.Totem
		{
			result = makeItem.Save(),
			color = new Vector3(color.r, color.g, color.b)
		};
	}
}
