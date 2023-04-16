using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public static Menu singleton;

	private HouseBuilder houseBuilder;

	public Slider zoomSlider;

	public Button zoomButton;

	public Button phernaliaButton;

	public RectTransform phernaliaRegistry;

	private string[] phernaliaRegistryButtons = new string[5] { "Cruxtruder", "Punch Designix", "Totem Lathe", "Alchemiter", "Prepunched Card" };

	private string[] prepunchedCardCode = new string[8] { "EntryCat", "EntryBtl", "EntryDnk", "EntryBec", "EntryApp", "EntryApl", "EntryOrg", "EntryEgg" };

	public Button deploymentButton;

	public RectTransform deploymentRepository;

	private string[] deploymentRepositoryFurniture;

	private string[] deploymentRepositoryDoors;

	public Button atheneumButton;

	public RectTransform atheneum;

	public Button backgroundOption;

	[SerializeField]
	private Text gristCache;

	private bool phernaliaIsOpen;

	private int deploymentOpenPosition;

	private int deploymentOpenTime;

	private int deploymentOpenClock;

	private bool deploymentIsOpen;

	private int atheneumOpenPosition;

	private int atheneumOpenTime;

	private int atheneumOpenClock;

	private bool atheneumIsOpen;

	private AssetBundle assetBundle;

	public static bool AnyMenuOpen()
	{
		if (!singleton.phernaliaIsOpen && !singleton.deploymentIsOpen)
		{
			return singleton.atheneumIsOpen;
		}
		return true;
	}

	public void ToggleZoomSlider()
	{
		zoomSlider.gameObject.SetActive(!zoomSlider.gameObject.activeSelf);
	}

	public void TogglePhernaliaRegistry()
	{
		phernaliaIsOpen = !phernaliaIsOpen;
		phernaliaRegistry.gameObject.SetActive(phernaliaIsOpen);
	}

	public void ToggleDeploymentRepository()
	{
		if (!deploymentIsOpen)
		{
			deploymentOpenClock = -deploymentOpenClock;
			deploymentRepository.gameObject.SetActive(value: true);
		}
		deploymentIsOpen = !deploymentIsOpen;
	}

	public void ToggleAtheneum()
	{
		if (!atheneumIsOpen)
		{
			atheneumOpenClock = -atheneumOpenClock;
			atheneum.gameObject.SetActive(value: true);
		}
		atheneumIsOpen = !atheneumIsOpen;
	}

	private void UpdatePhernaliaRegistry()
	{
		GameObject gameObject = phernaliaRegistry.transform.GetChild(0).gameObject;
		string[] array = phernaliaRegistryButtons;
		foreach (string text in array)
		{
			GameObject clone = UnityEngine.Object.Instantiate(gameObject, phernaliaRegistry);
			clone.name = text;
			Button component = clone.GetComponent<Button>();
			if (clone.name.StartsWith("Prefabs/"))
			{
				string shortName = clone.name.Substring("Prefabs/".Length);
				component.onClick.AddListener(delegate
				{
					houseBuilder.SelectFurniture("Prefabs/" + shortName);
				});
				UnityEngine.Object.DestroyImmediate(clone.transform.Find("Image").GetComponent<Image>());
				clone.transform.Find("Text").GetComponent<Text>().text = shortName;
				continue;
			}
			if (clone.name == "Prepunched Card")
			{
				component.onClick.AddListener(delegate
				{
					NormalItem result = new NormalItem(prepunchedCardCode[UnityEngine.Random.Range(0, prepunchedCardCode.Length)], Item.ItemType.Entry);
					houseBuilder.SelectItem(new PunchCard(result));
				});
			}
			else
			{
				component.onClick.AddListener(delegate
				{
					houseBuilder.SelectFurniture(clone.name);
				});
			}
			AssetBundleRequest request = assetBundle.LoadAssetAsync<Sprite>(text);
			request.completed += delegate
			{
				clone.transform.Find("Image").GetComponent<Image>().sprite = (Sprite)request.asset;
			};
			clone.transform.Find("Text").GetComponent<Text>().text = clone.name;
		}
		UnityEngine.Object.Destroy(gameObject);
	}

	private void UpdatePhernaliaVisibility(House house)
	{
		Debug.Log("Updating Phernalia Visibility!");
		Button[] componentsInChildren = phernaliaRegistry.GetComponentsInChildren<Button>();
		foreach (Button button in componentsInChildren)
		{
			button.interactable = (!(button.name == "Cruxtruder") || !house.HasCruxtruder) && (!(button.name == "Totem Lathe") || !house.HasTotemLathe) && (!(button.name == "Alchemiter") || !house.HasAlchemiter) && (!(button.name == "Punch Designix") || !house.HasPunchDesignix) && (!(button.name == "Spawner") || !(house.GetComponentInChildren<DungeonEntrance>() != null)) && (!(button.name == "Prepunched Card") || (!house.HasPrepunchedCard && !house.HasPlanet));
		}
	}

	private void UpdateDeploymentRepositoryPosition()
	{
		deploymentRepository.transform.Translate(new Vector3(0f, (float)(-deploymentOpenClock * deploymentOpenClock - deploymentOpenPosition) - deploymentRepository.transform.localPosition.y, 0f));
	}

	public void UpdateDeploymentRepository(string searchString = "")
	{
		searchString = searchString.ToUpper();
		int num = (Screen.width - 8) / 128;
		GameObject gameObject = deploymentRepository.transform.GetChild(0).GetChild(0).GetChild(0)
			.gameObject;
		gameObject.SetActive(value: true);
		for (int i = 1; i < gameObject.transform.parent.childCount; i++)
		{
			UnityEngine.Object.Destroy(gameObject.transform.parent.GetChild(i).gameObject);
		}
		int num2 = 0;
		int num3 = 0;
		string[] array = deploymentRepositoryFurniture;
		foreach (string furniture in array)
		{
			if (!furniture.ToUpper().Contains(searchString))
			{
				continue;
			}
			GameObject clone = UnityEngine.Object.Instantiate(gameObject, deploymentRepository.transform.GetChild(0).GetChild(0));
			clone.transform.Translate(new Vector3(num2 * 128, -num3 * 128));
			clone.name = furniture;
			clone.GetComponent<Button>().onClick.AddListener(delegate
			{
				houseBuilder.SelectFurniture(furniture);
				if (deploymentIsOpen)
				{
					ToggleDeploymentRepository();
				}
			});
			AssetBundleRequest request = assetBundle.LoadAssetAsync<Sprite>(furniture);
			request.completed += delegate
			{
				clone.transform.Find("Image").GetComponent<Image>().sprite = (Sprite)request.asset;
			};
			clone.transform.Find("Text").GetComponent<Text>().text = Sylladex.MetricFormat(Furniture.GetCost(furniture));
			num2++;
			if (num2 >= num)
			{
				num2 = 0;
				num3++;
			}
		}
		array = deploymentRepositoryDoors;
		foreach (string door in array)
		{
			if (!door.ToUpper().Contains(searchString))
			{
				continue;
			}
			GameObject clone2 = UnityEngine.Object.Instantiate(gameObject, deploymentRepository.transform.GetChild(0).GetChild(0));
			clone2.transform.Translate(new Vector3(num2 * 128, -num3 * 128));
			clone2.name = door;
			clone2.GetComponent<Button>().onClick.AddListener(delegate
			{
				houseBuilder.SelectFurniture(door);
				if (deploymentIsOpen)
				{
					ToggleDeploymentRepository();
				}
			});
			AssetBundleRequest request2 = assetBundle.LoadAssetAsync<Sprite>(door);
			request2.completed += delegate
			{
				clone2.transform.Find("Image").GetComponent<Image>().sprite = (Sprite)request2.asset;
			};
			clone2.transform.Find("Text").GetComponent<Text>().text = Sylladex.MetricFormat(Furniture.GetCost(door));
			num2++;
			if (num2 >= num)
			{
				num2 = 0;
				num3++;
			}
		}
		if (num2 == 0)
		{
			gameObject.SetActive(value: false);
		}
		else
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		RectTransform component = deploymentRepository.GetChild(0).GetChild(0).GetComponent<RectTransform>();
		Vector2 offsetMax = component.offsetMax;
		offsetMax.y = (float)((num3 + 1) * 128) + component.offsetMin.y;
		component.offsetMax = offsetMax;
		Vector3 localPosition = component.localPosition;
		localPosition.y = 0f;
		component.localPosition = localPosition;
	}

	private void UpdateAtheneumPosition()
	{
		atheneum.Translate(new Vector3(0f, (float)(-atheneumOpenClock * atheneumOpenClock - atheneumOpenPosition) - atheneum.localPosition.y, 0f));
	}

	private static IEnumerable<NormalItem> GetAtheneumItems(House house)
	{
		if (!BuildExploreSwitcher.cheatMode)
		{
			return house.atheneum;
		}
		string[] code = StreamingAssets.ReadAllLines("atheneum.txt");
		return ((IEnumerable<LDBItem>)ItemDownloader.Instance.GetItems(code)).Select((Func<LDBItem, NormalItem>)((LDBItem it) => it));
	}

	private void SetAtheneum(House house, House oldHouse)
	{
		int num = (Screen.width - 8) / 128;
		Button button = null;
		foreach (Transform item in atheneum.GetChild(0).GetChild(0))
		{
			if (button == null)
			{
				button = item.GetComponent<Button>();
			}
			else
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		Image component = button.transform.Find("Dowel").GetComponent<Image>();
		component.material = ImageEffects.SetShiftColor(new Material(component.material), house.cruxiteColor);
		int num2 = 0;
		foreach (NormalItem atheneumItem in GetAtheneumItems(house))
		{
			AddAtheneumButton(atheneumItem, num2 % num, num2 / num, button);
			num2++;
		}
		if ((bool)oldHouse)
		{
			oldHouse.atheneum.Callback -= OnAtheneumChanged;
		}
		house.atheneum.Callback += OnAtheneumChanged;
		button.gameObject.SetActive(value: false);
	}

	private void AddAtheneumButton(NormalItem item, int coordX, int coordY, Button button)
	{
		Button button2 = UnityEngine.Object.Instantiate(button, button.transform.parent);
		button2.transform.Translate(new Vector3(coordX * 128, -coordY * 128));
		button2.name = item.GetItemName();
		if (BuildExploreSwitcher.cheatMode)
		{
			button2.onClick.AddListener(delegate
			{
				houseBuilder.SelectItem(item.Copy());
			});
		}
		else
		{
			button2.onClick.AddListener(delegate
			{
				houseBuilder.SelectItem(new Totem(item, houseBuilder.House.cruxiteColor));
			});
		}
		Image component = button2.transform.Find("Image").GetComponent<Image>();
		component.sprite = item.sprite;
		component.preserveAspect = true;
		button2.GetComponent<Tooltipped>().tooltip = item.GetItemName();
		button2.gameObject.SetActive(value: true);
	}

	private void OnAtheneumChanged(SyncSet<NormalItem>.Operation op, NormalItem item)
	{
		if (op != 0)
		{
			Debug.LogError($"Atheneum can only handle ADD operations, not {op}!");
			return;
		}
		Button component = atheneum.GetChild(0).GetChild(0).GetChild(0)
			.GetComponent<Button>();
		Vector3 vector = atheneum.GetChild(atheneum.childCount - 1).position - component.transform.position;
		int num = (Screen.width - 8) / 128;
		AddAtheneumButton(item, (int)((vector.x / 128f + 1f) % (float)num), (int)((0f - vector.y) / 128f + (vector.x / 128f + 1f) / (float)num), component);
	}

	public void SetHouse(House house, House oldHouse = null)
	{
		SetAtheneum(house, oldHouse);
		if ((bool)oldHouse && (bool)oldHouse.Owner)
		{
			oldHouse.Owner.GristCache = null;
		}
		if ((bool)house.Owner)
		{
			house.Owner.GristCache = gristCache;
			gristCache.text = Sylladex.MetricFormat(house.Owner.Grist[Grist.SpecialType.Build]);
		}
	}

	private void Start()
	{
		zoomButton.onClick.AddListener(ToggleZoomSlider);
		houseBuilder = UnityEngine.Object.FindObjectOfType<HouseBuilder>();
		if (BuildExploreSwitcher.cheatMode)
		{
			Queue<string> queue = new Queue<string>();
			queue.Enqueue("Player");
			queue.Enqueue("Spawner");
			queue.Enqueue("Item Maker (Debug)");
			string[] creatureNames = SpawnHelper.instance.GetCreatureNames();
			foreach (string path in creatureNames)
			{
				queue.Enqueue("Prefabs/" + Path.GetFileNameWithoutExtension(path));
			}
			phernaliaRegistryButtons = queue.ToArray();
		}
		else if (NetcodeManager.Instance.offline)
		{
			phernaliaRegistryButtons = new List<string>(phernaliaRegistryButtons) { "Spawner", "Item Maker (Debug)" }.ToArray();
		}
		deploymentOpenPosition = -(int)deploymentRepository.transform.localPosition.y;
		deploymentOpenTime = Mathf.CeilToInt(Mathf.Sqrt(deploymentRepository.position.y + deploymentRepository.rect.yMax));
		deploymentOpenClock = deploymentOpenTime;
		UpdateDeploymentRepositoryPosition();
		deploymentRepository.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (Screen.width - 8) / 128 * 128);
		deploymentRepositoryFurniture = StreamingAssets.ReadAllLines("furniture.txt");
		deploymentRepositoryDoors = StreamingAssets.ReadAllLines("doors.txt");
		AssetBundleCreateRequest request = AssetBundleExtensions.LoadAsync("furnitureicon");
		request.completed += delegate
		{
			assetBundle = request.assetBundle;
			UpdatePhernaliaRegistry();
			UpdateDeploymentRepository();
		};
		atheneumOpenPosition = -(int)atheneum.transform.localPosition.y;
		atheneumOpenTime = Mathf.CeilToInt(Mathf.Sqrt(atheneum.position.y + atheneum.rect.yMax));
		atheneumOpenClock = atheneumOpenTime;
		UpdateAtheneumPosition();
		atheneum.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (Screen.width - 8) / 128 * 128);
		if (BuildExploreSwitcher.cheatMode)
		{
			string[] creatureNames = House.GetBackgroundOptions();
			foreach (string path2 in creatureNames)
			{
				string simpleName = Path.GetFileNameWithoutExtension(path2);
				Button button = UnityEngine.Object.Instantiate(backgroundOption, backgroundOption.transform.parent);
				button.transform.GetChild(0).GetComponent<Text>().text = simpleName;
				button.onClick.AddListener(delegate
				{
					houseBuilder.House.SetBackground(simpleName);
				});
			}
			backgroundOption.transform.GetChild(0).GetComponent<Text>().text = "None";
			backgroundOption.onClick.AddListener(delegate
			{
				houseBuilder.House.SetBackground(null);
			});
		}
		else
		{
			UnityEngine.Object.Destroy(backgroundOption.transform.parent.parent.gameObject);
		}
		backgroundOption = null;
		SetHouse(houseBuilder.House);
		singleton = this;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject != zoomSlider.gameObject && EventSystem.current.currentSelectedGameObject != zoomButton.gameObject)
		{
			zoomSlider.gameObject.SetActive(value: false);
		}
		if (phernaliaIsOpen && Input.GetMouseButtonUp(0) && EventSystem.current.currentSelectedGameObject != phernaliaButton.gameObject)
		{
			TogglePhernaliaRegistry();
		}
		if (deploymentIsOpen && Input.GetMouseButtonUp(0) && EventSystem.current.currentSelectedGameObject != deploymentButton.gameObject && !deploymentRepository.rect.Contains(deploymentRepository.InverseTransformPoint(Input.mousePosition)))
		{
			ToggleDeploymentRepository();
		}
		if (deploymentOpenClock < 0 || (!deploymentIsOpen && deploymentOpenClock < deploymentOpenTime))
		{
			deploymentOpenClock++;
			UpdateDeploymentRepositoryPosition();
		}
		else if (deploymentOpenClock >= deploymentOpenTime)
		{
			deploymentRepository.gameObject.SetActive(value: false);
		}
		if (atheneumIsOpen && Input.GetMouseButtonUp(0) && EventSystem.current.currentSelectedGameObject != atheneumButton.gameObject)
		{
			ToggleAtheneum();
		}
		if (atheneumOpenClock < 0 || (!atheneumIsOpen && atheneumOpenClock < atheneumOpenTime))
		{
			atheneumOpenClock++;
			UpdateAtheneumPosition();
		}
		else if (atheneumOpenClock >= atheneumOpenTime)
		{
			atheneum.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		deploymentOpenClock = deploymentOpenTime;
		UpdateDeploymentRepositoryPosition();
		atheneumOpenClock = atheneumOpenTime;
		UpdateAtheneumPosition();
	}

	private void OnDestroy()
	{
		assetBundle.Unload(unloadAllLoadedObjects: true);
	}
}
