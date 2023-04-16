using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Specibus : MonoBehaviour, ItemAcceptor, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[ProtoContract]
	public struct Data
	{
		[ProtoMember(1)]
		public int size;

		[ProtoMember(2)]
		public HouseData.Item[] weapons;
	}

	private class ToolbarSlot : ItemSlot
	{
		private readonly Specibus specibus;

		private readonly Text powerText;

		private readonly Text speedText;

		public ToolbarSlot(Specibus specibus, Item it, Text powerText, Text speedText)
			: base(it)
		{
			this.specibus = specibus;
			this.powerText = powerText;
			this.speedText = speedText;
		}

		public override bool AcceptItem(Item newItem)
		{
			if (specibus.AddWeapon(newItem, out var _, out var _))
			{
				return base.AcceptItem(newItem);
			}
			return false;
		}

		public override bool RemoveItem()
		{
			specibus.RemoveWeapon((NormalItem)base.item, removeFromSlot: false);
			return base.RemoveItem();
		}

		public override string VisualUpdate(Image image)
		{
			needsVisualUpdate = false;
			if (base.item == null)
			{
				Item.ClearImage(image);
				image.enabled = false;
				powerText.enabled = false;
				speedText.enabled = false;
				return null;
			}
			base.item.ApplyToImage(image);
			image.enabled = true;
			powerText.enabled = true;
			speedText.enabled = true;
			powerText.text = Sylladex.MetricFormat(((NormalItem)base.item).Power);
			speedText.text = Sylladex.MetricFormat(((NormalItem)base.item).Speed);
			return base.item.GetItemName();
		}
	}

	public Sylladex sylladex;

	public AudioClip clipEquip;

	private int size;

	private int aKind;

	private int aIndex;

	private Rect rect;

	private readonly List<List<NormalItem>> weapons = new List<List<NormalItem>>();

	private readonly List<WeaponKind> weaponKinds = new List<WeaponKind>();

	[SerializeField]
	private ArmorSlot[] armor = new ArmorSlot[5];

	private GameObject kindCard;

	private GameObject weaponCard;

	private readonly List<GameObject> kindCards = new List<GameObject>();

	private readonly List<List<GameObject>> weaponCards = new List<List<GameObject>>();

	[SerializeField]
	private GameObject abstratusCard;

	private readonly List<GameObject> abstratusCards = new List<GameObject>();

	[SerializeField]
	private VisualItemSlot[] toolbar;

	private ToolbarSlot[] toolbarSlots;

	[SerializeField]
	private Transform weaponShowcase;

	public int Size
	{
		get
		{
			return size;
		}
		set
		{
			if (value < size)
			{
				Debug.LogWarning("Specibus size set to a smaller value?");
				return;
			}
			for (int i = size; i < value; i++)
			{
				weaponKinds.Add(WeaponKind.None);
				weapons.Add(new List<NormalItem>());
				weaponCards.Add(new List<GameObject>());
			}
			size = value;
		}
	}

	private void Start()
	{
		AcceptorList.acceptors.Add(this);
		kindCard = base.transform.Find("Weaponkind").gameObject;
		weaponCard = base.transform.Find("Weapon").gameObject;
		toolbarSlots = new ToolbarSlot[toolbar.Length];
		for (int i = 0; i < toolbar.Length; i++)
		{
			toolbarSlots[i] = new ToolbarSlot(this, null, toolbar[i].transform.GetChild(1).GetChild(0).GetComponent<Text>(), toolbar[i].transform.GetChild(2).GetChild(0).GetComponent<Text>());
			toolbar[i].SetSlot(toolbarSlots[i]);
		}
		if (sylladex != null && Player.loadedPlayerData == null)
		{
			Size = 4;
		}
	}

	public void InitArmor(NormalItem[] clothes)
	{
		for (int i = 0; i < 5; i++)
		{
			armor[i].Init(clothes[i]);
		}
	}

	private void Update()
	{
		for (int i = 0; i < toolbar.Length; i++)
		{
			if (toolbarSlots[i].item != null && Input.GetKeyUp((KeyCode)(49 + i)))
			{
				SetActive((NormalItem)toolbarSlots[i].item);
			}
		}
		if (aKind < weapons.Count && weapons[aKind].Count != 0)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			int num = aIndex;
			aIndex += (int)(axis * 10f);
			while (aIndex >= weapons[aKind].Count)
			{
				aIndex -= weapons[aKind].Count;
			}
			while (aIndex < 0)
			{
				aIndex += weapons[aKind].Count;
			}
			if (num != aIndex)
			{
				UpdateWeapon();
			}
		}
	}

	private void UpdateWeapon()
	{
		NormalItem normalItem = ((aIndex < weapons[aKind].Count) ? weapons[aKind][aIndex] : null);
		if (!Player.player.SetWeapon(normalItem))
		{
			return;
		}
		Item.ClearImage(base.transform.GetChild(3).GetComponent<Image>());
		Item.ClearImage(weaponShowcase.GetChild(0).GetComponent<Image>());
		if (normalItem == null)
		{
			base.transform.GetChild(0).gameObject.SetActive(value: false);
			base.transform.GetChild(1).gameObject.SetActive(value: false);
			base.transform.GetChild(2).gameObject.SetActive(value: false);
			base.transform.GetChild(3).gameObject.SetActive(value: false);
			weaponShowcase.gameObject.SetActive(value: false);
			return;
		}
		base.transform.GetChild(0).gameObject.SetActive(value: true);
		base.transform.GetChild(0).GetChild(0).GetComponent<Text>()
			.text = normalItem.GetItemName();
		weaponShowcase.GetChild(3).GetChild(0).GetChild(0)
			.GetComponent<Text>()
			.text = normalItem.name;
		base.transform.GetChild(1).gameObject.SetActive(value: true);
		base.transform.GetChild(1).GetChild(0).GetComponent<Text>()
			.text = Sylladex.MetricFormat(normalItem.Power);
		weaponShowcase.GetChild(1).GetChild(1).GetComponent<Text>()
			.text = Sylladex.MetricFormat(normalItem.Power);
		base.transform.GetChild(2).gameObject.SetActive(value: true);
		base.transform.GetChild(2).GetChild(0).GetComponent<Text>()
			.text = Sylladex.MetricFormat(normalItem.Speed);
		weaponShowcase.GetChild(1).GetChild(3).GetComponent<Text>()
			.text = Sylladex.MetricFormat(normalItem.Speed);
		base.transform.GetChild(3).gameObject.SetActive(value: true);
		normalItem.ApplyToImage(base.transform.GetChild(3).GetComponent<Image>());
		normalItem.ApplyToImage(weaponShowcase.GetChild(0).GetComponent<Image>());
		weaponShowcase.GetChild(2).GetChild(0).GetComponent<Image>()
			.sprite = ItemDownloader.GetWeaponKind(normalItem.weaponKind[0]);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NormalItem.Tag tag in normalItem.GetTags())
		{
			stringBuilder.AppendLine(tag.ToString());
		}
		weaponShowcase.GetChild(3).GetChild(1).GetComponent<Text>()
			.text = stringBuilder.ToString();
		weaponShowcase.gameObject.SetActive(value: true);
	}

	private void OnRectTransformDimensionsChange()
	{
		rect = new Rect(new Vector2(base.transform.position.x, base.transform.position.y) - (base.transform as RectTransform).sizeDelta, (base.transform as RectTransform).sizeDelta);
	}

	public void Hover(Item item)
	{
	}

	public Rect GetItemRect()
	{
		return rect;
	}

	public bool IsActive(Item itemBase)
	{
		if (itemBase is NormalItem normalItem && base.gameObject.activeInHierarchy)
		{
			if (normalItem.weaponKind.Length == 0)
			{
				return normalItem.armor != ArmorKind.None;
			}
			return true;
		}
		return false;
	}

	public bool AcceptItem(Item item)
	{
		if (AddWeapon(item, out var kind, out var index))
		{
			if (kind != -1)
			{
				aKind = kind;
				aIndex = index;
				UpdateWeapon();
				sylladex.PlaySoundEffect(clipEquip);
			}
			return true;
		}
		return false;
	}

	private bool AddWeapon(Item itemBase, out int kind, out int index)
	{
		if (!(itemBase is NormalItem normalItem))
		{
			index = -1;
			kind = -1;
			return false;
		}
		if (normalItem.weaponKind.Length != 0)
		{
			List<WeaponKind> list = new List<WeaponKind>(normalItem.weaponKind.Intersect(weaponKinds));
			if (list.Count == 0)
			{
				kind = weaponKinds.IndexOf(WeaponKind.None);
				if (kind == -1)
				{
					index = -1;
					return false;
				}
				weaponKinds[kind] = normalItem.weaponKind[0];
				weapons[kind].Add(normalItem);
				index = weapons[kind].Count - 1;
				AddKindCard(kind);
				AddWeaponCard(normalItem, kind);
				return true;
			}
			kind = weaponKinds.IndexOf(list[0]);
			weapons[kind].Add(normalItem);
			index = weapons[kind].Count - 1;
			AddWeaponCard(normalItem, kind);
			return true;
		}
		if (normalItem.armor != ArmorKind.None && armor[(int)normalItem.armor].AcceptItem(normalItem))
		{
			index = -1;
			kind = -1;
			return true;
		}
		index = -1;
		kind = -1;
		return false;
	}

	private void AddKindCard(int index)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(kindCard, base.transform);
		gameObject.name = weaponKinds[index].ToString() + "Card";
		gameObject.transform.GetChild(0).GetComponent<Image>().sprite = ItemDownloader.GetWeaponKind(weaponKinds[index]);
		gameObject.GetComponent<Button>().onClick.AddListener(delegate
		{
			aKind = index;
			UpdateWeapon();
		});
		gameObject.AddComponent<Tooltipped>().tooltip = weaponKinds[index].ToString() + "kind";
		gameObject.transform.Translate(0f, -80 * kindCards.Count, 0f);
		kindCards.Add(gameObject);
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener(delegate
		{
			ExpandWeaponKind(index);
		});
		gameObject.GetComponent<EventTrigger>().triggers.Add(entry);
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerExit;
		entry.callback.AddListener(delegate
		{
			RetractWeaponKind(index);
		});
		gameObject.GetComponent<EventTrigger>().triggers.Add(entry);
		Transform parent = abstratusCard.transform.parent;
		if (index > parent.childCount - 3)
		{
			throw new Exception("Can't hold all these specibus cards! No empty sleeves left.");
		}
		Transform child;
		do
		{
			child = parent.GetChild(UnityEngine.Random.Range(0, parent.childCount - 3));
		}
		while (child.childCount != 0);
		gameObject = UnityEngine.Object.Instantiate(abstratusCard, child);
		gameObject.name = weaponKinds[index].ToString() + "kind";
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.GetChild(0).GetComponent<Image>().sprite = ItemDownloader.GetWeaponKind(weaponKinds[index]);
		gameObject.transform.GetChild(2).GetComponent<Text>().text = weaponKinds[index].ToString().ToLower() + "kind";
		gameObject.SetActive(value: true);
		abstratusCards.Add(gameObject);
	}

	private void AddWeaponCard(NormalItem item, int kind)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(weaponCard, kindCards[kind].transform);
		gameObject.name = item?.ToString() + " card";
		gameObject.transform.Translate(-64 * weaponCards[kind].Count, 0f, 0f);
		item.ApplyToImage(gameObject.transform.GetChild(0).GetComponent<Image>());
		gameObject.transform.GetChild(1).GetChild(0).GetComponent<Text>()
			.text = Sylladex.MetricFormat(item.Power);
		gameObject.transform.GetChild(2).GetChild(0).GetComponent<Text>()
			.text = Sylladex.MetricFormat(item.Speed);
		weaponCards[kind].Add(gameObject);
		Transform child = abstratusCards[kind].transform.GetChild(1).GetChild(0).GetChild(0);
		Transform cardlet = UnityEngine.Object.Instantiate(child.GetChild(0), child);
		item.ApplyToImage(cardlet.GetChild(0).GetComponent<Image>());
		cardlet.gameObject.SetActive(value: true);
		gameObject.GetComponent<Button>().onClick.AddListener(delegate
		{
			aKind = kind;
			aIndex = cardlet.GetSiblingIndex() - 1;
			UpdateWeapon();
		});
		cardlet.GetComponent<Button>().onClick.AddListener(delegate
		{
			aKind = kind;
			aIndex = cardlet.GetSiblingIndex() - 1;
			UpdateWeapon();
		});
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.BeginDrag;
		entry.callback.AddListener(delegate
		{
			PickUpCard(kind, cardlet.GetSiblingIndex() - 1);
		});
		gameObject.GetComponent<EventTrigger>().triggers.Add(entry);
		cardlet.GetComponent<EventTrigger>().triggers.Add(entry);
		gameObject.AddComponent<Tooltipped>().tooltip = item.GetItemName();
		cardlet.gameObject.AddComponent<Tooltipped>().tooltip = item.GetItemName();
	}

	private NormalItem RemoveWeapon(int kind, int weapon, bool removeFromSlot = true)
	{
		UnityEngine.Object.Destroy(weaponCards[kind][weapon]);
		NormalItem normalItem = weapons[kind][weapon];
		weapons[kind].RemoveAt(weapon);
		weaponCards[kind].RemoveAt(weapon);
		UnityEngine.Object.Destroy(abstratusCards[kind].transform.GetChild(1).GetChild(0).GetChild(0)
			.GetChild(weapon + 1)
			.gameObject);
			for (int i = weapon; i < weaponCards[kind].Count; i++)
			{
				weaponCards[kind][i].transform.Translate(64f, 0f, 0f);
			}
			if (removeFromSlot)
			{
				ToolbarSlot[] array = toolbarSlots;
				foreach (ToolbarSlot toolbarSlot in array)
				{
					if (toolbarSlot.item == normalItem)
					{
						toolbarSlot.RemoveItem();
					}
				}
			}
			if (kind == aKind && weapon == aIndex)
			{
				if (aIndex >= weapons[aKind].Count && weapons[aKind].Count != 0)
				{
					aIndex--;
				}
				UpdateWeapon();
			}
			return normalItem;
		}

		public bool RemoveWeapon(NormalItem item, bool removeFromSlot = true)
		{
			int num = weaponKinds.IndexOf(item.weaponKind[0]);
			if (num == -1)
			{
				return false;
			}
			int num2 = weapons[num].IndexOf(item);
			if (num2 == -1)
			{
				return false;
			}
			RemoveWeapon(num, num2, removeFromSlot);
			return true;
		}

		public bool RemoveArmor(NormalItem item, PlayerSync player)
		{
			if (item.armor != ArmorKind.None && armor[(int)item.armor].item == item)
			{
				armor[(int)item.armor].item = null;
				player.DisableArmor(item.armor);
				return true;
			}
			return false;
		}

		private void ExpandWeaponKind(int index)
		{
			foreach (GameObject item in weaponCards[index])
			{
				item.SetActive(value: true);
			}
		}

		private void RetractWeaponKind(int index)
		{
			foreach (GameObject item in weaponCards[index])
			{
				item.SetActive(value: false);
			}
		}

		private void PickUpCard(int kind, int weapon)
		{
			if (sylladex.GetDragItem() == null)
			{
				sylladex.SetDragItem(weapons[kind][weapon], GetComponent<Image>().material);
				RemoveWeapon(kind, weapon);
			}
		}

		public void PickUpActive()
		{
			if (aIndex < weapons[aKind].Count)
			{
				PickUpCard(aKind, aIndex);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			foreach (GameObject kindCard in kindCards)
			{
				kindCard.SetActive(value: true);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			for (int i = 0; i < kindCards.Count; i++)
			{
				kindCards[i].SetActive(value: false);
				RetractWeaponKind(i);
			}
		}

		public Item GetActive()
		{
			if (aKind < weapons.Count && aIndex < weapons[aKind].Count)
			{
				return weapons[aKind][aIndex];
			}
			return null;
		}

		private void SetActive(NormalItem item)
		{
			aKind = weaponKinds.IndexOf(item.weaponKind[0]);
			aIndex = weapons[aKind].IndexOf(item);
			if (aIndex == -1)
			{
				aIndex = 0;
			}
			UpdateWeapon();
		}

		public void OnDestroy()
		{
			AcceptorList.acceptors.Remove(this);
		}

		public Item[] EjectWeapons(IEnumerable<Item> except = null)
		{
			List<Item> list = new List<Item>();
			for (int i = 0; i < weapons.Count; i++)
			{
				for (int num = weapons[i].Count - 1; num >= 0; num--)
				{
					if (except == null || !except.Contains(weapons[i][num]))
					{
						list.Add(RemoveWeapon(i, num));
					}
				}
			}
			UpdateWeapon();
			return list.ToArray();
		}

		public Data Save()
		{
			Data result = default(Data);
			result.size = size;
			result.weapons = weapons.SelectMany((List<NormalItem> list) => list.Select((NormalItem item) => item.Save())).ToArray();
			return result;
		}

		public void Load(Data data)
		{
			kindCard = base.transform.Find("Weaponkind").gameObject;
			weaponCard = base.transform.Find("Weapon").gameObject;
			Size = data.size;
			if (data.weapons != null)
			{
				Debug.Log("Loading weapons");
				HouseData.Item[] array = data.weapons;
				foreach (HouseData.Item data2 in array)
				{
					AcceptItem(Item.Load(data2));
				}
			}
		}
	}
