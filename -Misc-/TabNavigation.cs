using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			HandleHotkeySelect();
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			switch (base.transform.parent.name)
			{
			case "Login":
				GameObject.Find("Button").GetComponent<Button>().onClick.Invoke();
				break;
			case "CreateLobbyScreen":
				GameObject.Find("ButtonBar").transform.Find("ChangeBtn").GetComponent<Button>().onClick.Invoke();
				break;
			}
		}
	}

	private void HandleHotkeySelect()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (Exists(currentSelectedGameObject) && IsActiveInScene(currentSelectedGameObject) && currentSelectedGameObject.transform.parent == base.transform.parent)
		{
			Debug.Log(currentSelectedGameObject.transform.parent.name);
			Selectable component = currentSelectedGameObject.GetComponent<Selectable>();
			if (Exists(component))
			{
				SelectNextIfExists(IsShiftHeld() ? component.FindSelectableOnUp() : component.FindSelectableOnDown());
				return;
			}
		}
		SelectFirstHotkey();
	}

	private static bool Exists(Object selectedObject)
	{
		return (object)selectedObject != null;
	}

	private static bool IsActiveInScene(GameObject selectedObject)
	{
		return selectedObject.activeInHierarchy;
	}

	private static void SelectNextIfExists(Selectable nextSelection)
	{
		nextSelection?.Select();
	}

	private static bool IsShiftHeld()
	{
		if (!Input.GetKey(KeyCode.LeftShift))
		{
			return Input.GetKey(KeyCode.RightShift);
		}
		return true;
	}

	private static void SelectFirstHotkey()
	{
		Selectable.allSelectablesArray[0].Select();
	}
}
