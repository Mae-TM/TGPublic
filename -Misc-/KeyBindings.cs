using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindings : MonoBehaviour
{
	[SerializeField]
	private Transform template;

	private void Start()
	{
		foreach (InputActionMap map in KeyboardControl.Maps)
		{
			Transform transform = Object.Instantiate(template, template.parent);
			transform.name = map.name;
			transform.GetChild(0).GetComponent<Text>().text = map.name;
			RectTransform rectTransform = null;
			foreach (InputAction item in map)
			{
				rectTransform = ((!(rectTransform == null)) ? Object.Instantiate(rectTransform, transform) : (transform.GetChild(1) as RectTransform));
				rectTransform.name = item.ToString();
				rectTransform.GetChild(0).GetComponent<Text>().text = item.name;
				Transform child;
				if (item.controls.Count == 4)
				{
					child = rectTransform.GetChild(2);
					rectTransform.GetChild(1).gameObject.SetActive(value: false);
					child.gameObject.SetActive(value: true);
					rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 47f);
				}
				else
				{
					child = rectTransform.GetChild(1);
					rectTransform.GetChild(2).gameObject.SetActive(value: false);
					child.gameObject.SetActive(value: true);
					rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 24f);
				}
				UpdateBindingText(item, child);
			}
		}
		Object.Destroy(template.gameObject);
	}

	private void SetBinding(InputAction action, Transform keys, int controlIndex, int bindingIndex)
	{
		bool enabled = action.actionMap.enabled;
		if (enabled)
		{
			action.actionMap.Disable();
		}
		InputActionRebindingExtensions.RebindingOperation rebindingOperation = action.PerformInteractiveRebinding();
		rebindingOperation.WithTargetBinding(bindingIndex);
		rebindingOperation.WithControlsExcluding("/Mouse/scroll");
		rebindingOperation.WithControlsExcluding("/Mouse/delta");
		rebindingOperation.WithControlsExcluding("/Mouse/position");
		rebindingOperation.OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation rb)
		{
			OnBindingDone(rb, keys, enabled);
		});
		rebindingOperation.OnCancel(delegate(InputActionRebindingExtensions.RebindingOperation rb)
		{
			OnBindingDone(rb, keys, enabled);
		});
		rebindingOperation.Start();
		keys.GetChild(controlIndex).GetChild(0).GetComponent<Text>()
			.text = "...";
		keys.GetChild(controlIndex).GetComponent<Button>().interactable = false;
	}

	private void OnBindingDone(InputActionRebindingExtensions.RebindingOperation rebind, Transform keys, bool enable)
	{
		UpdateBindingText(rebind.action, keys);
		if (enable)
		{
			rebind.action.actionMap.Enable();
		}
		rebind.Dispose();
	}

	private void UpdateBindingText(InputAction action, Transform keys)
	{
		int num = 0;
		for (int i = 0; i < action.bindings.Count; i++)
		{
			if (!action.bindings[i].isComposite)
			{
				Transform transform = ((num >= keys.childCount) ? Object.Instantiate(keys.GetChild(0), keys) : keys.GetChild(num));
				Button component = transform.GetComponent<Button>();
				component.interactable = true;
				Button.ButtonClickedEvent onClick = component.onClick;
				onClick.RemoveAllListeners();
				int index1 = num;
				int index2 = i;
				onClick.AddListener(delegate
				{
					SetBinding(action, keys, index1, index2);
				});
				transform.GetChild(0).GetComponent<Text>().text = action.controls[num].displayName;
				num++;
			}
		}
		for (int j = action.controls.Count; j < keys.childCount; j++)
		{
			Object.DestroyImmediate(keys.GetChild(j).gameObject);
		}
	}
}
