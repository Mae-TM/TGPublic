using System;
using UnityEngine;
using UnityEngine.UI;

public class DragCard : MonoBehaviour
{
	[SerializeField]
	private RectTransform ghostCard;

	private Item item;

	public Action<Item, bool> reject;

	public void SetItem(Item newItem, Material mat)
	{
		item = newItem;
		item.ApplyToImage(base.transform.GetChild(0).GetComponent<Image>());
		GetComponent<Image>().material = mat;
		base.transform.position = Input.mousePosition;
		base.gameObject.SetActive(value: true);
		KeyboardControl.Drag();
	}

	public Item GetItem()
	{
		return item;
	}

	private void Update()
	{
		base.transform.position = Input.mousePosition;
		ItemAcceptor itemAcceptor = null;
		float num = 65536f;
		Rect rect = default(Rect);
		foreach (ItemAcceptor acceptor in AcceptorList.acceptors)
		{
			if (acceptor.IsActive(item))
			{
				Rect itemRect = acceptor.GetItemRect();
				float sqrMagnitude = (itemRect.center - new Vector2(Input.mousePosition.x, Input.mousePosition.y)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					itemAcceptor = acceptor;
					rect = itemRect;
				}
			}
		}
		if (itemAcceptor == null)
		{
			ghostCard.gameObject.SetActive(value: false);
		}
		else
		{
			ghostCard.gameObject.SetActive(value: true);
			ghostCard.anchoredPosition = rect.center;
			ghostCard.sizeDelta = rect.size;
			if (rect.Contains(Input.mousePosition))
			{
				itemAcceptor.Hover(item);
			}
		}
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (itemAcceptor != null && rect.Contains(Input.mousePosition))
		{
			if (!itemAcceptor.AcceptItem(item))
			{
				reject(item, arg2: true);
			}
		}
		else
		{
			reject(item, arg2: false);
		}
		item = null;
		KeyboardControl.Undrag();
		base.gameObject.SetActive(value: false);
		ghostCard.gameObject.SetActive(value: false);
		Item.ClearImage(base.transform.GetChild(0).GetComponent<Image>());
	}
}
