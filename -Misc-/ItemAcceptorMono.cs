using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class ItemAcceptorMono : ClickOpen, ItemAcceptor
{
	private static Camera cam;

	protected static RectTransform invUI;

	protected Renderer renderer;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	protected bool ejectItems;

	[field: SerializeField]
	public ItemSlots ItemSlot { get; private set; }

	protected abstract void SetSlots();

	private void OnValidate()
	{
		if (!ItemSlot)
		{
			NetworkIdentity[] componentsInParent = GetComponentsInParent<NetworkIdentity>(includeInactive: true);
			ItemSlot = componentsInParent[0].gameObject.AddComponent<ItemSlots>();
		}
	}

	protected override void Awake()
	{
		if (cam == null)
		{
			cam = MSPAOrthoController.main;
		}
		renderer = GetComponentInChildren<Renderer>();
		if (invUI == null)
		{
			invUI = Player.Ui.Find("InventoryUI") as RectTransform;
		}
		if (!ItemSlot.IsSet)
		{
			SetSlots();
		}
		if ((bool)anim)
		{
			ItemSlot.OnAnimate = delegate(bool value)
			{
				anim.SetBool("Open", value);
			};
		}
		base.Awake();
	}

	protected override void OnBecameVisible()
	{
		AcceptorList.acceptors.Add(this);
		base.OnBecameVisible();
	}

	protected override void OnBecameInvisible()
	{
		AcceptorList.acceptors.Remove(this);
		base.OnBecameInvisible();
	}

	private IEnumerator UpdateUiPosition()
	{
		while (ClickOpen.active == this)
		{
			invUI.position = cam.WorldToScreenPoint(renderer.bounds.center);
			yield return null;
		}
	}

	public Rect GetItemRect()
	{
		return new Rect(cam.WorldToScreenPoint(renderer.bounds.center) - new Vector3(32f, 40f), new Vector2(64f, 80f));
	}

	public void Hover(Item item)
	{
		if (!ejectItems)
		{
			Open();
		}
	}

	protected override void Opened()
	{
		if (anim == null)
		{
			ShowUI();
		}
		else
		{
			ItemSlot.CmdAnimate(value: true);
		}
	}

	public void ShowUI()
	{
		if (ejectItems)
		{
			EjectItems();
		}
		else if (ClickOpen.active == this)
		{
			ApplyToUI();
			invUI.gameObject.SetActive(value: true);
			StartCoroutine(UpdateUiPosition());
		}
	}

	private void EjectItems()
	{
		if (NetworkServer.active)
		{
			NavMeshObstacle componentInParent = GetComponentInParent<NavMeshObstacle>();
			WorldArea component = base.transform.root.GetComponent<WorldArea>();
			Bounds sourceBounds = new Bounds(base.transform.TransformPoint(componentInParent.center), base.transform.TransformVector(componentInParent.size));
			Vector3 forward = base.transform.forward;
			foreach (ItemSlot item2 in ItemSlot)
			{
				Item item = item2.item;
				if (item2.RemoveItem())
				{
					item.PutDown(component, ModelUtility.GetSpawnPos(item.SceneObject.transform, sourceBounds, forward));
				}
			}
		}
		Close();
		Object.Destroy(this);
	}

	protected override void Closed()
	{
		foreach (Transform item in invUI)
		{
			Object.Destroy(item.gameObject);
		}
		invUI.GetComponent<Image>().enabled = false;
		invUI.gameObject.SetActive(value: false);
		if (anim != null)
		{
			ItemSlot.CmdAnimate(value: false);
		}
	}

	protected virtual void ApplyToUI()
	{
		if (ItemSlot.Length == 1)
		{
			Object.Instantiate(Resources.Load<Transform>("UIPrefabs/ItemSlot"), invUI).GetComponent<VisualItemSlot>().SetSlot(ItemSlot[0]);
			return;
		}
		RectTransform rectTransform = Resources.Load<RectTransform>("UIPrefabs/ItemSlot");
		int num = Mathf.CeilToInt(Mathf.Sqrt(ItemSlot.Length));
		int num2 = Mathf.CeilToInt((float)ItemSlot.Length / (float)num);
		float width = rectTransform.rect.width;
		float height = rectTransform.rect.height;
		invUI.GetComponent<Image>().enabled = true;
		invUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * (float)num);
		invUI.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * (float)num2);
		for (int i = 0; i < ItemSlot.Length; i++)
		{
			RectTransform rectTransform2 = Object.Instantiate(rectTransform, invUI, worldPositionStays: false);
			rectTransform2.localPosition = new Vector3(((float)(i % num) - (float)(num - 1) / 2f) * width, ((float)(num2 - 1) / 2f - (float)(i / num)) * height, 0f);
			rectTransform2.GetComponent<VisualItemSlot>().SetSlot(ItemSlot[i]);
		}
	}

	public virtual bool AcceptItem(Item item)
	{
		if (ejectItems)
		{
			return false;
		}
		Open();
		return ItemSlot.Any((ItemSlot slot) => slot?.AcceptItem(item) ?? false);
	}

	public bool IsActive(Item item)
	{
		if (base.isActiveAndEnabled && ClickOpen.active != this && Visibility.Get(base.gameObject))
		{
			return IsPlayerClose();
		}
		return false;
	}

	protected override void QuickItemAction()
	{
		if (ejectItems)
		{
			return;
		}
		foreach (ItemSlot item2 in ItemSlot)
		{
			Item item = item2?.item;
			if (item != null && item2.RemoveItem())
			{
				Player.player.sylladex.SetDragItem(item);
				break;
			}
		}
	}

	protected override void QuickAction()
	{
		QuickItemAction();
	}
}
