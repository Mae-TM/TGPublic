using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VisualItemSlot : Tooltipped, ItemAcceptor, IPointerDownHandler, IEventSystemHandler
{
	private Image back;

	private Image image;

	protected ItemSlot slot;

	private void Awake()
	{
		back = GetComponent<Image>();
		image = base.transform.GetChild(0).GetComponent<Image>();
	}

	private void Update()
	{
		if (slot.needsVisualUpdate)
		{
			tooltip = slot.VisualUpdate(image);
		}
	}

	public void SetSlot(ItemSlot slot)
	{
		this.slot = slot;
		slot.needsVisualUpdate = true;
	}

	public void Hover(Item item)
	{
	}

	public Rect GetItemRect()
	{
		return new Rect(new Vector2(back.rectTransform.position.x, back.rectTransform.position.y) - Vector2.Scale(back.rectTransform.pivot, back.rectTransform.sizeDelta), back.rectTransform.sizeDelta);
	}

	public virtual bool AcceptItem(Item newItem)
	{
		return slot.AcceptItem(newItem);
	}

	protected virtual bool RemoveItem()
	{
		return slot.RemoveItem();
	}

	private void OnEnable()
	{
		AcceptorList.acceptors.Add(this);
	}

	protected override void OnDisable()
	{
		AcceptorList.acceptors.Remove(this);
		base.OnDisable();
	}

	public bool IsActive(Item item)
	{
		if (slot.CanAcceptItem(item))
		{
			return base.gameObject.activeInHierarchy;
		}
		return false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Item item = slot.item;
		if (item != null && Player.player.sylladex.GetDragItem() == null && eventData.button == PointerEventData.InputButton.Left && RemoveItem())
		{
			Player.player.sylladex.SetDragItem(item, back.material);
		}
	}
}
