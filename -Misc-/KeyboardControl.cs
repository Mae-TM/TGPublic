using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyboardControl : MonoBehaviour
{
	private static KeyboardControl instance;

	[SerializeField]
	private PauseMenu pauseMenu;

	private bool isTyping;

	private Controls controls;

	private bool isHovering;

	private float lastRaycastTime;

	private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

	private int blocking;

	private bool isDragging;

	public static bool IsHovering
	{
		get
		{
			instance.Raycast();
			return instance.isHovering;
		}
	}

	public static bool IsDragging => instance.isDragging;

	public static bool IsReady => instance != null;

	public static IEnumerable<InputActionMap> Maps => instance.controls.asset.actionMaps;

	public static Controls.PlayerActions PlayerControls => instance.controls.Player;

	public static Controls.CameraActions CameraControls => instance.controls.Camera;

	public static Controls.UIActions UIControls => instance.controls.UI;

	public static Controls.HouseBuilderActions HouseBuilderControls => instance.controls.HouseBuilder;

	public static bool IsQuickAction => instance.controls.Interaction.QuickAction.phase == InputActionPhase.Started;

	public static bool IsItemAction => instance.controls.Interaction.ItemAction.phase == InputActionPhase.Started;

	private void Awake()
	{
		instance = this;
		controls = new Controls();
		File.Delete(Application.persistentDataPath + "/bindings.txt");
		if (File.Exists(Application.persistentDataPath + "/bindings.bin"))
		{
			using FileStream stream = File.OpenRead(Application.persistentDataPath + "/bindings.bin");
			ReadControls(stream);
		}
		controls.Enable();
		controls.UI.PauseMenu.performed += TogglePauseMenu;
		controls.UI.ToggleUI.performed += ToggleUI;
	}

	private void OnDestroy()
	{
		using (FileStream stream = File.Create(Application.persistentDataPath + "/bindings.bin"))
		{
			WriteControls(stream);
		}
		controls.Disable();
		controls = null;
	}

	private void WriteControls(Stream stream)
	{
		using BinaryWriter binaryWriter = new BinaryWriter(stream);
		foreach (InputAction item in controls.asset)
		{
			for (int i = 0; i < item.bindings.Count; i++)
			{
				if (item.bindings[i].overridePath != null)
				{
					binaryWriter.Write(item.id.ToByteArray());
					binaryWriter.Write(i);
					binaryWriter.Write(item.bindings[i].overridePath);
				}
			}
		}
	}

	private void ReadControls(Stream stream)
	{
		using BinaryReader binaryReader = new BinaryReader(stream);
		while (stream.Position < stream.Length)
		{
			Guid guid = new Guid(binaryReader.ReadBytes(16));
			int bindingIndex = binaryReader.ReadInt32();
			string path = binaryReader.ReadString();
			controls.asset.FindAction(guid).ApplyBindingOverride(bindingIndex, path);
		}
	}

	private void Update()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		isTyping = currentSelectedGameObject != null && currentSelectedGameObject.TryGetComponent<InputField>(out var component) && component.isFocused;
	}

	private void Raycast()
	{
		if (!(lastRaycastTime >= Time.time))
		{
			lastRaycastTime = Time.time;
			PointerEventData eventData = new PointerEventData(EventSystem.current)
			{
				position = Input.mousePosition
			};
			EventSystem.current.RaycastAll(eventData, raycastResults);
			isHovering = raycastResults.Any((RaycastResult result) => result.distance == 0f);
			raycastResults.Clear();
		}
	}

	private void TogglePauseMenu(InputAction.CallbackContext context)
	{
		if (pauseMenu.gameObject.activeSelf)
		{
			pauseMenu.ResumeGame();
		}
		else if (BuildExploreSwitcher.Instance.IsInBuildMode)
		{
			BuildExploreSwitcher.Instance.SwitchToExplore();
		}
		else
		{
			pauseMenu.PauseGame();
		}
	}

	private void ToggleUI(InputAction.CallbackContext context)
	{
		if (!IsKeyboardBlocked())
		{
			if (BuildExploreSwitcher.IsExploring)
			{
				BuildExploreSwitcher.Instance.PlayerUIActive = !BuildExploreSwitcher.Instance.PlayerUIActive;
			}
			else if (BuildExploreSwitcher.Instance.IsInBuildMode)
			{
				BuildExploreSwitcher.Instance.ServerUIActive = !BuildExploreSwitcher.Instance.ServerUIActive;
			}
			GlobalChat.ToggleActive();
		}
	}

	public static void Block()
	{
		instance.blocking++;
	}

	public static void Unblock()
	{
		instance.blocking--;
	}

	public static void Drag()
	{
		instance.isDragging = true;
	}

	public static void Undrag()
	{
		instance.isDragging = false;
	}

	public static bool IsMouseBlocked(bool ignoreClickOpen = false)
	{
		if (instance.blocking <= 0 && !IsDragging)
		{
			if (!ignoreClickOpen)
			{
				if (!IsHovering && AutoClose.activeCount == 0 && (object)ClickOpen.active == null)
				{
					return ClickOpen.hovering;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool IsKeyboardBlocked()
	{
		if (instance.blocking <= 0)
		{
			return instance.isTyping;
		}
		return true;
	}

	public static void Disable()
	{
		instance.controls.Disable();
	}

	public static void Enable()
	{
		instance.controls.Enable();
	}

	public static InputAction GetAbilityBinding(int index)
	{
		return (new InputAction[6]
		{
			instance.controls.Ability.Attack,
			instance.controls.Ability.Abjure,
			instance.controls.Ability.Slide,
			instance.controls.Ability.Ability1,
			instance.controls.Ability.Ability2,
			instance.controls.Ability.Ability3
		})[index];
	}

	public static Ray GetMouseRay(Camera camera)
	{
		return camera.ScreenPointToRay(Mouse.current.position.ReadValue());
	}

	public static bool Raycast(out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity)
	{
		return Raycast(out hitInfo, MSPAOrthoController.main, maxDistance);
	}

	public static bool Raycast(out RaycastHit hitInfo, Camera camera, float maxDistance = float.PositiveInfinity)
	{
		Ray mouseRay = GetMouseRay(camera);
		return Physics.Raycast(mouseRay.origin, mouseRay.direction, out hitInfo, maxDistance, camera.cullingMask & -5);
	}
}
