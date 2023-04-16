using Quest.NET.Enums;
using Quest.NET.Interfaces;
using UnityEngine;

public class ItemGiveObjective : QuestObjective
{
	private readonly Item item;

	private readonly NormalItem.Tag tag;

	private readonly WeaponKind weapon;

	private readonly ArmorKind armor;

	private readonly string image;

	public ItemGiveObjective(bool isBonus, Item item, string title = null, string description = null, string image = null, NormalItem.Tag? tag = null, WeaponKind? weapon = null, ArmorKind? armor = null)
		: base(title ?? ("Give " + GetTitle(item, tag, weapon, armor)), description, isBonus)
	{
		this.item = item;
		this.tag = tag ?? NormalItem.Tag.Count;
		if (weapon.HasValue)
		{
			this.weapon = weapon.Value;
			this.armor = armor ?? ArmorKind.None;
		}
		else if (armor.HasValue)
		{
			this.weapon = WeaponKind.None;
			this.armor = armor.Value;
		}
		else
		{
			this.weapon = WeaponKind.Count;
			this.armor = ArmorKind.Count;
		}
		this.image = image;
	}

	private static string GetTitle(Item item, NormalItem.Tag? tag, WeaponKind? weapon, ArmorKind? armor)
	{
		if (item != null)
		{
			return item.ToString();
		}
		string text = null;
		if (weapon.HasValue)
		{
			text = weapon.ToString();
		}
		if (armor.HasValue)
		{
			text = armor.ToString();
		}
		if (tag.HasValue)
		{
			if (text != null)
			{
				return $"{tag} {text}";
			}
			return $"something {tag}";
		}
		return text ?? "something";
	}

	public override void Invoke(object arg)
	{
		if (arg is Component component)
		{
			if (!component.TryGetComponent<ItemAcceptorCallback>(out var component2))
			{
				component2 = component.gameObject.AddComponent<ItemAcceptorCallback>();
			}
			Sprite sprite = Resources.Load<Sprite>(image);
			if (!sprite && item != null)
			{
				sprite = item.sprite;
			}
			if (!sprite && weapon != WeaponKind.Count)
			{
				sprite = ItemDownloader.GetWeaponKind(weapon);
			}
			if (!sprite && armor != ArmorKind.Count)
			{
				sprite = ItemDownloader.GetArmorKind(armor);
			}
			component2.AddItemEvent(AcceptItem, sprite);
		}
	}

	private bool AcceptItem(Item givenItem)
	{
		if (item != null && !item.Equals(givenItem))
		{
			return false;
		}
		if (!givenItem.SatisfiesConstraint(weapon, armor) || (tag != NormalItem.Tag.Count && (!(givenItem is NormalItem normalItem) || !normalItem.HasTag(tag))))
		{
			return false;
		}
		Status = ObjectiveStatus.Completed;
		return true;
	}
}
