using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(RegionChild))]
public class Grist : NetworkBehaviour
{
	public enum SpecialType
	{
		Build,
		Artifact,
		Zillium,
		Count
	}

	private static Grist spriteGrist;

	private static Grist masterGrist;

	private static readonly List<Grist> grists = new List<Grist>();

	[SerializeField]
	private int value = 1;

	[SerializeField]
	private int type;

	private Sprite[] sprites;

	private SpriteRenderer renderer;

	private Rigidbody rigidbody;

	private static readonly object delay = new WaitForSeconds(0.1f);

	public const int LEVELS = 5;

	public const int COUNT = 63;

	private static string[] names;

	private static AssetBundle assetBundle;

	private static Grist[] modelGrist;

	private static Material[] materials;

	private static Color[] colors;

	private static AssetBundle AssetBundle
	{
		get
		{
			if (!(assetBundle != null))
			{
				return assetBundle = AssetBundleExtensions.Load("grist");
			}
			return assetBundle;
		}
	}

	public static void RegisterPrefabs()
	{
		spriteGrist = Resources.Load<Grist>("Prefabs/Grist");
		NetworkClient.RegisterPrefab(spriteGrist.gameObject);
		modelGrist = Resources.LoadAll<Grist>("Prefabs/Grists");
		Grist[] array = modelGrist;
		for (int i = 0; i < array.Length; i++)
		{
			NetworkClient.RegisterPrefab(array[i].gameObject);
		}
	}

	public static Grist Make(int value, int type, Vector3 position, WorldArea area)
	{
		Grist grist = Object.Instantiate(HasSpriteGrist(value, type) ? spriteGrist : GetPrefab(type));
		grist.Set(value, type);
		grist.GetComponent<RegionChild>().Area = area;
		grist.transform.position = position;
		NetworkServer.Spawn(grist.gameObject);
		return grist;
	}

	private static bool HasSpriteGrist(int value, int type)
	{
		if (value >= 64)
		{
			if (IsSpecial(type))
			{
				return type != 0;
			}
			return false;
		}
		return true;
	}

	private void Set(int value, int type)
	{
		this.value = value;
		this.type = type;
		if (HasSpriteGrist(value, type))
		{
			sprites = GetSprites(type);
			renderer = GetComponent<SpriteRenderer>();
			renderer.sprite = sprites[0];
			base.transform.localScale *= Mathf.Log(value, 2f) / 2f + 1f;
		}
		else
		{
			if (!IsSpecial(type))
			{
				GetComponent<MeshRenderer>().material.color = GetColor(type);
			}
			base.transform.localScale *= Mathf.Log(value, 2f) / 2f - 2.5f;
		}
		base.name = value + "Grist";
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write(value);
			writer.Write(type);
		}
		return base.OnSerialize(writer, initialState);
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			Set(reader.Read<int>(), reader.Read<int>());
		}
		base.OnDeserialize(reader, initialState);
	}

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		Object.Destroy(base.gameObject, 300f);
	}

	private void OnEnable()
	{
		grists.Add(this);
		if ((object)masterGrist == null)
		{
			masterGrist = this;
			StartCoroutine(GristMagnetism());
		}
	}

	private void OnDisable()
	{
		grists.Remove(this);
		if (!(masterGrist != this))
		{
			if (grists.Count != 0)
			{
				masterGrist = grists[0];
				masterGrist.StartCoroutine(GristMagnetism());
			}
			else
			{
				masterGrist = null;
			}
		}
	}

	private IEnumerator OnBecameVisible()
	{
		if (sprites.Length > 1)
		{
			int index = 0;
			do
			{
				renderer.sprite = sprites[index];
				yield return delay;
				index = (index + 1) % sprites.Length;
			}
			while (renderer.isVisible);
		}
	}

	private static IEnumerator GristMagnetism()
	{
		while (true)
		{
			yield return null;
			IEnumerable<Player> all = Player.GetAll();
			foreach (Grist grist in grists)
			{
				foreach (Player item in all)
				{
					Vector3 vector = item.transform.position - grist.transform.position;
					float sqrMagnitude = vector.sqrMagnitude;
					if (sqrMagnitude < 25f)
					{
						grist.rigidbody.AddForce(vector / sqrMagnitude * 50f, ForceMode.Force);
					}
				}
			}
		}
	}

	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		if (NetworkServer.active && other.TryGetComponent<Player>(out var component))
		{
			component.Grist[type] += value;
			Object.Destroy(base.gameObject);
		}
	}

	public static string GetName(int index)
	{
		if (names == null)
		{
			names = (from s in File.ReadLines(Application.streamingAssetsPath + "/grist.txt")
				where !string.IsNullOrWhiteSpace(s)
				select s).ToArray();
		}
		return names[index];
	}

	public static Sprite[] GetSprites(int index)
	{
		string text;
		if (IsSpecial(index))
		{
			SpecialType specialType = (SpecialType)index;
			text = specialType.ToString();
		}
		else
		{
			text = $"{(Aspect)GetType(index)}{GetTier(index) + 1}";
		}
		return AssetBundle.LoadAssetWithSubAssets<Sprite>(text);
	}

	private static Grist GetPrefab(int index)
	{
		return modelGrist[(index != 0) ? (GetTier(index) + 1) : 0];
	}

	public static Material GetMaterial(int index)
	{
		if (materials == null)
		{
			MakeMaterials();
		}
		return materials[index - 3];
	}

	public static Color GetColor(int index)
	{
		if (colors == null)
		{
			MakeMaterials();
		}
		return colors[index - 3];
	}

	private static void MakeMaterials()
	{
		Color[] pixels = AssetBundle.LoadAsset<Texture2D>("palettes").GetPixels();
		Shader shader = Shader.Find("Custom/litHSV2");
		colors = new Color[pixels.Length / 2];
		materials = new Material[pixels.Length / 2];
		for (int i = 0; i < pixels.Length / 2; i++)
		{
			Color color = pixels[2 * i];
			Color color2 = pixels[2 * i + 1];
			colors[i] = color;
			Material mat = new Material(shader);
			materials[i] = ImageEffects.SetShiftColors(mat, color, color2);
		}
	}

	public static int GetIndex(SpecialType type)
	{
		return (int)type;
	}

	public static int GetIndex(int level, Aspect type)
	{
		return 3 + (int)type * 5 + level;
	}

	public static int GetType(int index)
	{
		return (index - 3) / 5;
	}

	public static int GetTier(int index)
	{
		return (index - 3) % 5;
	}

	public static bool IsSpecial(int index)
	{
		return index < 3;
	}

	private void MirrorProcessed()
	{
	}
}
