using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class Controls : IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct PlayerActions
	{
		private Controls m_Wrapper;

		public InputAction Move => m_Wrapper.m_Player_Move;

		public InputAction Jump => m_Wrapper.m_Player_Jump;

		public bool enabled => Get().enabled;

		public PlayerActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Player;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(PlayerActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IPlayerActions instance)
		{
			if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
			{
				Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
				Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
				Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
				Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
				Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
				Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
			}
			m_Wrapper.m_PlayerActionsCallbackInterface = instance;
			if (instance != null)
			{
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Jump.started += instance.OnJump;
				Jump.performed += instance.OnJump;
				Jump.canceled += instance.OnJump;
			}
		}
	}

	public struct CameraActions
	{
		private Controls m_Wrapper;

		public InputAction Rotate => m_Wrapper.m_Camera_Rotate;

		public bool enabled => Get().enabled;

		public CameraActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Camera;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(CameraActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(ICameraActions instance)
		{
			if (m_Wrapper.m_CameraActionsCallbackInterface != null)
			{
				Rotate.started -= m_Wrapper.m_CameraActionsCallbackInterface.OnRotate;
				Rotate.performed -= m_Wrapper.m_CameraActionsCallbackInterface.OnRotate;
				Rotate.canceled -= m_Wrapper.m_CameraActionsCallbackInterface.OnRotate;
			}
			m_Wrapper.m_CameraActionsCallbackInterface = instance;
			if (instance != null)
			{
				Rotate.started += instance.OnRotate;
				Rotate.performed += instance.OnRotate;
				Rotate.canceled += instance.OnRotate;
			}
		}
	}

	public struct UIActions
	{
		private Controls m_Wrapper;

		public InputAction PauseMenu => m_Wrapper.m_UI_PauseMenu;

		public InputAction ToggleUI => m_Wrapper.m_UI_ToggleUI;

		public InputAction OpenChat => m_Wrapper.m_UI_OpenChat;

		public InputAction MousePosition => m_Wrapper.m_UI_MousePosition;

		public bool enabled => Get().enabled;

		public UIActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterface != null)
			{
				PauseMenu.started -= m_Wrapper.m_UIActionsCallbackInterface.OnPauseMenu;
				PauseMenu.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnPauseMenu;
				PauseMenu.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnPauseMenu;
				ToggleUI.started -= m_Wrapper.m_UIActionsCallbackInterface.OnToggleUI;
				ToggleUI.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnToggleUI;
				ToggleUI.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnToggleUI;
				OpenChat.started -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenChat;
				OpenChat.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenChat;
				OpenChat.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnOpenChat;
				MousePosition.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMousePosition;
				MousePosition.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMousePosition;
				MousePosition.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMousePosition;
			}
			m_Wrapper.m_UIActionsCallbackInterface = instance;
			if (instance != null)
			{
				PauseMenu.started += instance.OnPauseMenu;
				PauseMenu.performed += instance.OnPauseMenu;
				PauseMenu.canceled += instance.OnPauseMenu;
				ToggleUI.started += instance.OnToggleUI;
				ToggleUI.performed += instance.OnToggleUI;
				ToggleUI.canceled += instance.OnToggleUI;
				OpenChat.started += instance.OnOpenChat;
				OpenChat.performed += instance.OnOpenChat;
				OpenChat.canceled += instance.OnOpenChat;
				MousePosition.started += instance.OnMousePosition;
				MousePosition.performed += instance.OnMousePosition;
				MousePosition.canceled += instance.OnMousePosition;
			}
		}
	}

	public struct AbilityActions
	{
		private Controls m_Wrapper;

		public InputAction Attack => m_Wrapper.m_Ability_Attack;

		public InputAction Abjure => m_Wrapper.m_Ability_Abjure;

		public InputAction Slide => m_Wrapper.m_Ability_Slide;

		public InputAction Ability1 => m_Wrapper.m_Ability_Ability1;

		public InputAction Ability2 => m_Wrapper.m_Ability_Ability2;

		public InputAction Ability3 => m_Wrapper.m_Ability_Ability3;

		public bool enabled => Get().enabled;

		public AbilityActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Ability;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(AbilityActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IAbilityActions instance)
		{
			if (m_Wrapper.m_AbilityActionsCallbackInterface != null)
			{
				Attack.started -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAttack;
				Attack.performed -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAttack;
				Attack.canceled -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAttack;
				Abjure.started -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbjure;
				Abjure.performed -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbjure;
				Abjure.canceled -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbjure;
				Slide.started -= m_Wrapper.m_AbilityActionsCallbackInterface.OnSlide;
				Slide.performed -= m_Wrapper.m_AbilityActionsCallbackInterface.OnSlide;
				Slide.canceled -= m_Wrapper.m_AbilityActionsCallbackInterface.OnSlide;
				Ability1.started -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility1;
				Ability1.performed -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility1;
				Ability1.canceled -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility1;
				Ability2.started -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility2;
				Ability2.performed -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility2;
				Ability2.canceled -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility2;
				Ability3.started -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility3;
				Ability3.performed -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility3;
				Ability3.canceled -= m_Wrapper.m_AbilityActionsCallbackInterface.OnAbility3;
			}
			m_Wrapper.m_AbilityActionsCallbackInterface = instance;
			if (instance != null)
			{
				Attack.started += instance.OnAttack;
				Attack.performed += instance.OnAttack;
				Attack.canceled += instance.OnAttack;
				Abjure.started += instance.OnAbjure;
				Abjure.performed += instance.OnAbjure;
				Abjure.canceled += instance.OnAbjure;
				Slide.started += instance.OnSlide;
				Slide.performed += instance.OnSlide;
				Slide.canceled += instance.OnSlide;
				Ability1.started += instance.OnAbility1;
				Ability1.performed += instance.OnAbility1;
				Ability1.canceled += instance.OnAbility1;
				Ability2.started += instance.OnAbility2;
				Ability2.performed += instance.OnAbility2;
				Ability2.canceled += instance.OnAbility2;
				Ability3.started += instance.OnAbility3;
				Ability3.performed += instance.OnAbility3;
				Ability3.canceled += instance.OnAbility3;
			}
		}
	}

	public struct HouseBuilderActions
	{
		private Controls m_Wrapper;

		public InputAction ToggleGrid => m_Wrapper.m_HouseBuilder_ToggleGrid;

		public InputAction PlaceDoor => m_Wrapper.m_HouseBuilder_PlaceDoor;

		public InputAction PlaceStairs => m_Wrapper.m_HouseBuilder_PlaceStairs;

		public InputAction Rotate => m_Wrapper.m_HouseBuilder_Rotate;

		public InputAction Zoom => m_Wrapper.m_HouseBuilder_Zoom;

		public InputAction ElevateItem => m_Wrapper.m_HouseBuilder_ElevateItem;

		public InputAction ChangeFloor => m_Wrapper.m_HouseBuilder_ChangeFloor;

		public InputAction Recycle => m_Wrapper.m_HouseBuilder_Recycle;

		public InputAction Copy => m_Wrapper.m_HouseBuilder_Copy;

		public bool enabled => Get().enabled;

		public HouseBuilderActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_HouseBuilder;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(HouseBuilderActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IHouseBuilderActions instance)
		{
			if (m_Wrapper.m_HouseBuilderActionsCallbackInterface != null)
			{
				ToggleGrid.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnToggleGrid;
				ToggleGrid.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnToggleGrid;
				ToggleGrid.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnToggleGrid;
				PlaceDoor.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnPlaceDoor;
				PlaceDoor.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnPlaceDoor;
				PlaceDoor.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnPlaceDoor;
				PlaceStairs.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnPlaceStairs;
				PlaceStairs.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnPlaceStairs;
				PlaceStairs.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnPlaceStairs;
				Rotate.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnRotate;
				Rotate.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnRotate;
				Rotate.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnRotate;
				Zoom.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnZoom;
				Zoom.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnZoom;
				Zoom.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnZoom;
				ElevateItem.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnElevateItem;
				ElevateItem.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnElevateItem;
				ElevateItem.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnElevateItem;
				ChangeFloor.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnChangeFloor;
				ChangeFloor.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnChangeFloor;
				ChangeFloor.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnChangeFloor;
				Recycle.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnRecycle;
				Recycle.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnRecycle;
				Recycle.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnRecycle;
				Copy.started -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnCopy;
				Copy.performed -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnCopy;
				Copy.canceled -= m_Wrapper.m_HouseBuilderActionsCallbackInterface.OnCopy;
			}
			m_Wrapper.m_HouseBuilderActionsCallbackInterface = instance;
			if (instance != null)
			{
				ToggleGrid.started += instance.OnToggleGrid;
				ToggleGrid.performed += instance.OnToggleGrid;
				ToggleGrid.canceled += instance.OnToggleGrid;
				PlaceDoor.started += instance.OnPlaceDoor;
				PlaceDoor.performed += instance.OnPlaceDoor;
				PlaceDoor.canceled += instance.OnPlaceDoor;
				PlaceStairs.started += instance.OnPlaceStairs;
				PlaceStairs.performed += instance.OnPlaceStairs;
				PlaceStairs.canceled += instance.OnPlaceStairs;
				Rotate.started += instance.OnRotate;
				Rotate.performed += instance.OnRotate;
				Rotate.canceled += instance.OnRotate;
				Zoom.started += instance.OnZoom;
				Zoom.performed += instance.OnZoom;
				Zoom.canceled += instance.OnZoom;
				ElevateItem.started += instance.OnElevateItem;
				ElevateItem.performed += instance.OnElevateItem;
				ElevateItem.canceled += instance.OnElevateItem;
				ChangeFloor.started += instance.OnChangeFloor;
				ChangeFloor.performed += instance.OnChangeFloor;
				ChangeFloor.canceled += instance.OnChangeFloor;
				Recycle.started += instance.OnRecycle;
				Recycle.performed += instance.OnRecycle;
				Recycle.canceled += instance.OnRecycle;
				Copy.started += instance.OnCopy;
				Copy.performed += instance.OnCopy;
				Copy.canceled += instance.OnCopy;
			}
		}
	}

	public struct InteractionActions
	{
		private Controls m_Wrapper;

		public InputAction QuickAction => m_Wrapper.m_Interaction_QuickAction;

		public InputAction ItemAction => m_Wrapper.m_Interaction_ItemAction;

		public bool enabled => Get().enabled;

		public InteractionActions(Controls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Interaction;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(InteractionActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IInteractionActions instance)
		{
			if (m_Wrapper.m_InteractionActionsCallbackInterface != null)
			{
				QuickAction.started -= m_Wrapper.m_InteractionActionsCallbackInterface.OnQuickAction;
				QuickAction.performed -= m_Wrapper.m_InteractionActionsCallbackInterface.OnQuickAction;
				QuickAction.canceled -= m_Wrapper.m_InteractionActionsCallbackInterface.OnQuickAction;
				ItemAction.started -= m_Wrapper.m_InteractionActionsCallbackInterface.OnItemAction;
				ItemAction.performed -= m_Wrapper.m_InteractionActionsCallbackInterface.OnItemAction;
				ItemAction.canceled -= m_Wrapper.m_InteractionActionsCallbackInterface.OnItemAction;
			}
			m_Wrapper.m_InteractionActionsCallbackInterface = instance;
			if (instance != null)
			{
				QuickAction.started += instance.OnQuickAction;
				QuickAction.performed += instance.OnQuickAction;
				QuickAction.canceled += instance.OnQuickAction;
				ItemAction.started += instance.OnItemAction;
				ItemAction.performed += instance.OnItemAction;
				ItemAction.canceled += instance.OnItemAction;
			}
		}
	}

	public interface IPlayerActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnJump(InputAction.CallbackContext context);
	}

	public interface ICameraActions
	{
		void OnRotate(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnPauseMenu(InputAction.CallbackContext context);

		void OnToggleUI(InputAction.CallbackContext context);

		void OnOpenChat(InputAction.CallbackContext context);

		void OnMousePosition(InputAction.CallbackContext context);
	}

	public interface IAbilityActions
	{
		void OnAttack(InputAction.CallbackContext context);

		void OnAbjure(InputAction.CallbackContext context);

		void OnSlide(InputAction.CallbackContext context);

		void OnAbility1(InputAction.CallbackContext context);

		void OnAbility2(InputAction.CallbackContext context);

		void OnAbility3(InputAction.CallbackContext context);
	}

	public interface IHouseBuilderActions
	{
		void OnToggleGrid(InputAction.CallbackContext context);

		void OnPlaceDoor(InputAction.CallbackContext context);

		void OnPlaceStairs(InputAction.CallbackContext context);

		void OnRotate(InputAction.CallbackContext context);

		void OnZoom(InputAction.CallbackContext context);

		void OnElevateItem(InputAction.CallbackContext context);

		void OnChangeFloor(InputAction.CallbackContext context);

		void OnRecycle(InputAction.CallbackContext context);

		void OnCopy(InputAction.CallbackContext context);
	}

	public interface IInteractionActions
	{
		void OnQuickAction(InputAction.CallbackContext context);

		void OnItemAction(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Player;

	private IPlayerActions m_PlayerActionsCallbackInterface;

	private readonly InputAction m_Player_Move;

	private readonly InputAction m_Player_Jump;

	private readonly InputActionMap m_Camera;

	private ICameraActions m_CameraActionsCallbackInterface;

	private readonly InputAction m_Camera_Rotate;

	private readonly InputActionMap m_UI;

	private IUIActions m_UIActionsCallbackInterface;

	private readonly InputAction m_UI_PauseMenu;

	private readonly InputAction m_UI_ToggleUI;

	private readonly InputAction m_UI_OpenChat;

	private readonly InputAction m_UI_MousePosition;

	private readonly InputActionMap m_Ability;

	private IAbilityActions m_AbilityActionsCallbackInterface;

	private readonly InputAction m_Ability_Attack;

	private readonly InputAction m_Ability_Abjure;

	private readonly InputAction m_Ability_Slide;

	private readonly InputAction m_Ability_Ability1;

	private readonly InputAction m_Ability_Ability2;

	private readonly InputAction m_Ability_Ability3;

	private readonly InputActionMap m_HouseBuilder;

	private IHouseBuilderActions m_HouseBuilderActionsCallbackInterface;

	private readonly InputAction m_HouseBuilder_ToggleGrid;

	private readonly InputAction m_HouseBuilder_PlaceDoor;

	private readonly InputAction m_HouseBuilder_PlaceStairs;

	private readonly InputAction m_HouseBuilder_Rotate;

	private readonly InputAction m_HouseBuilder_Zoom;

	private readonly InputAction m_HouseBuilder_ElevateItem;

	private readonly InputAction m_HouseBuilder_ChangeFloor;

	private readonly InputAction m_HouseBuilder_Recycle;

	private readonly InputAction m_HouseBuilder_Copy;

	private readonly InputActionMap m_Interaction;

	private IInteractionActions m_InteractionActionsCallbackInterface;

	private readonly InputAction m_Interaction_QuickAction;

	private readonly InputAction m_Interaction_ItemAction;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public PlayerActions Player => new PlayerActions(this);

	public CameraActions Camera => new CameraActions(this);

	public UIActions UI => new UIActions(this);

	public AbilityActions Ability => new AbilityActions(this);

	public HouseBuilderActions HouseBuilder => new HouseBuilderActions(this);

	public InteractionActions Interaction => new InteractionActions(this);

	public Controls()
	{
		asset = InputActionAsset.FromJson("{\n    \"name\": \"Controls\",\n    \"maps\": [\n        {\n            \"name\": \"Player\",\n            \"id\": \"5d85f227-a227-4819-b9b5-94f44096cb19\",\n            \"actions\": [\n                {\n                    \"name\": \"Move\",\n                    \"type\": \"Value\",\n                    \"id\": \"db4a393f-dc32-44ed-bb57-0d31c6eca0f5\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Jump\",\n                    \"type\": \"Value\",\n                    \"id\": \"59f00a9b-38d9-48af-a2c1-867f519c48ec\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"WASD\",\n                    \"id\": \"de04229c-6a44-4285-a912-177a689f0b05\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Up\",\n                    \"id\": \"ee29f13d-fcd9-4e1a-a39d-0dc3b414516c\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Down\",\n                    \"id\": \"39d2d219-e43e-4b82-be69-1664dca00500\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Left\",\n                    \"id\": \"58a243ec-18c3-4804-aad2-f5574ee9e50b\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Right\",\n                    \"id\": \"f1d6e972-0e13-4fbf-a484-80ad136ef787\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"82ccdd24-009c-46fa-91d0-265304aca3a5\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Camera\",\n            \"id\": \"98fb2fa8-0468-4c9d-b79c-a35c84751c0d\",\n            \"actions\": [\n                {\n                    \"name\": \"Rotate\",\n                    \"type\": \"Value\",\n                    \"id\": \"2c31abef-085d-4a20-9b56-0c7f8b46b485\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"1D Axis\",\n                    \"id\": \"c6eded54-dbdc-4c7f-8ff2-88a752298d5f\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Negative\",\n                    \"id\": \"b8006715-8924-447a-9db9-e86333af9ffd\",\n                    \"path\": \"<Keyboard>/r\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Positive\",\n                    \"id\": \"f4771ced-66e4-41c2-a1ad-c30b7b691021\",\n                    \"path\": \"<Keyboard>/t\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                }\n            ]\n        },\n        {\n            \"name\": \"UI\",\n            \"id\": \"04a16609-f161-4f6c-9a96-de10814a0c0c\",\n            \"actions\": [\n                {\n                    \"name\": \"Pause Menu\",\n                    \"type\": \"Value\",\n                    \"id\": \"e5084fa4-1dd0-4328-a2e2-a63807dd0d33\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Toggle UI\",\n                    \"type\": \"Value\",\n                    \"id\": \"a4728ea9-0368-4193-bf6a-a81965c10b10\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Open Chat\",\n                    \"type\": \"Value\",\n                    \"id\": \"66e08333-15cf-48f3-8aa4-6d37d6549baf\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"MousePosition\",\n                    \"type\": \"Value\",\n                    \"id\": \"d5475a7f-8112-4a81-8b0d-a008722fb90a\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"73325ce0-e59a-4156-83d7-2622f0670246\",\n                    \"path\": \"<Keyboard>/escape\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Pause Menu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c768d5ff-a138-4730-8463-1f02a1fdf519\",\n                    \"path\": \"<Keyboard>/f11\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Toggle UI\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"b8e601a3-5ad5-4177-9606-5751acab45f9\",\n                    \"path\": \"<Keyboard>/enter\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Open Chat\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"dc618575-08ed-47c5-ae6c-67280e705c62\",\n                    \"path\": \"<Mouse>/position\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"MousePosition\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Ability\",\n            \"id\": \"1469396b-71c5-4fae-8c25-b9dae81c1541\",\n            \"actions\": [\n                {\n                    \"name\": \"Attack\",\n                    \"type\": \"Value\",\n                    \"id\": \"3bcedc35-c64d-4ad5-b8ae-e8b1603cf36a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Abjure\",\n                    \"type\": \"Value\",\n                    \"id\": \"f705391a-aeab-49c2-a445-1609c53e518d\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Slide\",\n                    \"type\": \"Value\",\n                    \"id\": \"e9b5cfa6-9443-42c5-aee3-d1293663007f\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Ability 1\",\n                    \"type\": \"Value\",\n                    \"id\": \"b87e36d6-3651-45cd-8925-99af78f4b5e2\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Ability 2\",\n                    \"type\": \"Value\",\n                    \"id\": \"68585e82-774b-4903-a468-1d952b2198cd\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Ability 3\",\n                    \"type\": \"Value\",\n                    \"id\": \"102aa0bc-7d6f-4f73-9062-8d84b1585842\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"afabca47-ef8c-4b34-b891-ab238e6ca4bb\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Attack\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7e5944ac-0ae9-4592-a227-304a80283b71\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Abjure\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1cd452b9-79ab-4ac4-be0a-8d6292ddaf3c\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Slide\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"56a11839-c614-4324-826f-279675a3136e\",\n                    \"path\": \"<Mouse>/middleButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Ability 1\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7049a43f-60b1-4115-bfb4-c8a757abbb38\",\n                    \"path\": \"<Keyboard>/q\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Ability 2\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"75f465d8-ce17-4960-8ad1-c2be43e9efdf\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Ability 3\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"HouseBuilder\",\n            \"id\": \"dbb78c48-7c4e-48b0-b141-ed774708bc31\",\n            \"actions\": [\n                {\n                    \"name\": \"Toggle Grid\",\n                    \"type\": \"Value\",\n                    \"id\": \"f22320d4-0324-4aa6-b070-318b19add0f9\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Place Door\",\n                    \"type\": \"Value\",\n                    \"id\": \"9adea36e-9612-499e-85c1-74816f32c73a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Place Stairs\",\n                    \"type\": \"Value\",\n                    \"id\": \"824aeb46-d7a0-43b3-bc78-1d83e7e2d1dc\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Rotate\",\n                    \"type\": \"Value\",\n                    \"id\": \"9a0b5a81-48a3-4f32-b6cd-ab842e864abc\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Zoom\",\n                    \"type\": \"Value\",\n                    \"id\": \"aaf48835-5754-440d-a169-59919b817259\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Elevate Item\",\n                    \"type\": \"Value\",\n                    \"id\": \"70019572-452d-44d6-877c-eb4fa15ae7de\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Change Floor\",\n                    \"type\": \"Value\",\n                    \"id\": \"75166dfc-8134-4946-b379-a2f06018c4d0\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Recycle\",\n                    \"type\": \"Value\",\n                    \"id\": \"1b46c897-b1e5-42b1-bf14-5ac249741260\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Copy\",\n                    \"type\": \"Value\",\n                    \"id\": \"abd312a8-f869-4554-9490-3b99609e1973\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"a75a94c0-29fe-4101-b2b2-bac570e761d4\",\n                    \"path\": \"<Keyboard>/g\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Toggle Grid\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"cb3a86a2-a768-40b3-92a1-8382c0e4b586\",\n                    \"path\": \"<Keyboard>/c\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Place Door\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"1D Axis\",\n                    \"id\": \"26deb165-65b8-4aa7-84df-cf914194db94\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Negative\",\n                    \"id\": \"7fca6794-9620-4a19-9642-c9eec48b3f5a\",\n                    \"path\": \"<Keyboard>/n\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Positive\",\n                    \"id\": \"638b3bc6-fcac-4087-a550-5168cd169883\",\n                    \"path\": \"<Keyboard>/m\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Rotate\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"289853bb-b95c-45a4-8750-30feeb295be3\",\n                    \"path\": \"<Mouse>/scroll/y\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Zoom\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Button With One Modifier\",\n                    \"id\": \"2f14b5a6-7e44-42e1-978e-585c42d8e8a8\",\n                    \"path\": \"ButtonWithOneModifier\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Elevate Item\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"modifier\",\n                    \"id\": \"f093278f-7da0-4963-8628-fd28f07a91c1\",\n                    \"path\": \"<Keyboard>/leftShift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Elevate Item\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"button\",\n                    \"id\": \"3fd3c4c5-befd-4774-8cc0-5fa24002a91c\",\n                    \"path\": \"<Mouse>/scroll/y\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Elevate Item\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"1D Axis\",\n                    \"id\": \"0083cc73-cb0f-4b75-b6f9-df18a6dc23f2\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Change Floor\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"Negative\",\n                    \"id\": \"cf14c0d7-18df-46ab-bb5e-05fb135be430\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Change Floor\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Positive\",\n                    \"id\": \"ccb8ca9a-2dcc-4fd9-a581-87d48ece5cad\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Change Floor\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"961e6078-5433-431b-b8cf-323ca5b6d3bc\",\n                    \"path\": \"<Keyboard>/delete\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Recycle\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"44a4d2a3-23ea-4d67-8657-ab7e253750ec\",\n                    \"path\": \"<Keyboard>/leftCtrl\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Copy\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"65f9a698-2342-44bd-8aa8-fae71c70a4dd\",\n                    \"path\": \"<Keyboard>/v\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Place Stairs\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        },\n        {\n            \"name\": \"Interaction\",\n            \"id\": \"8a2c3526-1d9c-439e-b6c4-9d07d8cab007\",\n            \"actions\": [\n                {\n                    \"name\": \"Quick Action\",\n                    \"type\": \"Value\",\n                    \"id\": \"e6219524-cc6a-40dc-bb0f-62678c099cf5\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Item Action\",\n                    \"type\": \"Value\",\n                    \"id\": \"eb605c37-aad7-484f-a1d6-a5a27f64e64f\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"57728d09-33a1-4608-b025-f940c078d98a\",\n                    \"path\": \"<Keyboard>/leftShift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Quick Action\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8ca77d89-86be-4ceb-bb30-bf6d2cf6650a\",\n                    \"path\": \"<Keyboard>/leftCtrl\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Item Action\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        }\n    ],\n    \"controlSchemes\": []\n}");
		m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
		m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
		m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
		m_Camera = asset.FindActionMap("Camera", throwIfNotFound: true);
		m_Camera_Rotate = m_Camera.FindAction("Rotate", throwIfNotFound: true);
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_PauseMenu = m_UI.FindAction("Pause Menu", throwIfNotFound: true);
		m_UI_ToggleUI = m_UI.FindAction("Toggle UI", throwIfNotFound: true);
		m_UI_OpenChat = m_UI.FindAction("Open Chat", throwIfNotFound: true);
		m_UI_MousePosition = m_UI.FindAction("MousePosition", throwIfNotFound: true);
		m_Ability = asset.FindActionMap("Ability", throwIfNotFound: true);
		m_Ability_Attack = m_Ability.FindAction("Attack", throwIfNotFound: true);
		m_Ability_Abjure = m_Ability.FindAction("Abjure", throwIfNotFound: true);
		m_Ability_Slide = m_Ability.FindAction("Slide", throwIfNotFound: true);
		m_Ability_Ability1 = m_Ability.FindAction("Ability 1", throwIfNotFound: true);
		m_Ability_Ability2 = m_Ability.FindAction("Ability 2", throwIfNotFound: true);
		m_Ability_Ability3 = m_Ability.FindAction("Ability 3", throwIfNotFound: true);
		m_HouseBuilder = asset.FindActionMap("HouseBuilder", throwIfNotFound: true);
		m_HouseBuilder_ToggleGrid = m_HouseBuilder.FindAction("Toggle Grid", throwIfNotFound: true);
		m_HouseBuilder_PlaceDoor = m_HouseBuilder.FindAction("Place Door", throwIfNotFound: true);
		m_HouseBuilder_PlaceStairs = m_HouseBuilder.FindAction("Place Stairs", throwIfNotFound: true);
		m_HouseBuilder_Rotate = m_HouseBuilder.FindAction("Rotate", throwIfNotFound: true);
		m_HouseBuilder_Zoom = m_HouseBuilder.FindAction("Zoom", throwIfNotFound: true);
		m_HouseBuilder_ElevateItem = m_HouseBuilder.FindAction("Elevate Item", throwIfNotFound: true);
		m_HouseBuilder_ChangeFloor = m_HouseBuilder.FindAction("Change Floor", throwIfNotFound: true);
		m_HouseBuilder_Recycle = m_HouseBuilder.FindAction("Recycle", throwIfNotFound: true);
		m_HouseBuilder_Copy = m_HouseBuilder.FindAction("Copy", throwIfNotFound: true);
		m_Interaction = asset.FindActionMap("Interaction", throwIfNotFound: true);
		m_Interaction_QuickAction = m_Interaction.FindAction("Quick Action", throwIfNotFound: true);
		m_Interaction_ItemAction = m_Interaction.FindAction("Item Action", throwIfNotFound: true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}
}
