using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HouseBuilder : MonoBehaviour, ISaveLoadable
{
	[Serializable]
	private struct CursorOption
	{
		[SerializeField]
		private Texture2D texture;

		[SerializeField]
		private Vector2 hotspot;

		public void Set()
		{
			if (PlayerPrefs.GetInt("HardwareCursor", 0) == 0)
			{
				Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
			}
		}
	}

	public enum Mode
	{
		Select,
		Room,
		Floor,
		Bulldoze
	}

	public static string saveAs = "Interlude.bds";

	[SerializeField]
	private Camera camera;

	[SerializeField]
	private SpriteRenderer selection;

	[SerializeField]
	private Renderer grid;

	[SerializeField]
	private GameObject recycleArea;

	[SerializeField]
	private GameObject stashArea;

	[SerializeField]
	private GameObject floorButton;

	[SerializeField]
	private Slider zoomSlider;

	[SerializeField]
	private Material alphaWallMaterial;

	[SerializeField]
	private CursorOption selectCursor;

	[SerializeField]
	private CursorOption reviseCursor;

	[SerializeField]
	private CursorOption deployCursor;

	[SerializeField]
	private CursorOption copyCursor;

	[SerializeField]
	private CursorOption stairsCursor;

	[SerializeField]
	private CursorOption floorCursor;

	[SerializeField]
	private CursorOption bulldozeCursor;

	private int story;

	private Action onPress;

	private Action onRelease;

	private Action onPressing;

	private Action onMove;

	private Vector2Int selectionStart;

	private Furniture furniture;

	private ItemObject item;

	private float itemHeight;

	private Mode mode = (Mode)(-1);

	[SerializeField]
	private SaveLoad saveLoad;

	private bool isSaving;

	public House House { get; private set; }

	private static bool isCopying => KeyboardControl.HouseBuilderControls.Copy.phase == InputActionPhase.Started;

	public event Action<string> OnDeployFurniture;

	public void Open(House to)
	{
		if (House != to)
		{
			if ((bool)Menu.singleton)
			{
				Menu.singleton.SetHouse(to, House);
			}
			House = to;
		}
		base.gameObject.SetActive(value: true);
		SetStory(House.SetStoryVisible(story));
		SetMode(Mode.Select);
	}

	private void Awake()
	{
		Wall.alphaMaterial = alphaWallMaterial;
	}

	private void OnEnable()
	{
		KeyboardControl.HouseBuilderControls.ChangeFloor.performed += ChangeFloor;
		KeyboardControl.HouseBuilderControls.ToggleGrid.performed += ToggleGrid;
		KeyboardControl.HouseBuilderControls.PlaceStairs.performed += PlaceStairs;
		KeyboardControl.HouseBuilderControls.PlaceDoor.performed += PlaceDoor;
		KeyboardControl.HouseBuilderControls.Zoom.performed += Zoom;
		Wall.SetCam(camera, newTransparent: true);
	}

	private void OnDisable()
	{
		KeyboardControl.HouseBuilderControls.ChangeFloor.performed -= ChangeFloor;
		KeyboardControl.HouseBuilderControls.ToggleGrid.performed -= ToggleGrid;
		KeyboardControl.HouseBuilderControls.PlaceStairs.performed -= PlaceStairs;
		KeyboardControl.HouseBuilderControls.PlaceDoor.performed -= PlaceDoor;
		KeyboardControl.HouseBuilderControls.Zoom.performed -= Zoom;
		if ((bool)House)
		{
			House.SetStoryVisible(-1, story);
		}
		SetMode((Mode)(-1));
		Wall.SetCam(MSPAOrthoController.main, newTransparent: false);
	}

	private void ChangeFloor(InputAction.CallbackContext context)
	{
		ChangeFloor((context.ReadValue<float>() > 0f) ? 1 : (-1));
	}

	public void ChangeFloor(int delta)
	{
		SetStory(House.SetStoryVisible(Math.Max(0, story + delta), story));
	}

	private void SetStory(int to)
	{
		Vector3 position = (float)to * 1.5f * 3f * Vector3.up;
		base.transform.localPosition = House.transform.TransformPoint(position);
		story = to;
	}

	private void ToggleGrid(InputAction.CallbackContext ctx)
	{
		if (!GlobalChat.instance.messageField.isFocused)
		{
			grid.enabled = !grid.enabled;
		}
	}

	private void PlaceStairs(InputAction.CallbackContext ctx)
	{
		SelectFurniture("Stairs");
	}

	private void PlaceDoor(InputAction.CallbackContext ctx)
	{
		SelectFurniture("Large Generic Door");
	}

	public void Undo()
	{
		House.CmdUndoCommand();
	}

	public void Redo()
	{
		House.CmdRedoCommand();
	}

	private void Zoom(InputAction.CallbackContext context)
	{
		if (!Menu.AnyMenuOpen())
		{
			zoomSlider.value -= context.ReadValue<float>() / 1000f;
		}
	}

	private Vector3 GetMousePosition()
	{
		return GetMousePosition(base.transform.localPosition.y);
	}

	private Vector3 GetMousePosition(float height)
	{
		Ray mouseRay = KeyboardControl.GetMouseRay(camera);
		new Plane(Vector3.down, height).Raycast(mouseRay, out var enter);
		return House.transform.InverseTransformPoint(mouseRay.GetPoint(enter));
	}

	private Vector2Int GetMouseCoords()
	{
		Vector3Int coords = Building.GetCoords(GetMousePosition());
		coords.x = Mathf.Clamp(coords.x, -30, 30);
		coords.z = Mathf.Clamp(coords.z, -30, 30);
		return new Vector2Int(coords.x, coords.z);
	}

	private void SetSelect(RectInt rect)
	{
		Vector2 rhs = new Vector2(Math.Abs(rect.width), Math.Abs(rect.height));
		selection.size = 1.5f * Vector2.Max(0.25f * Vector2.one, rhs);
		selection.transform.localPosition = 1.5f * new Vector3(rect.center.x, 0.15f, rect.center.y);
	}

	public void SetMode(int to)
	{
		SetMode((Mode)to);
	}

	public void SetMode(Mode to)
	{
		if (mode != to)
		{
			if (mode == Mode.Select)
			{
				DropSelection();
				ItemObject.OnGetAuthority -= OnGetItem;
				Furniture.OnGetAuthority -= OnGetFurniture;
				KeyboardControl.HouseBuilderControls.Copy.started -= StartCopy;
				KeyboardControl.HouseBuilderControls.Copy.canceled -= EndCopy;
			}
			else if (mode != (Mode)(-1))
			{
				selection.enabled = false;
			}
			mode = to;
			(mode switch
			{
				Mode.Select => selectCursor, 
				Mode.Room => reviseCursor, 
				Mode.Floor => floorCursor, 
				Mode.Bulldoze => bulldozeCursor, 
				_ => default(CursorOption), 
			}).Set();
			if (mode == Mode.Select)
			{
				ItemObject.OnGetAuthority += OnGetItem;
				Furniture.OnGetAuthority += OnGetFurniture;
				KeyboardControl.HouseBuilderControls.Copy.started += StartCopy;
				KeyboardControl.HouseBuilderControls.Copy.canceled += EndCopy;
				onPress = OnSelectClick;
				onPressing = null;
				onRelease = null;
			}
			else if (mode != (Mode)(-1))
			{
				onPress = OnStartDrag;
				onPressing = OnDragging;
				onRelease = OnEndDrag;
				SpriteRenderer spriteRenderer = selection;
				spriteRenderer.color = mode switch
				{
					Mode.Room => Color.green, 
					Mode.Floor => Color.yellow, 
					Mode.Bulldoze => Color.red, 
					_ => throw new InvalidOperationException(), 
				};
			}
			floorButton.SetActive(mode == Mode.Room || mode == Mode.Floor);
		}
	}

	public void SelectFurniture(string furnitureName)
	{
		if (!GlobalChat.instance.messageField.isFocused)
		{
			SetMode(Mode.Select);
			Furniture.Spawn(furnitureName, House);
			this.OnDeployFurniture?.Invoke(furnitureName);
		}
	}

	public void SelectItem(Item toSpawn)
	{
		SetMode(Mode.Select);
		Vector3 mousePosition = GetMousePosition(base.transform.localPosition.y + 2.25f);
		toSpawn.PutDownLocal(House, mousePosition, default(Quaternion), asOwner: true);
	}

	private void StartCopy(InputAction.CallbackContext obj)
	{
		copyCursor.Set();
	}

	private void EndCopy(InputAction.CallbackContext obj)
	{
		(((bool)furniture || (bool)item) ? deployCursor : selectCursor).Set();
	}

	private void OnStartDrag()
	{
		selectionStart = GetMouseCoords();
	}

	private void OnDragging()
	{
		SetSelect(new RectInt(selectionStart, GetMouseCoords() - selectionStart));
		selection.enabled = true;
	}

	private void OnEndDrag()
	{
		switch (mode)
		{
		case Mode.Room:
		{
			int id = House.GetRoom(new Vector3Int(selectionStart.x, story, selectionStart.y)).Id;
			House.CmdSetRoom(new RectInt(selectionStart, GetMouseCoords() - selectionStart), story, id);
			break;
		}
		case Mode.Floor:
			House.CmdSetFloor(new RectInt(selectionStart, GetMouseCoords() - selectionStart), story);
			break;
		case Mode.Bulldoze:
			House.CmdRemoveRoom(new RectInt(selectionStart, GetMouseCoords() - selectionStart), story);
			break;
		}
		selection.enabled = false;
	}

	private void RecycleSelection(InputAction.CallbackContext ctx)
	{
		DropSelection(isRecycled: true);
	}

	public void DropSelection(bool isRecycled = false)
	{
		if (!furniture && !item)
		{
			return;
		}
		if ((bool)furniture)
		{
			if (isRecycled)
			{
				furniture.CmdRecycle();
			}
			else
			{
				furniture.CmdReleaseAuthority();
				SoundEffects.Instance.Kachunk(House);
			}
			furniture = null;
			selection.enabled = false;
			KeyboardControl.HouseBuilderControls.Rotate.performed -= RotateFurniture;
		}
		else
		{
			if (isRecycled && !item.Item.IsEntry)
			{
				item.CmdRecycle();
			}
			else
			{
				item.CmdReleaseAuthority();
			}
			item = null;
			KeyboardControl.HouseBuilderControls.ElevateItem.performed -= ElevateItem;
		}
		selectCursor.Set();
		onMove = null;
		stashArea.SetActive(value: false);
		recycleArea.SetActive(value: false);
		KeyboardControl.HouseBuilderControls.Recycle.performed -= RecycleSelection;
	}

	private void OnSelectClick()
	{
		if ((bool)furniture || (bool)item)
		{
			if ((bool)furniture && isCopying)
			{
				SelectFurniture(furniture.name);
			}
			else
			{
				DropSelection();
			}
		}
		else
		{
			if (!KeyboardControl.Raycast(out var hitInfo, camera))
			{
				return;
			}
			Furniture componentInParent = hitInfo.collider.GetComponentInParent<Furniture>();
			if ((bool)componentInParent)
			{
				if (isCopying)
				{
					SelectFurniture(componentInParent.name);
				}
				else if (componentInParent.AllowMovement)
				{
					componentInParent.CmdRequestAuthority();
				}
			}
			else
			{
				ItemObject componentInParent2 = hitInfo.collider.GetComponentInParent<ItemObject>();
				if ((bool)componentInParent2)
				{
					componentInParent2.CmdRequestAuthority();
				}
			}
		}
	}

	private void OnGetFurniture(Furniture newFurniture)
	{
		DropSelection();
		(isCopying ? copyCursor : deployCursor).Set();
		furniture = newFurniture;
		onMove = OnFurnitureHold;
		selection.enabled = true;
		recycleArea.SetActive(value: true);
		KeyboardControl.HouseBuilderControls.Recycle.performed += RecycleSelection;
		KeyboardControl.HouseBuilderControls.Rotate.performed += RotateFurniture;
		SoundEffects.Instance.Pickup();
	}

	private void OnGetItem(ItemObject newItem)
	{
		DropSelection();
		(isCopying ? copyCursor : deployCursor).Set();
		item = newItem;
		onMove = OnItemHold;
		if (newItem.Item is Totem totem && !totem.IsEntry)
		{
			stashArea.SetActive(value: true);
		}
		else
		{
			recycleArea.SetActive(value: true);
		}
		KeyboardControl.HouseBuilderControls.Recycle.performed += RecycleSelection;
		KeyboardControl.HouseBuilderControls.ElevateItem.performed += ElevateItem;
		itemHeight = (item.transform.localPosition.y - base.transform.localPosition.y) / 1.5f;
	}

	private void ElevateItem(InputAction.CallbackContext context)
	{
		float num = context.ReadValue<float>() / 360f;
		itemHeight = Mathf.Clamp(itemHeight - num, 0.1f, 2.9f);
	}

	private void RotateFurniture(InputAction.CallbackContext context)
	{
		furniture.CmdRotate(context.ReadValue<float>() > 0f);
	}

	private void OnItemHold()
	{
		item.transform.localPosition = GetMousePosition(base.transform.localPosition.y + 1.5f * itemHeight);
	}

	private void OnFurnitureHold()
	{
		Vector2Int mouseCoords = GetMouseCoords();
		bool flag = furniture.SetCoords(new Vector3Int(mouseCoords.x, story, mouseCoords.y));
		selection.color = (flag ? Color.green : Color.red);
		SetSelect(new RectInt(mouseCoords, furniture.Size));
	}

	private void Update()
	{
		if (Mouse.current.leftButton.wasPressedThisFrame && onPress != null && EventSystem.current.currentSelectedGameObject == null)
		{
			onPress();
		}
		else if (Mouse.current.leftButton.wasReleasedThisFrame && onRelease != null && EventSystem.current.currentSelectedGameObject == null)
		{
			onRelease();
		}
		else if (Mouse.current.leftButton.isPressed && onPressing != null && EventSystem.current.currentSelectedGameObject == null)
		{
			onPressing();
		}
		else
		{
			onMove?.Invoke();
		}
		if (Keyboard.current.ctrlKey.isPressed)
		{
			if (Keyboard.current[Key.Z].wasReleasedThisFrame)
			{
				Undo();
			}
			else if (Keyboard.current[Key.Y].wasReleasedThisFrame)
			{
				Redo();
			}
		}
	}

	public void Save()
	{
		isSaving = true;
		saveLoad.run(this, "Save House As", Application.streamingAssetsPath + "/Houses/", "*.b??", getDirectoryInsteadOfFile: false);
	}

	public void Load()
	{
		if (BuildExploreSwitcher.cheatMode)
		{
			isSaving = false;
			saveLoad.run(this, "Load House", Application.streamingAssetsPath + "/Houses/", "*.b??", getDirectoryInsteadOfFile: false);
		}
	}

	public void PickFile(string path)
	{
		if (isSaving)
		{
			path = Regex.Replace(path, "\\.[^\\\\/]*$", "") + ".bds";
			HouseManager.SaveHouse(House, path);
			return;
		}
		Player.player.RegionChild.SetRegion(null);
		base.transform.SetParent(null);
		UnityEngine.Object.DestroyImmediate(House.gameObject);
		HouseData data = HouseManager.LoadHouse(path);
		Open(HouseManager.Instance.SpawnHouse(Player.player.GetID(), data));
		House.Owner = Player.player;
		Player.player.MoveToSpawn(House);
	}

	public void Cancel()
	{
	}
}
