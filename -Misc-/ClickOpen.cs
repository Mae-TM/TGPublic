using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ClickOpen : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	private class Child : MonoBehaviour
	{
		public ClickOpen parent;
	}

	private class PointerChild : Child, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			parent.OnPointerClick(eventData);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			parent.OnPointerEnter(eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			parent.OnPointerExit(eventData);
		}
	}

	private class VisibleChild : Child
	{
		public void OnBecameVisible()
		{
			parent.AddVisibility();
		}

		public void OnBecameInvisible()
		{
			parent.RemoveVisibility();
		}
	}

	public static ClickOpen active;

	public static bool hovering;

	private static Texture2D cursorTexture;

	private int visibility;

	protected virtual bool CanOpen()
	{
		return true;
	}

	protected abstract void Opened();

	protected abstract void Closed();

	protected abstract void QuickItemAction();

	protected abstract void QuickAction();

	protected bool IsPlayerClose()
	{
		return Vector3.SqrMagnitude(Player.player.transform.position - base.transform.position) < 36f;
	}

	protected virtual void Awake()
	{
		if (cursorTexture == null)
		{
			cursorTexture = Resources.Load<Texture2D>("InteractCursor");
		}
		AddChildren<Collider, PointerChild>();
		Renderer[] array = AddChildren<Renderer, VisibleChild>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isVisible)
			{
				AddVisibility();
			}
		}
	}

	private TComponent[] AddChildren<TComponent, TChild>() where TComponent : Component where TChild : Child
	{
		TComponent[] componentsInChildren = GetComponentsInChildren<TComponent>();
		TComponent[] array = componentsInChildren;
		foreach (TComponent val in array)
		{
			if (val.gameObject != base.gameObject && (!val.gameObject.TryGetComponent<TChild>(out var component) || component.parent != this))
			{
				val.gameObject.AddComponent<TChild>().parent = this;
			}
		}
		return componentsInChildren;
	}

	protected virtual void OnDestroy()
	{
		Child[] componentsInChildren = GetComponentsInChildren<Child>();
		foreach (Child child in componentsInChildren)
		{
			if (child.parent == this)
			{
				Object.Destroy(child);
			}
		}
		Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			if (componentsInChildren2[i].isVisible)
			{
				RemoveVisibility();
			}
		}
	}

	private IEnumerator PollForClose()
	{
		while (active == this && BuildExploreSwitcher.IsExploring)
		{
			if (Input.GetMouseButtonDown(0) && !KeyboardControl.IsDragging && !KeyboardControl.IsHovering)
			{
				Close();
			}
			if (!IsPlayerClose())
			{
				Close();
			}
			yield return null;
		}
	}

	protected virtual void OnBecameVisible()
	{
	}

	protected virtual void OnBecameInvisible()
	{
		if (BuildExploreSwitcher.IsExploring)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			hovering = false;
		}
		if (active == this)
		{
			Close();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (BuildExploreSwitcher.IsExploring && base.enabled && !KeyboardControl.IsMouseBlocked(ignoreClickOpen: true) && IsPlayerClose() && active != this && CanOpen())
		{
			if (KeyboardControl.IsItemAction)
			{
				QuickItemAction();
				return;
			}
			if (KeyboardControl.IsQuickAction)
			{
				QuickAction();
				return;
			}
			Open();
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			hovering = false;
		}
	}

	protected void Open()
	{
		if (active != null)
		{
			if (active.gameObject == base.gameObject)
			{
				return;
			}
			active.Closed();
		}
		active = this;
		Opened();
		StartCoroutine(PollForClose());
	}

	protected void Close()
	{
		active = null;
		Closed();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (BuildExploreSwitcher.IsExploring)
		{
			if (base.enabled && active != this && IsPlayerClose() && !KeyboardControl.IsDragging && !Input.GetMouseButton(0) && CanOpen())
			{
				Cursor.SetCursor(cursorTexture, new Vector2(29f, 3f), CursorMode.Auto);
				hovering = true;
			}
			else
			{
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				hovering = false;
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (BuildExploreSwitcher.IsExploring)
		{
			hovering = false;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}

	private void AddVisibility()
	{
		if (visibility == 0)
		{
			OnBecameVisible();
		}
		visibility++;
	}

	private void RemoveVisibility()
	{
		visibility--;
		if (visibility == 0)
		{
			OnBecameInvisible();
		}
	}
}
