using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemAcceptorCallback : MonoBehaviour, ItemAcceptor
{
	private readonly List<(Predicate<Item>, GameObject)> itemEvents = new List<(Predicate<Item>, GameObject)>();

	private Renderer renderer;

	public event Action OnReceiveItem;

	private void Awake()
	{
		renderer = GetComponentInChildren<Renderer>();
		base.enabled = itemEvents.Count != 0;
	}

	public void AddItemEvent(Predicate<Item> evt, Sprite sprite = null)
	{
		GameObject gameObject = null;
		if ((bool)sprite)
		{
			IOverhead componentInChildren = GetComponentInChildren<IOverhead>();
			if (componentInChildren != null)
			{
				gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("UIPrefabs/Speech Bubble"));
				componentInChildren.ShowAbove((RectTransform)gameObject.transform);
				gameObject.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
			}
		}
		itemEvents.Add((evt, gameObject));
		base.enabled = true;
	}

	public Rect GetItemRect()
	{
		return new Rect(MSPAOrthoController.main.WorldToScreenPoint(renderer.bounds.center) - new Vector3(32f, 40f), new Vector2(64f, 80f));
	}

	public void Hover(Item item)
	{
	}

	public bool IsActive(Item item)
	{
		return true;
	}

	protected void OnEnable()
	{
		AcceptorList.acceptors.Add(this);
	}

	protected void OnDisable()
	{
		AcceptorList.acceptors.Remove(this);
	}

	public bool AcceptItem(Item item)
	{
		bool result = false;
		for (int i = 0; i < itemEvents.Count; i++)
		{
			var (predicate, gameObject) = itemEvents[i];
			if (predicate(item))
			{
				itemEvents.RemoveAt(i);
				if ((bool)gameObject)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
				if (itemEvents.Count == 0)
				{
					base.enabled = false;
				}
				result = true;
				break;
			}
		}
		this.OnReceiveItem?.Invoke();
		return result;
	}
}
