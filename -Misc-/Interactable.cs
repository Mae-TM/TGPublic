using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Interactable : ClickOpen
{
	private static RectTransform interactUI;

	private List<IInteractableAction> options;

	private Vector2 guiPosition;

	private Color[][] normalColor;

	private Renderer[] objectRenderers;

	public bool renderersNeedReset;

	private void Start()
	{
		ResetRenderers();
		RefreshOptions();
	}

	public void ResetRenderers()
	{
		objectRenderers = GetComponentsInChildren<Renderer>();
		normalColor = new Color[objectRenderers.Length][];
		for (int i = 0; i < objectRenderers.Length; i++)
		{
			normalColor[i] = new Color[objectRenderers[i].materials.Length];
		}
	}

	private void ApplyToUI()
	{
		if (interactUI == null)
		{
			interactUI = (RectTransform)Player.Ui.Find("InteractUI");
		}
		interactUI.gameObject.SetActive(value: true);
		if (!(options[0] is PickupItemAction))
		{
			interactUI.transform.GetChild(0).gameObject.SetActive(value: false);
		}
		Button component = interactUI.transform.GetChild(1).GetComponent<Button>();
		if (options.Count != 1 || options[0] is PickupItemAction)
		{
			interactUI.GetComponent<Image>().enabled = true;
			for (int i = 0; i < options.Count; i++)
			{
				IInteractableAction interactableAction = options[i];
				PickupItemAction pickup = interactableAction as PickupItemAction;
				if ((object)pickup != null)
				{
					if (pickup.targetItem.SceneObject == base.gameObject)
					{
						ReplaceByStorage(ref pickup);
					}
					Image component2 = interactUI.transform.GetChild(0).GetComponent<Image>();
					component2.GetComponent<EventTrigger>().triggers.Clear();
					Item.ClearImage(component2);
					component2.gameObject.SetActive(value: true);
					pickup.targetItem.ApplyToImage(component2);
					component2.GetComponent<Tooltipped>().tooltip = pickup.Desc;
					EventTrigger.Entry entry = new EventTrigger.Entry();
					entry.eventID = EventTriggerType.PointerDown;
					entry.callback.AddListener(delegate
					{
						pickup.DragItem();
						Closed();
						ClickOpen.active = null;
					});
					component2.GetComponent<EventTrigger>().triggers.Add(entry);
				}
				float f = (float)Math.PI / 2f + (float)Math.PI * 2f * (float)i / (float)options.Count;
				Vector2 vector = 52f * new Vector2(Mathf.Cos(f), Mathf.Sin(f));
				MakeButton(component, interactableAction).transform.Translate(vector);
			}
		}
		else
		{
			interactUI.GetComponent<Image>().enabled = false;
			MakeButton(component, options[0]);
		}
	}

	private Button MakeButton(Button prefab, IInteractableAction action)
	{
		Button button = UnityEngine.Object.Instantiate(prefab, interactUI);
		button.name = action.Desc;
		Image component = button.transform.GetChild(0).GetComponent<Image>();
		action.ApplyToImage(component);
		button.onClick.AddListener(delegate
		{
			action.Execute();
			Closed();
			ClickOpen.active = null;
		});
		button.gameObject.SetActive(value: true);
		button.GetComponent<Tooltipped>().tooltip = action.Desc;
		return button;
	}

	private void ReplaceByStorage(ref PickupItemAction pickup)
	{
		ItemAcceptorMono[] componentsInChildren = GetComponentsInChildren<ItemAcceptorMono>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (ItemSlot item in componentsInChildren[i].ItemSlot)
			{
				if (item.item != null && item.item.SceneObject != null)
				{
					pickup = item.item.SceneObject.GetComponent<PickupItemAction>();
					pickup.notify = item;
					return;
				}
			}
		}
	}

	private void LateUpdate()
	{
		if (ClickOpen.active == this)
		{
			guiPosition = MSPAOrthoController.main.WorldToScreenPoint((objectRenderers.Length != 0) ? objectRenderers[0].bounds.center : base.transform.position);
			interactUI.position = new Vector2(guiPosition.x, guiPosition.y);
		}
	}

	public void RemoveOption(InteractableAction toRemove)
	{
		options?.Remove(toRemove);
		UnityEngine.Object.Destroy(toRemove);
	}

	public T AddOption<T>() where T : InteractableAction
	{
		T val = base.gameObject.AddComponent<T>();
		options?.Add(val);
		return val;
	}

	public void RefreshOptions()
	{
		options = new List<IInteractableAction>(GetComponents<IInteractableAction>());
	}

	protected override bool CanOpen()
	{
		if ((objectRenderers.Length == 0 || objectRenderers[0].enabled) && options.Count != 0)
		{
			return options[0].enabled;
		}
		return false;
	}

	protected override void Opened()
	{
		if (renderersNeedReset)
		{
			ResetRenderers();
		}
		for (int i = 0; i < objectRenderers.Length; i++)
		{
			if (!(objectRenderers[i] != null))
			{
				continue;
			}
			for (int j = 0; j < objectRenderers[i].materials.Length; j++)
			{
				if (objectRenderers[i].materials[j].HasProperty("_Color"))
				{
					normalColor[i][j] = objectRenderers[i].materials[j].color;
					objectRenderers[i].materials[j].color = Color.grey;
				}
			}
		}
		ApplyToUI();
	}

	protected override void Closed()
	{
		for (int i = 0; i < objectRenderers.Length; i++)
		{
			if (!(objectRenderers[i] != null))
			{
				continue;
			}
			for (int j = 0; j < objectRenderers[i].materials.Length; j++)
			{
				if (objectRenderers[i].materials[j].HasProperty("_Color"))
				{
					objectRenderers[i].materials[j].color = normalColor[i][j];
				}
			}
		}
		if (interactUI != null)
		{
			for (int k = 2; k < interactUI.childCount; k++)
			{
				UnityEngine.Object.Destroy(interactUI.GetChild(k).gameObject);
			}
			interactUI.gameObject.SetActive(value: false);
		}
	}

	protected override void QuickItemAction()
	{
		ItemAcceptorMono[] componentsInChildren = GetComponentsInChildren<ItemAcceptorMono>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (ItemSlot item in componentsInChildren[i].ItemSlot)
			{
				if (item.item != null && item.item.SceneObject != null)
				{
					if (item.CanRemoveItem() && Player.player.sylladex.AddItem(item.item))
					{
						item.RemoveItem();
					}
					return;
				}
			}
		}
		if (TryGetComponent<InteractableAction>(out var component))
		{
			component.Execute();
			return;
		}
		component = GetComponent<GetItemAction>();
		if (component != null)
		{
			component.Execute();
		}
	}

	protected override void QuickAction()
	{
		options[0].Execute();
	}
}
