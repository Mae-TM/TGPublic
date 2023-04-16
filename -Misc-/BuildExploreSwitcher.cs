using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.ImageEffects;

public class BuildExploreSwitcher : MonoBehaviour
{
	public static bool cheatMode;

	public MSPAOrthoController camera;

	public HouseBuilder houseBuilder;

	[SerializeField]
	private GameObject houseInterface;

	[SerializeField]
	private EventSystem eventSystem;

	[SerializeField]
	private Material wallAlphaMaterial;

	private Furniture accessPoint;

	public Action OnSwitchToExplore;

	public Action OnSwitchToBuild;

	public static bool IsExploring { get; private set; }

	public static BuildExploreSwitcher Instance { get; private set; }

	private static Camera Cam => MSPAOrthoController.main;

	public bool IsInBuildMode => houseBuilder.gameObject.activeSelf;

	public bool PlayerUIActive
	{
		get
		{
			return Player.Ui.gameObject.activeSelf;
		}
		set
		{
			Player.Ui.gameObject.SetActive(value);
		}
	}

	public bool ServerUIActive
	{
		get
		{
			return houseInterface.activeSelf;
		}
		set
		{
			houseInterface.SetActive(value);
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
		RegionChild.onFocusRegionChanged += OnFocusRegionChanged;
	}

	private void OnDestroy()
	{
		cheatMode = false;
		RegionChild.onFocusRegionChanged -= OnFocusRegionChanged;
	}

	public void SwitchToExplore()
	{
		if (IsInBuildMode)
		{
			SwitchFromBuild();
		}
		float zoomLevel = Player.player.RegionChild.Region.ZoomLevel;
		camera.zoom = zoomLevel;
		eventSystem.sendNavigationEvents = false;
		IsExploring = true;
		Player.player.sylladex.enabled = true;
		Player.player.sylladex.PlaySoundEffect(null);
		Player.player.GetComponent<PlayerController>().reactToInput = true;
		Player.player.RegionChild.Focus();
		PlayerUIActive = true;
		OnSwitchToExplore?.Invoke();
	}

	private void SwitchFromExplore()
	{
		IsExploring = false;
		Player.player.sylladex.enabled = false;
		Player.player.GetComponent<PlayerController>().reactToInput = false;
		Player.player.RegionChild.Unfocus();
		PlayerUIActive = false;
	}

	public void SwitchToBuild()
	{
		SwitchToBuild(null);
	}

	public void SwitchToBuild(Furniture accessPoint)
	{
		House house = ((!cheatMode) ? AbstractSingletonManager<WorldManager>.Instance.GetClient() : (Player.player.RegionChild.Area as House));
		if (house != null && house.Owner != null)
		{
			SwitchToBuild(accessPoint, house);
		}
	}

	public void SwitchToBuild(Furniture accessPoint, House house)
	{
		if (IsExploring)
		{
			SwitchFromExplore();
		}
		if (house == null)
		{
			house = houseBuilder.House;
		}
		houseBuilder.Open(house);
		eventSystem.sendNavigationEvents = true;
		if (accessPoint != null)
		{
			accessPoint.AllowMovement = false;
			this.accessPoint = accessPoint;
		}
		OnSwitchToBuild?.Invoke();
	}

	private void SwitchFromBuild()
	{
		houseBuilder.gameObject.SetActive(value: false);
		if (accessPoint != null)
		{
			accessPoint.AllowMovement = true;
			accessPoint = null;
		}
	}

	public void OnBossEnter(Transform cameraParent)
	{
		camera.enabled = false;
		camera.cameraAngle = 0f;
		camera.ForceUpdate();
		camera.transform.position = cameraParent.position;
	}

	private void OnFocusRegionChanged(WorldRegion oldRegion, WorldRegion region)
	{
		camera.enabled = true;
		BackgroundMusic.instance.PlayBasic(region.Music, 1f);
		RenderSettings.ambientLight = region.AmbientLight;
		Cam.backgroundColor = region.BackgroundColor;
		if (string.IsNullOrEmpty(Player.player.sylladex.PlayerName))
		{
			camera.zoom = 1f / 3f;
			camera.SetFocusPoint(Player.player.transform.position);
		}
		else
		{
			camera.zoom = region.ZoomLevel;
		}
		camera.GetComponent<EdgeDetection>().sensitivityDepth = 15f;
	}
}
