using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Alchemiter : ItemAcceptorMono
{
	private class TotemSlot : Shelf.ShelfSlot
	{
		private static Sprite limitSprite;

		private readonly Alchemiter parent;

		public TotemSlot(Alchemiter nparent)
			: base(nparent.cruxite)
		{
			parent = nparent;
			if (limitSprite == null)
			{
				limitSprite = ItemDownloader.GetSprite("Dowel");
			}
		}

		public override bool CanAcceptItem(Item newItem)
		{
			if (newItem is Totem)
			{
				return base.item == null;
			}
			return false;
		}

		public override bool AcceptItem(Item newItem)
		{
			if (base.AcceptItem(newItem))
			{
				Item makeItem = (newItem as Totem).makeItem;
				if (makeItem.IsEntry)
				{
					parent.gristCost = new GristCollection();
					if (NetworkServer.active)
					{
						parent.CreateObject();
					}
					Exile.StopAction(Exile.Action.artifact);
				}
				else
				{
					parent.gristCost = makeItem.GetCost();
				}
				return true;
			}
			return false;
		}

		public override bool CanRemoveItem()
		{
			return !parent.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Create");
		}

		public override void SetItemDirect(Item to)
		{
			base.SetItemDirect(to);
			if (to == null)
			{
				return;
			}
			Item makeItem = (to as Totem).makeItem;
			if (makeItem.IsEntry)
			{
				parent.gristCost = new GristCollection();
				if (NetworkServer.active)
				{
					parent.CreateObject();
				}
			}
			else
			{
				parent.gristCost = makeItem.GetCost();
			}
		}

		protected override void SetLocalOrientation(GameObject obj)
		{
			obj.transform.localPosition = new Vector3(0f, 0f, 1f / 3f);
			obj.transform.localRotation = Quaternion.identity;
		}

		public override string VisualUpdate(Image image)
		{
			parent.UpdateUI();
			needsVisualUpdate = false;
			if (base.item == null)
			{
				Item.ClearImage(image);
				image.sprite = limitSprite;
				image.color = new Color(1f, 1f, 1f, 0.3f);
				image.material = image.transform.parent.GetComponent<Image>().material;
				return "Totems only";
			}
			image.color = Color.white;
			image.material = image.transform.parent.GetComponent<Image>().defaultMaterial;
			base.item.ApplyToImage(image);
			return base.item.GetItemName();
		}
	}

	[SerializeField]
	private Transform makeTransform;

	[SerializeField]
	private RectTransform uiPrefab;

	private Transform cruxite;

	private GristCollection gristCost = new GristCollection();

	private Transform createButton;

	private Transform gristBox;

	private Transform gristText;

	protected override void SetSlots()
	{
		base.ItemSlot.Set(new TotemSlot(this));
	}

	protected override void Awake()
	{
		cruxite = base.transform.Find("Cruxite");
		base.ItemSlot.ServerAnimate = ServerCreateObject;
		base.ItemSlot.OnAnimate = delegate
		{
			GetComponent<Animator>().SetTrigger("Active");
		};
		base.Awake();
	}

	private bool CanCreateObject(Player player)
	{
		if (base.ItemSlot[0].item == null)
		{
			return false;
		}
		if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			return player.Grist.CanPay(gristCost);
		}
		return false;
	}

	private bool ServerCreateObject(bool value, NetworkConnectionToClient conn)
	{
		Player component = conn.identity.GetComponent<Player>();
		if (!CanCreateObject(component))
		{
			return false;
		}
		component.Grist.Subtract(gristCost);
		return true;
	}

	private void CreateObject()
	{
		if (CanCreateObject(Player.player))
		{
			base.ItemSlot.CmdAnimate(value: true);
		}
		else
		{
			SoundEffects.Instance.Nope();
		}
	}

	[ServerCallback]
	public void SpawnObject()
	{
		if (NetworkServer.active)
		{
			Item item = ((Totem)base.ItemSlot[0].item).makeItem;
			if (!item.IsEntry)
			{
				item = item.Copy();
			}
			item.SceneObject.transform.position = Vector3.zero;
			Vector3 position = makeTransform.position - ModelUtility.GetBottom(item.SceneObject);
			item.PutDown(base.transform.root.GetComponent<WorldArea>(), position);
			if (item.IsEntry)
			{
				base.ItemSlot[0].item.Destroy();
				base.ItemSlot[0].RemoveItem();
			}
		}
	}

	protected override void ApplyToUI()
	{
		RectTransform rectTransform = Object.Instantiate(uiPrefab, ItemAcceptorMono.invUI);
		gristBox = rectTransform.GetChild(0);
		gristText = gristBox.GetChild(0);
		rectTransform.GetChild(1).GetComponent<VisualItemSlot>().SetSlot(base.ItemSlot[0]);
		createButton = rectTransform.GetChild(2);
		createButton.GetComponent<Button>().onClick.AddListener(CreateObject);
		UpdateUI();
	}

	private void UpdateUI()
	{
		bool flag = false;
		bool flag2 = true;
		foreach (Transform item3 in gristBox)
		{
			if (item3 != gristText)
			{
				Object.Destroy(item3.gameObject);
			}
		}
		foreach (var item4 in gristCost)
		{
			int item = item4.index;
			int item2 = item4.value;
			flag = true;
			bool flag3 = Player.player.Grist[item] >= item2;
			flag2 = flag2 && flag3;
			Transform obj = Object.Instantiate(gristText, gristBox);
			obj.GetChild(0).GetComponent<GristImage>().SetGrist(item);
			Text component = obj.GetChild(1).GetComponent<Text>();
			component.color = (flag3 ? Color.black : Color.red);
			component.text = Sylladex.MetricFormat(item2);
			obj.gameObject.SetActive(value: true);
		}
		createButton.gameObject.SetActive(base.ItemSlot[0].item != null && flag2);
		gristBox.gameObject.SetActive(base.ItemSlot[0].item != null && flag);
	}
}
