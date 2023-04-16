using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlatMap : MonoBehaviour
{
	private static FlatMap instance;

	[SerializeField]
	private Camera renderCamera;

	[SerializeField]
	private RawImage mapImage;

	[SerializeField]
	private RectTransform medium;

	[SerializeField]
	private Image planet;

	[SerializeField]
	private Text planetText;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private GameObject mapMarker;

	[SerializeField]
	private GameObject transportalizer;

	[SerializeField]
	private Sprite villageSprite;

	[SerializeField]
	private GameObject backButton;

	private readonly Dictionary<int, RectTransform> maps = new Dictionary<int, RectTransform>();

	private readonly List<Tuple<Transform, Transform>> movingMarkers = new List<Tuple<Transform, Transform>>();

	private readonly List<Button> transportalizers = new List<Button>();

	private readonly Dictionary<string, AssetBundle> groundBundles = new Dictionary<string, AssetBundle>();

	private readonly Dictionary<string, AssetBundle> waterBundles = new Dictionary<string, AssetBundle>();

	private readonly Dictionary<string, AssetBundle> propBundles = new Dictionary<string, AssetBundle>();

	private AssetBundle cloudBundle;

	public void Init()
	{
		instance = this;
		cloudBundle = AssetBundleExtensions.Load("mapclouds");
	}

	public void Destroy()
	{
		foreach (AssetBundle value in groundBundles.Values)
		{
			value.Unload(unloadAllLoadedObjects: true);
		}
		foreach (AssetBundle value2 in waterBundles.Values)
		{
			value2.Unload(unloadAllLoadedObjects: true);
		}
		foreach (AssetBundle value3 in propBundles.Values)
		{
			value3.Unload(unloadAllLoadedObjects: true);
		}
		cloudBundle.Unload(unloadAllLoadedObjects: true);
	}

	private void Update()
	{
		RectTransform content = scrollRect.content;
		if (content != medium)
		{
			Vector2 vector = Quaternion.Inverse(content.localRotation) * content.anchoredPosition;
			float y = MSPAOrthoController.main.transform.localRotation.eulerAngles.y;
			Quaternion quaternion2 = (content.localRotation = Quaternion.Euler(0f, 0f, y));
			Vector2 vector2 = content.sizeDelta / 2f;
			vector.x = Mathf.Repeat(vector.x + vector2.x / 2f, vector2.x) - vector2.x / 2f;
			vector.y = Mathf.Repeat(vector.y + vector2.y / 2f, vector2.y) - vector2.y / 2f;
			content.anchoredPosition = quaternion2 * vector;
		}
		foreach (var (mapObject, original) in movingMarkers)
		{
			Sync(original, mapObject);
		}
	}

	private void OnDisable()
	{
		foreach (Button transportalizer in instance.transportalizers)
		{
			if (transportalizer == null)
			{
				break;
			}
			transportalizer.enabled = false;
		}
	}

	public static void MapPlanet(Transform transform, int id, float yMin, float yMax, int width, int height)
	{
		instance.renderCamera.transform.position = transform.position;
		instance.renderCamera.nearClipPlane = 0f - yMax;
		instance.renderCamera.farClipPlane = 0f - yMin + 1f;
		instance.renderCamera.orthographicSize = (float)height / 2f;
		instance.renderCamera.aspect = (float)width / (float)height;
		RenderTexture renderTexture = new RenderTexture(width, height, 24);
		instance.renderCamera.targetTexture = renderTexture;
		instance.renderCamera.Render();
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		RenderTexture.active = null;
		instance.renderCamera.targetTexture = null;
		texture2D.Apply();
		RawImage rawImage = UnityEngine.Object.Instantiate(instance.mapImage, instance.mapImage.transform.parent);
		rawImage.texture = texture2D;
		rawImage.uvRect = new Rect(0.5f, 0.5f, 2f, 2f);
		rawImage.SetNativeSize();
		instance.maps.Add(id, rawImage.rectTransform);
	}

	public static void AddPlanet(int id, string name, Color groundColor, Color waterColor, Color cloudColor, string ground, string water, string clouds, string props)
	{
		Transform parent = instance.planet.transform.parent;
		Image image = UnityEngine.Object.Instantiate(instance.planet, parent);
		if (ground != null)
		{
			image.sprite = LoadSprite(ground, "mapground", instance.groundBundles);
		}
		image.material = ImageEffects.SetShiftColor(new Material(image.material), groundColor);
		Image component = image.transform.GetChild(0).GetComponent<Image>();
		if (waterColor == Color.clear)
		{
			component.enabled = false;
		}
		else if (water != null)
		{
			component.sprite = LoadSprite(water, "mapwater", instance.waterBundles);
		}
		component.material = ImageEffects.SetShiftColor(new Material(component.material), waterColor);
		Image component2 = image.transform.GetChild(1).GetComponent<Image>();
		if (string.IsNullOrEmpty(props))
		{
			component2.enabled = false;
		}
		else
		{
			component2.sprite = LoadSprite(props, "mapprops", instance.propBundles);
		}
		Image component3 = image.transform.GetChild(2).GetComponent<Image>();
		if (cloudColor == Color.clear)
		{
			component3.enabled = false;
		}
		else if (clouds != null)
		{
			component3.sprite = LoadSprite(clouds, instance.cloudBundle);
		}
		component3.material = ImageEffects.SetShiftColor(new Material(component.material), cloudColor);
		image.GetComponent<Button>().onClick.AddListener(delegate
		{
			instance.SwitchToPlanet(id);
		});
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter
		};
		entry.callback.AddListener(delegate
		{
			instance.planetText.text = name;
		});
		image.GetComponent<EventTrigger>().triggers.Add(entry);
		image.gameObject.SetActive(value: true);
		int count = instance.maps.Count;
		int num = 0;
		foreach (Transform item in parent)
		{
			item.localPosition = 150f * new Vector3(Mathf.Cos((float)num * 2f * (float)Math.PI / (float)count), Mathf.Sin((float)num * 2f * (float)Math.PI / (float)count));
			num++;
		}
	}

	private static Sprite LoadSprite(string path, string assetBundle, IDictionary<string, AssetBundle> assetBundles)
	{
		string[] array = path.Split(new char[1] { '/' }, 2);
		if (!assetBundles.TryGetValue(array[0], out var value))
		{
			value = AssetBundleExtensions.Load(assetBundle + "/" + array[0]);
			assetBundles.Add(array[0], value);
		}
		return LoadSprite(array[1], value);
	}

	private static Sprite LoadSprite(string name, AssetBundle bundle)
	{
		Sprite[] array = bundle.LoadAssetWithSubAssets<Sprite>(name);
		return array[UnityEngine.Random.Range(0, array.Length)];
	}

	private void SwitchToPlanet(int id)
	{
		if (scrollRect.content != null)
		{
			scrollRect.content.gameObject.SetActive(value: false);
		}
		maps[id].gameObject.SetActive(value: true);
		scrollRect.content = maps[id];
		backButton.SetActive(value: true);
	}

	public void SwitchToMedium()
	{
		if (scrollRect.content != null)
		{
			scrollRect.content.gameObject.SetActive(value: false);
		}
		medium.gameObject.SetActive(value: true);
		scrollRect.content = medium;
		backButton.SetActive(value: false);
	}

	public static Image[] AddMarker(Sprite sprite, Transform transform, bool moving = false, bool colored = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(instance.mapMarker);
		Sync(transform, gameObject.transform);
		Image[] componentsInChildren = gameObject.GetComponentsInChildren<Image>();
		Image[] array = componentsInChildren;
		foreach (Image image in array)
		{
			image.sprite = sprite;
			image.SetNativeSize();
			if (!colored)
			{
				image.material = null;
			}
		}
		gameObject.SetActive(value: true);
		if (moving)
		{
			instance.movingMarkers.Add(Tuple.Create(gameObject.transform, transform));
		}
		return componentsInChildren;
	}

	public static void RemoveMarker(Transform transform)
	{
		int num = instance.movingMarkers.FindIndex((Tuple<Transform, Transform> marker) => marker.Item2 == transform);
		if (num == -1)
		{
			Debug.LogError($"Could not find marker for {transform}!");
			return;
		}
		if (instance.movingMarkers[num].Item1 != null)
		{
			UnityEngine.Object.Destroy(instance.movingMarkers[num].Item1.gameObject);
		}
		instance.movingMarkers.RemoveAt(num);
	}

	public static void AddTransportalizer(Transform source, Transform teleportPos)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(instance.transportalizer);
		Sync(source, gameObject.transform);
		Button[] componentsInChildren = gameObject.GetComponentsInChildren<Button>();
		foreach (Button button in componentsInChildren)
		{
			button.onClick.AddListener(delegate
			{
				Player.player.SetPosition(teleportPos);
			});
			instance.transportalizers.Add(button);
		}
		gameObject.gameObject.SetActive(value: true);
	}

	public static void AddVillage(Transform transform)
	{
		AddMarker(instance.villageSprite, transform);
	}

	private static void Sync(Transform original, Transform mapObject)
	{
		House component = original.root.GetComponent<House>();
		if (component == null)
		{
			mapObject.gameObject.SetActive(value: false);
			return;
		}
		mapObject.gameObject.SetActive(value: true);
		instance.maps.TryGetValue(component.Id, out var value);
		if (value == null)
		{
			mapObject.gameObject.SetActive(value: false);
			return;
		}
		mapObject.SetParent(value, worldPositionStays: false);
		Vector3 localPosition = original.localPosition;
		mapObject.localPosition = new Vector3(localPosition.x, localPosition.z);
	}

	public static void OpenTransportalizerMode()
	{
		instance.mapImage.GetComponentsInParent<PauseMenu>(includeInactive: true)[0].OpenTab(5);
		foreach (Button transportalizer in instance.transportalizers)
		{
			transportalizer.enabled = true;
		}
	}
}
