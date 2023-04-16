using System.IO;
using UnityEngine;

public class RenderObject : MonoBehaviour
{
	public static int size = 112;

	public static string path = "Assets/Environment/House/Textures/Icons/";

	private Camera renderCamera;

	private RenderTexture renderTexture;

	private void makeBackgroundClear(Texture2D texture)
	{
		Color pixel = texture.GetPixel(0, 0);
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				Color pixel2 = texture.GetPixel(i, j);
				if (pixel2 == pixel)
				{
					texture.SetPixel(i, j, Color.clear);
				}
				else if (pixel2.a == 0f)
				{
					texture.SetPixel(i, j, Color.black);
				}
			}
		}
	}

	private void MakeSquarePngFromOurVirtualThingy(string savePath)
	{
		renderCamera.aspect = 1f;
		RenderTexture renderTexture = new RenderTexture(size, size, 24);
		renderCamera.targetTexture = renderTexture;
		renderCamera.Render();
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(size, size, TextureFormat.ARGB32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, size, size), 0, 0);
		makeBackgroundClear(texture2D);
		RenderTexture.active = null;
		renderCamera.targetTexture = null;
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(savePath, bytes);
	}

	private void Start()
	{
		renderCamera = GetComponent<Camera>();
		GameObject[] array = AssetBundleExtensions.Load("furniture").LoadAllAssets<GameObject>();
		foreach (GameObject gameObject in array)
		{
			GameObject obj = Object.Instantiate(gameObject);
			Furniture component = gameObject.GetComponent<Furniture>();
			renderCamera.orthographicSize = (float)(Mathf.Max(component.Size.x + component.Size.y, 4) * 2) / 3f;
			MakeSquarePngFromOurVirtualThingy(path + gameObject.name + ".png");
			MonoBehaviour.print(gameObject.name);
			Object.DestroyImmediate(obj);
		}
		array = AssetBundleExtensions.Load("door").LoadAllAssets<GameObject>();
		foreach (GameObject gameObject2 in array)
		{
			GameObject obj2 = Object.Instantiate(gameObject2);
			gameObject2.transform.localPosition = Vector3.zero;
			WallFurniture component2 = gameObject2.GetComponent<WallFurniture>();
			renderCamera.orthographicSize = (float)Mathf.Max(component2.Size.x, 2) * 1.5f;
			MakeSquarePngFromOurVirtualThingy(path + gameObject2.name + ".png");
			MonoBehaviour.print(gameObject2.name);
			Object.DestroyImmediate(obj2);
		}
		PunchCard punchCard = new PunchCard(new NormalItem("00000000"));
		punchCard.SceneObject.GetComponent<PickupItemAction>().enabled = false;
		punchCard.SceneObject.transform.position = Vector3.zero;
		punchCard.SceneObject.SetActive(value: true);
		renderCamera.orthographicSize = 0.5f;
		renderCamera.transform.localPosition = new Vector3(-1f, 1f, -1f);
		MakeSquarePngFromOurVirtualThingy(path + "Prepunched Card.png");
		Debug.Break();
	}

	public static Sprite getIcon(string name)
	{
		return Sprite.Create(Resources.Load<Texture2D>("Icons/" + name), new Rect(0f, 0f, size, size), Vector2.zero);
	}

	private void Update()
	{
	}
}
