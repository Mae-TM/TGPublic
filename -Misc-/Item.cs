using System;
using Mirror;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract(Surrogate = typeof(HouseData.Item))]
public abstract class Item
{
	public enum ItemType
	{
		Normal,
		Totem,
		Punched,
		Entry,
		Custom,
		Container
	}

	private struct SpawnMessage : NetworkMessage
	{
		public Item item;

		public NetworkBehaviour area;

		public Vector3 position;

		public bool asOwner;
	}

	public readonly ItemType itemType;

	protected ItemObject itemObject;

	public Sprite sprite { get; protected set; }

	public float weight { get; protected set; }

	public string description { get; protected set; }

	public GameObject SceneObject => ItemObject.gameObject;

	public NetworkIdentity NetIdentity => itemObject?.netIdentity;

	public ItemObject ItemObject
	{
		get
		{
			if ((object)itemObject == null)
			{
				GameObject prefab = ItemDownloader.GetPrefab(Prefab) ?? ItemDownloader.GetPrefab("ItemObject");
				ItemObject.Make(this, prefab);
			}
			return itemObject;
		}
		set
		{
			if (!(value == itemObject))
			{
				itemObject = value;
				FillItemObject();
			}
		}
	}

	protected abstract string Prefab { get; }

	public abstract bool IsEntry { get; }

	public bool IsPrefab(string prefab)
	{
		return Prefab.Equals(prefab, StringComparison.InvariantCultureIgnoreCase);
	}

	protected virtual void FillItemObject()
	{
		if (itemObject == null)
		{
			return;
		}
		if (itemObject.name.StartsWith("ItemObject"))
		{
			FillItemObjectSprite(itemObject.transform);
		}
		itemObject.name = GetItemName();
		if (!itemObject.TryGetComponent<PickupItemAction>(out var component))
		{
			component = SceneObject.AddComponent<PickupItemAction>();
		}
		component.targetItem = this;
		if (itemObject.TryGetComponent<Rigidbody>(out var component2))
		{
			if (weight == 0f)
			{
				component2.isKinematic = true;
			}
			else
			{
				component2.mass = weight;
			}
		}
	}

	protected void FillItemObjectSprite(Transform transform)
	{
		transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprite;
		BoxCollider component = transform.GetChild(0).GetComponent<BoxCollider>();
		component.size = new Vector3(sprite.bounds.size.x, sprite.bounds.size.y, 0.2f);
		component.center = sprite.bounds.center;
	}

	public void PutDown(WorldArea area, Vector3 position, Quaternion rotation = default(Quaternion), bool asOwner = false)
	{
		PutDownLocal(area, area.transform.InverseTransformPoint(position), rotation, asOwner);
	}

	public void PutDownLocal(WorldArea area, Vector3 position, Quaternion rotation = default(Quaternion), bool asOwner = false)
	{
		if (NetworkServer.active)
		{
			ItemObject.PutDown(area, position, rotation, asOwner ? NetworkServer.localConnection : null);
			return;
		}
		SpawnMessage message = default(SpawnMessage);
		message.item = this;
		message.area = area;
		message.position = position;
		message.asOwner = asOwner;
		NetworkClient.Send(message);
	}

	protected Item(ItemType itemType)
	{
		this.itemType = itemType;
	}

	protected Item(Item item)
		: this(item.itemType)
	{
		sprite = item.sprite;
		weight = item.weight;
		description = item.description;
	}

	public static implicit operator Item(string code)
	{
		if (code.Length <= 8)
		{
			return new NormalItem(code);
		}
		return code[0] switch
		{
			'T' => new Totem(new NormalItem(code.Substring(1)), Color.black), 
			'P' => new PunchCard(new NormalItem(code.Substring(1))), 
			_ => null, 
		};
	}

	public abstract string GetItemName();

	public override string ToString()
	{
		return GetItemName();
	}

	public virtual GristCollection GetCost()
	{
		return new GristCollection();
	}

	public abstract Item Copy();

	~Item()
	{
		Destroy();
	}

	public virtual void Destroy()
	{
		if (SpawnHelper.instance != null && itemObject != null)
		{
			SpawnHelper.instance.Destroy(itemObject.gameObject);
		}
	}

	public abstract void ApplyToImage(Image image);

	public virtual void ApplyToCard(SpriteRenderer card)
	{
		card.sprite = sprite;
		card.transform.localScale *= 2f / (sprite.bounds.size.x + sprite.bounds.size.y);
	}

	public virtual void ClearCard(SpriteRenderer card)
	{
		card.sprite = null;
		card.transform.localScale /= 2f / (sprite.bounds.size.x + sprite.bounds.size.y);
	}

	public static void ClearImage(Image image)
	{
		foreach (Transform item in image.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		image.sprite = null;
		image.material = null;
		image.color = Color.white;
	}

	public virtual Material GetMaterial()
	{
		return null;
	}

	public virtual Color GetColor()
	{
		return Color.white;
	}

	public virtual bool SatisfiesConstraint(WeaponKind weaponConstraint, ArmorKind armorConstraint)
	{
		if (weaponConstraint != 0 || armorConstraint != ArmorKind.None)
		{
			if (weaponConstraint == WeaponKind.Count)
			{
				return armorConstraint == ArmorKind.Count;
			}
			return false;
		}
		return true;
	}

	public static implicit operator HouseData.Item(Item item)
	{
		return item?.Save();
	}

	public abstract HouseData.Item Save();

	public static implicit operator Item(HouseData.Item data)
	{
		return Load(data);
	}

	public static Item Load(HouseData.Item data)
	{
		return Load(data, null);
	}

	public static Item Load(HouseData.Item data, ItemObject itemObject)
	{
		if (!(data is HouseData.NormalItem data2))
		{
			if (!(data is HouseData.AlchemyItem data3))
			{
				if (!(data is HouseData.PunchCard data4))
				{
					if (data is HouseData.Totem data5)
					{
						return new Totem(data5);
					}
					return null;
				}
				return new PunchCard(data4);
			}
			return new NormalItem(data3);
		}
		return new NormalItem(data2, itemObject);
	}

	static Item()
	{
		NetcodeManager.RegisterStaticHandler<SpawnMessage>(Spawn);
	}

	private static void Spawn(NetworkConnection sender, SpawnMessage message)
	{
		message.item.ItemObject.PutDown(message.area as WorldArea, message.position, default(Quaternion), message.asOwner ? sender : null);
	}
}
