using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class Modus : MonoBehaviour, ItemAcceptor
{
	protected class Card
	{
		public Item item;

		public Vector2 target;

		public Vector2 bigTarget;

		private readonly Image image;

		private readonly Image bigImage;

		public Card(Modus modus, Item item, Transform parent, Vector2 position, Transform bigParent, Vector2 bigPosition, short sorting = 1)
		{
			Card card = this;
			this.item = item;
			image = UnityEngine.Object.Instantiate(cardPrefab, parent);
			image.rectTransform.anchoredPosition = position;
			item.ApplyToImage(image.transform.GetChild(0).GetComponent<Image>());
			bigImage = UnityEngine.Object.Instantiate(bigCardPrefab, bigParent);
			bigImage.rectTransform.anchoredPosition = bigPosition;
			item.ApplyToImage(bigImage.transform.GetChild(0).GetComponent<Image>());
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener(delegate
			{
				modus.TryTakeItem(card);
			});
			image.GetComponent<EventTrigger>().triggers.Add(entry);
			bigImage.GetComponent<EventTrigger>().triggers.Add(entry);
			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(BringToFront);
			image.GetComponent<EventTrigger>().triggers.Add(entry);
			bigImage.GetComponent<EventTrigger>().triggers.Add(entry);
			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener(delegate
			{
				card.RestoreDepth(sorting);
			});
			image.GetComponent<EventTrigger>().triggers.Add(entry);
			bigImage.GetComponent<EventTrigger>().triggers.Add(entry);
			RestoreDepth(sorting);
			bigImage.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
			{
				modus.sylladex.OpenItemView(item);
			});
			Button button = bigImage.transform.GetChild(2).GetChild(0).GetComponent<Button>();
			InteractableAction[] components = item.SceneObject.GetComponents<InteractableAction>();
			foreach (InteractableAction act in components)
			{
				if (!(act is ConsumeAction) && !(act is OpenPesterchum) && !(act is BuildModeAction) && !(act is Book))
				{
					continue;
				}
				button.onClick.AddListener(delegate
				{
					act.Execute();
					if (item.ItemObject == null)
					{
						modus.RemoveItem(card);
					}
				});
				button.transform.GetChild(0).GetComponent<Text>().text = act.Desc;
				button = UnityEngine.Object.Instantiate(button, bigImage.transform.GetChild(2));
			}
			UnityEngine.Object.Destroy(button.gameObject);
		}

		public Card(Modus modus, Item item, Vector2 position, Card parent, Vector2 bigPosition)
			: this(modus, item, parent.image.transform.parent, position, parent.bigImage.transform.parent, bigPosition, 1)
		{
		}

		public void RestoreDepth(short sorting = 1)
		{
			int num = 0;
			foreach (Transform item in bigImage.transform.parent)
			{
				if ((item.localPosition.y - bigTarget.y) * (float)sorting < 0f)
				{
					bigImage.transform.SetSiblingIndex(num);
					break;
				}
				num++;
			}
		}

		public void BringToFront(BaseEventData data = null)
		{
			bigImage.transform.SetAsLastSibling();
		}

		public void Move()
		{
			image.rectTransform.anchoredPosition += (target - image.rectTransform.anchoredPosition) * 0.1f;
			bigImage.rectTransform.anchoredPosition += (bigTarget - bigImage.rectTransform.anchoredPosition) * 0.1f;
		}

		public void SetItemInteraction(bool active = true)
		{
			bigImage.transform.GetChild(1).gameObject.SetActive(active);
			bigImage.transform.GetChild(2).gameObject.SetActive(active);
			bigImage.color = (active ? Color.white : new Color(0.85f, 0.85f, 0.85f));
			image.color = (active ? Color.white : new Color(0.85f, 0.85f, 0.85f));
		}

		public Item Destroy()
		{
			UnityEngine.Object.Destroy(image.gameObject);
			UnityEngine.Object.Destroy(bigImage.gameObject);
			return item;
		}
	}

	protected const float CARD_SEPARATION = 59.2f;

	protected Vector2 separation = new Vector2(0f, 120f);

	protected Vector2 simplecardsize = new Vector2(64f, 80f);

	protected Vector2 complexcardsize = new Vector2(96f, 120f);

	public Sylladex sylladex;

	protected float maxWeight = -1f;

	protected float weight;

	protected int itemCapacity = 4;

	private Toggle autoPickup;

	private static GUIStyle buttonStyle;

	private static Image cardPrefab;

	private static Image bigCardPrefab;

	public virtual int ItemCapacity
	{
		get
		{
			return itemCapacity;
		}
		set
		{
			itemCapacity = value;
		}
	}

	public bool AutoPickup
	{
		get
		{
			if (autoPickup != null)
			{
				return autoPickup.isOn;
			}
			return false;
		}
	}

	protected void Awake()
	{
		if (cardPrefab == null)
		{
			cardPrefab = Resources.Load<Image>("UIPrefabs/ModusCard");
		}
		if (bigCardPrefab == null)
		{
			bigCardPrefab = Resources.Load<Image>("UIPrefabs/BigModusCard");
		}
		sylladex = Player.player.sylladex;
		Toggle[] componentsInChildren = sylladex.modusSettings.GetComponentsInChildren<Toggle>();
		componentsInChildren[0].onValueChanged.AddListener(delegate(bool to)
		{
			base.transform.GetChild(0).gameObject.SetActive(to);
		});
		componentsInChildren[1].onValueChanged.AddListener(delegate(bool to)
		{
			base.transform.GetChild(1).gameObject.SetActive(to);
		});
		autoPickup = componentsInChildren[2];
	}

	private void OnEnable()
	{
		AcceptorList.acceptors.Add(this);
	}

	private void OnDisable()
	{
		AcceptorList.acceptors.Remove(this);
	}

	private void FixedUpdate()
	{
		foreach (Card item in GetItemList())
		{
			item?.Move();
		}
	}

	protected virtual void UpdatePositions()
	{
		Vector2 vector = new Vector2(Mathf.Max(0f, (0f - separation.x) * (float)itemCapacity), Mathf.Min(0f, (0f - separation.y) * (float)itemCapacity));
		int num = 0;
		foreach (Card item in GetItemList())
		{
			if (item != null)
			{
				item.target = new Vector2(59.2f * (float)num, 0f);
				item.bigTarget = vector + separation * num;
				item.SetItemInteraction(IsRetrievable(item));
			}
			num++;
		}
	}

	protected virtual bool AddItemToModus(Item toAdd)
	{
		MonoBehaviour.print("No specific AddItemToModus function set!");
		return false;
	}

	protected void ThrowItem(Card card)
	{
		sylladex.PlayEjectCard();
		Item item = card.Destroy();
		if (item.SceneObject != null)
		{
			Transform transform = Player.player.transform;
			Vector3 vector = new Vector3(Mathf.Sign(transform.localScale.x), 0f, 0f);
			Vector3 forward = MSPAOrthoController.main.transform.forward;
			forward.y = 0f;
			vector = Quaternion.FromToRotation(Vector3.forward, forward) * vector;
			Vector3 spawnPos = ModelUtility.GetSpawnPos(item.SceneObject.transform, transform, vector);
			item.PutDown(Player.player.RegionChild.Area, spawnPos);
			if (item.SceneObject.TryGetComponent<Rigidbody>(out var component))
			{
				component.AddForce(vector * 20f, ForceMode.VelocityChange);
			}
		}
		else
		{
			MonoBehaviour.print("No gameObject set, item lost.");
		}
	}

	public void OpenSettings()
	{
		sylladex.modusSettings.gameObject.SetActive(value: true);
	}

	public virtual int CountItem(Item item)
	{
		return GetItemList().Count((Card card) => card?.item.Equals(item) ?? false);
	}

	protected virtual bool IsRetrievable(Card item)
	{
		return true;
	}

	protected virtual IEnumerable<Card> GetItemList()
	{
		return new Card[0];
	}

	public abstract ModusData Save();

	public abstract void Load(ModusData data);

	public virtual void Save(Stream stream)
	{
		HouseLoader.writeInt(itemCapacity, stream);
		HouseLoader.writeProtoBuf(stream, (from card in GetItemList()
			select card?.item?.Save()).ToArray());
	}

	protected virtual void Load(Item[] items)
	{
		throw new NotImplementedException("Load function for modus " + base.name + " not implemented!");
	}

	public virtual int GetAmount()
	{
		return 0;
	}

	private void TryTakeItem(Card item)
	{
		if (RemoveItem(item))
		{
			sylladex.SetDragItem(item.item);
		}
	}

	protected bool RemoveItem(Card item)
	{
		if (RemoveItemFromModus(item))
		{
			weight -= item.item.weight;
			item.Destroy();
			UpdatePositions();
			return true;
		}
		return false;
	}

	protected virtual bool RemoveItemFromModus(Card item)
	{
		MonoBehaviour.print("No specific RemoveItemFromModus function set!");
		return false;
	}

	protected void SetColor(Color color)
	{
		Material material = sylladex.modusSettings.GetComponent<Image>().material;
		Color.RGBToHSV(color, out var H, out var S, out var V);
		if (V > 1f)
		{
			V /= 255f;
		}
		material.SetFloat("_HueShift", H * 360f);
		material.SetFloat("_Sat", S);
		material.SetFloat("_Val", V);
		color = Color.HSVToRGB(H, S + 0.25f, V - 0.25f);
		Toggle[] componentsInChildren = sylladex.modusSettings.GetComponentsInChildren<Toggle>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetComponentInChildren<Text>().color = color;
		}
	}

	protected void SetIcon(string title)
	{
		sylladex.modusIcon.sprite = Resources.Load<Sprite>("Modi/" + title + "Modus");
	}

	protected Toggle AddToggle(string name, bool status, UnityAction<bool> call)
	{
		Toggle[] componentsInChildren = sylladex.modusSettings.GetComponentsInChildren<Toggle>();
		Toggle toggle = UnityEngine.Object.Instantiate(componentsInChildren[componentsInChildren.Length - 1], sylladex.modusSettings);
		toggle.onValueChanged.AddListener(call);
		toggle.name = name;
		toggle.GetComponentInChildren<Text>().text = name;
		toggle.isOn = status;
		if (componentsInChildren.Length == 3)
		{
			(toggle.transform as RectTransform).anchoredPosition = new Vector2(62f, 0f);
		}
		else
		{
			toggle.transform.localPosition -= new Vector3(0f, 24f);
		}
		return toggle;
	}

	public void Hover(Item item)
	{
	}

	public virtual Rect GetItemRect()
	{
		return new Rect(GetCardPosition(GetAmount()), complexcardsize);
	}

	protected Vector2 GetCardPosition(int index)
	{
		return (Vector2)base.transform.GetChild(1).position + new Vector2(Mathf.Max(0f, (0f - separation.x) * (float)itemCapacity), Mathf.Min(0f, (0f - separation.y) * (float)itemCapacity) - complexcardsize.y) + separation * index;
	}

	public virtual bool IsActive(Item item)
	{
		return base.gameObject.activeInHierarchy;
	}

	public bool AcceptItem(Item toAdd)
	{
		if (weight + toAdd.weight > maxWeight && maxWeight != -1f)
		{
			sylladex.PlayRejectCard();
			return false;
		}
		sylladex.PlayAcceptCard();
		if (AddItemToModus(toAdd))
		{
			weight += toAdd.weight;
			UpdatePositions();
			return true;
		}
		sylladex.PlayRejectCard();
		return false;
	}

	public void Eject()
	{
		foreach (Card item in GetItemList())
		{
			if (item != null)
			{
				ThrowItem(item);
			}
		}
		UnityEngine.Object.Destroy(this);
	}

	public void OnDestroy()
	{
		if (sylladex.modusSettings == null)
		{
			return;
		}
		uint num = 0u;
		Toggle[] componentsInChildren = sylladex.modusSettings.GetComponentsInChildren<Toggle>();
		foreach (Toggle toggle in componentsInChildren)
		{
			if (num < 3)
			{
				num++;
			}
			else
			{
				UnityEngine.Object.Destroy(toggle.gameObject);
			}
		}
	}

	protected Card MakeCard(Item item, int index, short sorting = 1)
	{
		if (cardPrefab == null)
		{
			cardPrefab = Resources.Load<Image>("UIPrefabs/ModusCard");
			bigCardPrefab = Resources.Load<Image>("UIPrefabs/BigModusCard");
		}
		return new Card(bigPosition: new Vector2(Mathf.Max(0f, (0f - separation.x) * (float)itemCapacity), Mathf.Min(0f, (0f - separation.y) * (float)itemCapacity)) + separation * index, modus: this, item: item, parent: base.transform.GetChild(0), position: new Vector2(59.2f * (float)index, 0f), bigParent: base.transform.GetChild(1), sorting: sorting);
	}
}
