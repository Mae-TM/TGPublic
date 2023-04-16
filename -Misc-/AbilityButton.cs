using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Image cooldownVisual;

	[SerializeField]
	private Text text;

	public bool? requiresMoving;

	public bool isRepeating;

	private Attacking.Ability ability;

	private InputAction binding;

	private InputAction.CallbackContext? repeatingContext;

	public event Action<Attackable, Vector3?> OnExecute;

	public void SetAppearance(string title, Color color)
	{
		text.text = ">" + title;
		text.color = color;
		ImageEffects.SetShiftColor(button.image.material, color);
	}

	public void SetAbility(Attacking.Ability newAbility)
	{
		if (ability != newAbility)
		{
			if (ability != null)
			{
				ability.OnExecute -= Activate;
			}
			newAbility.OnExecute += Activate;
			ability = newAbility;
			SetAppearance(ability.name, ability.color);
			base.name = "Ability " + ability.name;
		}
	}

	private void Activate()
	{
		base.gameObject.SetActive(value: true);
	}

	public void UnsetAbility()
	{
		ability = null;
		base.gameObject.SetActive(value: false);
	}

	public void SetBinding(InputAction newBinding)
	{
		if (binding != newBinding)
		{
			if (binding != null)
			{
				UnsetBinding();
			}
			binding = newBinding;
			binding.performed += DoAbility;
			binding.started += StartRepeating;
			binding.canceled += StopRepeating;
		}
	}

	private void UnsetBinding()
	{
		binding.performed -= DoAbility;
		binding.started -= StartRepeating;
		binding.canceled -= StopRepeating;
		binding = null;
	}

	private void Awake()
	{
		Image image = button.image;
		image.material = new Material(image.material);
	}

	private void OnDestroy()
	{
		UnsetBinding();
	}

	private void StartRepeating(InputAction.CallbackContext context)
	{
		if (isRepeating)
		{
			repeatingContext = context;
		}
	}

	private void StopRepeating(InputAction.CallbackContext context)
	{
		repeatingContext = null;
	}

	private void Update()
	{
		if (ability.IsAvailable())
		{
			base.gameObject.SetActive(value: false);
			if (repeatingContext.HasValue)
			{
				DoAbility(repeatingContext.Value);
			}
		}
		cooldownVisual.fillAmount = ability.cooldown / ability.maxCooldown;
	}

	private void DoAbility(InputAction.CallbackContext context)
	{
		if (ability == null || base.gameObject.activeSelf)
		{
			return;
		}
		PlayerController component = Player.player.GetComponent<PlayerController>();
		if (!component.reactToInput || (requiresMoving.HasValue && component.IsMoving != requiresMoving.Value))
		{
			return;
		}
		string text = context.control.device.name;
		if (text == "Mouse")
		{
			if (KeyboardControl.IsMouseBlocked() || KeyboardControl.IsItemAction)
			{
				return;
			}
			component.FaceMouse();
		}
		else if (text == "Keyboard" && KeyboardControl.IsKeyboardBlocked())
		{
			return;
		}
		if (KeyboardControl.Raycast(out var hitInfo))
		{
			Vector3 value = Player.player.RegionChild.Area.transform.InverseTransformPoint(hitInfo.point);
			this.OnExecute?.Invoke(hitInfo.transform.GetComponent<Attackable>(), value);
		}
		else
		{
			this.OnExecute?.Invoke(null, null);
		}
	}
}
