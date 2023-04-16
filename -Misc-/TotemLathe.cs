using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class TotemLathe : ItemAcceptorMono
{
	private class CardSlot : ItemSlot
	{
		private static Sprite limitSprite;

		private readonly Action updateUI;

		private readonly Animator anim;

		public CardSlot(Action updateUI, Animator anim)
			: base(null)
		{
			this.updateUI = updateUI;
			this.anim = anim;
			if (limitSprite == null)
			{
				limitSprite = ItemDownloader.GetSprite("Punched");
			}
		}

		public override bool CanAcceptItem(Item newItem)
		{
			if (base.item == null)
			{
				return newItem is PunchCard;
			}
			return false;
		}

		public override bool CanRemoveItem()
		{
			return anim.GetCurrentAnimatorStateInfo(0).IsName("Idle");
		}

		public override string VisualUpdate(Image image)
		{
			updateUI();
			needsVisualUpdate = false;
			if (base.item == null)
			{
				Item.ClearImage(image);
				image.sprite = limitSprite;
				image.color = new Color(1f, 1f, 1f, 0.3f);
				image.material = image.transform.parent.GetComponent<Image>().material;
				return "Punched Cards only";
			}
			image.color = Color.white;
			image.material = image.transform.parent.GetComponent<Image>().defaultMaterial;
			base.item.ApplyToImage(image);
			return base.item.GetItemName();
		}
	}

	private class DowelSlot : Shelf.ShelfSlot
	{
		private static Sprite limitSprite;

		private readonly Action updateUI;

		private readonly Animator anim;

		public Color CruxiteColor { get; private set; }

		public DowelSlot(Action updateUI, Animator anim, Transform cruxite)
			: base(cruxite)
		{
			this.updateUI = updateUI;
			this.anim = anim;
			if (limitSprite == null)
			{
				limitSprite = ItemDownloader.GetSprite("Dowel");
			}
		}

		public override bool CanAcceptItem(Item newItem)
		{
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && base.item == null)
			{
				return newItem is Totem;
			}
			return false;
		}

		protected override void SetLocalOrientation(GameObject obj)
		{
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
			CruxiteColor = obj.GetComponent<MeshRenderer>().material.color;
		}

		public override string VisualUpdate(Image image)
		{
			updateUI();
			needsVisualUpdate = false;
			if (base.item == null)
			{
				Item.ClearImage(image);
				image.sprite = limitSprite;
				image.color = new Color(1f, 1f, 1f, 0.3f);
				image.material = image.transform.parent.GetComponent<Image>().material;
				return "Dowels only";
			}
			image.color = Color.white;
			image.material = image.transform.parent.GetComponent<Image>().defaultMaterial;
			base.item.ApplyToImage(image);
			return base.item.GetItemName();
		}
	}

	[SerializeField]
	private RectTransform latheUI;

	[SerializeField]
	private AudioSource audio;

	private GameObject cruxiteAnimated;

	private Transform carveButton;

	private Text carveText;

	private IEnumerator<AsyncOperation> recipeRoutine;

	private NormalItem result;

	private bool CanCarve
	{
		get
		{
			if (base.ItemSlot[0].item != null)
			{
				if (base.ItemSlot[1].item == null)
				{
					return base.ItemSlot[2].item != null;
				}
				return true;
			}
			return false;
		}
	}

	public event Action OnCarveArtifact;

	protected override void SetSlots()
	{
		Transform transform = base.transform.Find("Totem");
		Animator componentInChildren = GetComponentInChildren<Animator>();
		cruxiteAnimated = transform.gameObject.transform.GetChild(0).gameObject;
		base.ItemSlot.OnAnimate = ClientCarveTotem;
		base.ItemSlot.ServerAnimate = ServerCarveTotem;
		base.ItemSlot.Set(new DowelSlot(UpdateUI, componentInChildren, transform), new CardSlot(UpdateUI, componentInChildren), new CardSlot(UpdateUI, componentInChildren));
	}

	private void CarveTotem()
	{
		if (CanCarve)
		{
			base.ItemSlot.CmdAnimate(value: true);
		}
		else
		{
			SoundEffects.Instance.Nope();
		}
	}

	private void ClientCarveTotem(bool value)
	{
		GetComponentInParent<Animator>().SetBool("Carve", value: true);
		audio.Play();
		cruxiteAnimated.GetComponent<MeshRenderer>().material.color = ((DowelSlot)base.ItemSlot[0]).CruxiteColor;
		cruxiteAnimated.SetActive(value: true);
	}

	private bool ServerCarveTotem(bool value, NetworkConnectionToClient conn)
	{
		if (!CanCarve)
		{
			return false;
		}
		base.ItemSlot[0].item.Destroy();
		base.ItemSlot[0].RemoveItem();
		if (base.ItemSlot[1].item is PunchCard punchCard)
		{
			if (base.ItemSlot[2].item is PunchCard b)
			{
				recipeRoutine = PunchCard.AND(punchCard, b, delegate(NormalItem item)
				{
					result = item;
				});
				if (!recipeRoutine.MoveNext())
				{
					recipeRoutine = null;
				}
			}
			else if (punchCard.IsEntry)
			{
				result = punchCard.GetItem();
				base.ItemSlot[1].RemoveItem();
				this.OnCarveArtifact?.Invoke();
			}
			else
			{
				result = (NormalItem)punchCard.GetItem().Copy();
			}
		}
		else
		{
			PunchCard punchCard2 = (PunchCard)base.ItemSlot[2].item;
			if (punchCard2.IsEntry)
			{
				result = punchCard2.GetItem();
				base.ItemSlot[2].RemoveItem();
				this.OnCarveArtifact?.Invoke();
			}
			else
			{
				result = (NormalItem)punchCard2.GetItem().Copy();
			}
		}
		return true;
	}

	protected void Update()
	{
		if (recipeRoutine != null && recipeRoutine.Current.isDone && !recipeRoutine.MoveNext())
		{
			recipeRoutine = null;
		}
	}

	public void FinishCarving()
	{
		cruxiteAnimated.SetActive(value: false);
		if (!NetworkServer.active)
		{
			return;
		}
		if (recipeRoutine != null)
		{
			while (!recipeRoutine.Current.isDone || recipeRoutine.MoveNext())
			{
			}
		}
		recipeRoutine = null;
		base.ItemSlot[0].AcceptItem(new Totem(result, ((DowelSlot)base.ItemSlot[0]).CruxiteColor));
		result = null;
	}

	protected override void ApplyToUI()
	{
		RectTransform rectTransform = UnityEngine.Object.Instantiate(latheUI, ItemAcceptorMono.invUI);
		rectTransform.GetChild(0).GetComponent<VisualItemSlot>().SetSlot(base.ItemSlot[0]);
		rectTransform.GetChild(1).GetComponent<VisualItemSlot>().SetSlot(base.ItemSlot[1]);
		rectTransform.GetChild(2).GetComponent<VisualItemSlot>().SetSlot(base.ItemSlot[2]);
		carveButton = rectTransform.GetChild(3);
		carveButton.GetComponent<Button>().onClick.AddListener(CarveTotem);
		carveText = carveButton.GetChild(0).GetComponent<Text>();
		UpdateUI();
	}

	private void UpdateUI()
	{
		carveButton.gameObject.SetActive(base.ItemSlot[0].item is Totem totem && totem.IsDowel && (base.ItemSlot[1].item != null || base.ItemSlot[2].item != null));
		carveText.text = ((base.ItemSlot[1].item != null && base.ItemSlot[2].item != null) ? "&&" : "Lathe");
	}
}
