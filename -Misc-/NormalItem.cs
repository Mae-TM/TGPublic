using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract(Surrogate = typeof(HouseData.Item))]
public class NormalItem : Item
{
	public enum Tag
	{
		Bouncy,
		Rocket,
		Piercing,
		Explosive,
		Scatter,
		Lightning,
		Burning,
		Cold,
		Poison,
		Electronic,
		Fragile,
		Menacing,
		Light,
		Metallic,
		Radioactive,
		Magic,
		Crockercorp,
		Dersite,
		Smoky,
		Heavy,
		Volatile,
		Grimdark,
		Trickster,
		Shitty,
		Nerf,
		Nautical,
		Spiky,
		Glorious,
		Seeking,
		Nuclear,
		Regenerative,
		Consumable,
		Candy,
		Timey,
		Breathy,
		Doomy,
		Bloody,
		Hearty,
		Spacey,
		Mindy,
		Lighty,
		Voidy,
		Ragey,
		Hopey,
		Lifey,
		Colored,
		Count
	}

	private readonly struct TagCombination
	{
		public readonly Tag first;

		public readonly Tag second;

		public readonly Tag result;

		public readonly bool keepFirst;

		public readonly bool keepSecond;

		public readonly ArmorKind armor;

		public readonly WeaponKind weapon;

		public TagCombination(Tag first, Tag second, Tag result = Tag.Count, ArmorKind armor = ArmorKind.Count, WeaponKind weapon = WeaponKind.Count, bool keepFirst = false, bool keepSecond = false)
		{
			this.first = first;
			this.second = second;
			this.result = result;
			this.armor = armor;
			this.weapon = weapon;
			this.keepFirst = keepFirst;
			this.keepSecond = keepSecond;
		}
	}

	public float tricksterCooldown = 5f;

	private static readonly TagCombination[] combinations = new TagCombination[16]
	{
		new TagCombination(Tag.Lighty, Tag.Poison, Tag.Radioactive),
		new TagCombination(Tag.Electronic, Tag.Piercing, Tag.Lightning),
		new TagCombination(Tag.Piercing, Tag.Fragile, Tag.Spiky, ArmorKind.Count, WeaponKind.None),
		new TagCombination(Tag.Rocket, Tag.Metallic, Tag.Glorious, ArmorKind.None, WeaponKind.Hammer),
		new TagCombination(Tag.Rocket, Tag.Heavy, Tag.Glorious, ArmorKind.None, WeaponKind.Hammer),
		new TagCombination(Tag.Rocket, Tag.Electronic, Tag.Seeking, ArmorKind.None, WeaponKind.Ranged),
		new TagCombination(Tag.Explosive, Tag.Radioactive, Tag.Nuclear, ArmorKind.None),
		new TagCombination(Tag.Timey, Tag.Explosive, Tag.Regenerative, ArmorKind.None, WeaponKind.Count, keepFirst: false, keepSecond: true),
		new TagCombination(Tag.Timey, Tag.Fragile, Tag.Regenerative, ArmorKind.None, WeaponKind.Count, keepFirst: false, keepSecond: true),
		new TagCombination(Tag.Timey, Tag.Nuclear, Tag.Regenerative, ArmorKind.None, WeaponKind.Count, keepFirst: false, keepSecond: true),
		new TagCombination(Tag.Bloody, Tag.Breathy),
		new TagCombination(Tag.Doomy, Tag.Lifey),
		new TagCombination(Tag.Hearty, Tag.Mindy),
		new TagCombination(Tag.Hopey, Tag.Ragey),
		new TagCombination(Tag.Lighty, Tag.Voidy),
		new TagCombination(Tag.Spacey, Tag.Timey)
	};

	private static readonly Tag[] aspectTags = new Tag[12]
	{
		Tag.Burning,
		Tag.Scatter,
		Tag.Poison,
		Tag.Piercing,
		Tag.Metallic,
		Tag.Explosive,
		Tag.Electronic,
		Tag.Fragile,
		Tag.Cold,
		Tag.Menacing,
		Tag.Light,
		Tag.Bouncy
	};

	public readonly string name;

	public readonly Sprite[][] equipSprites;

	private readonly float rawPower;

	private readonly float powerMultiplier = 1f;

	private readonly float rawSpeed;

	private readonly float speedMultiplier = 1f;

	public readonly float size;

	public readonly ArmorKind armor;

	public readonly WeaponKind[] weaponKind;

	public string captchaCode;

	public readonly string animation;

	private readonly HashSet<Tag> tags;

	private readonly HashSet<Tag> customTags;

	private string sceneObjectName = "ItemObject";

	private Sprite FirstEquipSprite
	{
		get
		{
			Sprite[][] array = equipSprites;
			if (array == null)
			{
				return null;
			}
			Sprite[] array2 = array.FirstOrDefault((Sprite[] list) => list.Length != 0);
			if (array2 == null)
			{
				return null;
			}
			return array2[0];
		}
	}

	public float Power => rawPower * powerMultiplier;

	public float Speed => rawSpeed * speedMultiplier;

	protected override string Prefab => sceneObjectName;

	public override bool IsEntry => itemType == ItemType.Entry;

	public WeaponKind WeaponKind
	{
		get
		{
			if (weaponKind.Length != 0)
			{
				return weaponKind[0];
			}
			return WeaponKind.None;
		}
	}

	public override bool SatisfiesConstraint(WeaponKind weaponConstraint, ArmorKind armorConstraint)
	{
		return SatisfiesConstraint(weaponConstraint, armorConstraint, IsWeapon(), IsWeapon() && IsRanged(), weaponKind, armor);
	}

	private static bool SatisfiesConstraint(WeaponKind weaponConstraint, ArmorKind armorConstraint, bool isWeapon, bool isRanged, ICollection<WeaponKind> weaponKind, ArmorKind armor)
	{
		bool flag = weaponConstraint switch
		{
			WeaponKind.Count => isWeapon, 
			WeaponKind.None => !isWeapon, 
			WeaponKind.Ranged => isRanged, 
			_ => weaponKind.Contains(weaponConstraint), 
		};
		bool flag2 = ((armorConstraint != ArmorKind.Count) ? (armorConstraint == armor) : (armor != ArmorKind.None));
		if (weaponConstraint == WeaponKind.Count && armorConstraint == ArmorKind.Count)
		{
			flag2 = true;
			flag = true;
		}
		return flag && flag2;
	}

	private static void CombineTags(ref HashSet<Tag> tags, bool isWeapon, bool isRanged, ICollection<WeaponKind> weaponKind, ArmorKind armor)
	{
		int[] array = new int[combinations.Length];
		for (int i = 0; i < combinations.Length; i++)
		{
			array[i] = i;
		}
		System.Random r = new System.Random();
		Array.Sort(array, (int a, int b) => r.Next(2) * 2 - 1);
		int[] array2 = array;
		foreach (int num in array2)
		{
			TagCombination tagCombination = combinations[num];
			bool num2 = SatisfiesConstraint(tagCombination.weapon, tagCombination.armor, isWeapon, isRanged, weaponKind, armor);
			if (num2 && tags.Contains(tagCombination.first) && tags.Contains(tagCombination.second) && !tags.Contains(tagCombination.result))
			{
				if (!tagCombination.keepFirst)
				{
					tags.Remove(tagCombination.first);
				}
				if (!tagCombination.keepSecond)
				{
					tags.Remove(tagCombination.second);
				}
				if (tagCombination.result != Tag.Count)
				{
					tags.Add(tagCombination.result);
				}
			}
			if (!num2 && tags.Contains(tagCombination.result))
			{
				tags.Remove(tagCombination.result);
				if (!tags.Contains(tagCombination.first) && !tagCombination.keepFirst)
				{
					tags.Add(tagCombination.first);
				}
				if (!tags.Contains(tagCombination.second) && !tagCombination.keepSecond)
				{
					tags.Add(tagCombination.second);
				}
			}
		}
	}

	public static Tag GetGristTag(int gristIndex)
	{
		return aspectTags[Grist.GetType(gristIndex)];
	}

	private static IEnumerable<Aspect> GetTagAspects(Tag tag)
	{
		if (tag >= Tag.Timey && tag <= Tag.Lifey)
		{
			return Enumerable.Repeat((Aspect)(tag - 33), 1);
		}
		for (int i = 0; i < aspectTags.Length; i++)
		{
			if (aspectTags[i] == tag)
			{
				return Enumerable.Repeat((Aspect)i, 1);
			}
		}
		TagCombination[] array = combinations;
		for (int j = 0; j < array.Length; j++)
		{
			TagCombination tagCombination = array[j];
			if (tagCombination.result == tag)
			{
				return GetTagAspects(tagCombination.first).Concat(GetTagAspects(tagCombination.second));
			}
		}
		return Enumerable.Empty<Aspect>();
	}

	private GristCollection GetTagCost(int normalCost)
	{
		GristCollection gristCollection = new GristCollection();
		gristCollection[Grist.SpecialType.Build] = normalCost;
		int num = Mathf.CeilToInt((float)normalCost / 3f);
		foreach (Tag tag in tags)
		{
			switch (tag)
			{
			case Tag.Shitty:
				gristCollection[Grist.SpecialType.Build] -= normalCost;
				gristCollection[Grist.SpecialType.Artifact] += normalCost;
				continue;
			case Tag.Trickster:
				gristCollection[Grist.SpecialType.Zillium]++;
				continue;
			}
			foreach (Aspect tagAspect in GetTagAspects(tag))
			{
				gristCollection[0, tagAspect] += num;
			}
		}
		return gristCollection;
	}

	public static IEnumerable<Tag> GetTagsWithout(ICollection<Aspect> gristTypes)
	{
		Tag tag = Tag.Bouncy;
		while (tag < Tag.Count)
		{
			if (GetTagAspects(tag).Except(gristTypes).Any())
			{
				yield return tag;
			}
			Tag tag2 = tag + 1;
			tag = tag2;
		}
	}

	private void FinishTags(ref float powerMult, ref float speedMult)
	{
		tags.TrimExcess();
		ApplyTagMultipliers(ref powerMult, ref speedMult);
	}

	public IEnumerable<Sprite[]> GetTagParticles()
	{
		return from sprites in tags.Select(GetTagParticles)
			where sprites != null
			select sprites;
	}

	public static Sprite[] GetTagParticles(Tag tag)
	{
		return tag switch
		{
			Tag.Burning => Resources.LoadAll<Sprite>("Effect/Fire"), 
			Tag.Cold => Resources.LoadAll<Sprite>("Effect/Cold"), 
			Tag.Magic => Resources.LoadAll<Sprite>("Effect/Magic"), 
			Tag.Radioactive => Resources.LoadAll<Sprite>("Effect/Radiation"), 
			Tag.Trickster => Resources.LoadAll<Sprite>("Effect/Zilly"), 
			_ => null, 
		};
	}

	private static bool TagsChangeTexture(ICollection<Tag> tags)
	{
		if (!tags.Contains(Tag.Bouncy) && !tags.Contains(Tag.Cold) && !tags.Contains(Tag.Lightning) && !tags.Contains(Tag.Poison) && !tags.Contains(Tag.Crockercorp) && !tags.Contains(Tag.Grimdark) && !tags.Contains(Tag.Shitty) && !tags.Contains(Tag.Nerf))
		{
			return tags.Contains(Tag.Fragile);
		}
		return true;
	}

	private static Sprite ApplyTags(Sprite sprite, ICollection<Tag> tags)
	{
		return ApplyTags(new Sprite[1][] { new Sprite[1] { sprite } }, tags)[0][0];
	}

	private static Sprite[][] ApplyTags(Sprite[][] sprites, ICollection<Tag> tags)
	{
		if (!TagsChangeTexture(tags))
		{
			return sprites;
		}
		float scaleFactor = 1f;
		Dictionary<Texture2D, Texture2D> textures = new Dictionary<Texture2D, Texture2D>();
		return sprites.Select((Sprite[] spr2) => spr2.Select(delegate(Sprite spr)
		{
			if (!textures.TryGetValue(spr.texture, out var value))
			{
				value = new Texture2D(spr.texture.width, spr.texture.height);
				value.SetPixels(spr.texture.GetPixels());
				value.filterMode = FilterMode.Point;
				value.name = spr.texture.name;
				textures[spr.texture] = value;
			}
			value = ApplyTags(value, tags, spr.rect, makeNew: false);
			Vector2 pivot = new Vector2(spr.pivot.x / spr.rect.width, spr.pivot.y / spr.rect.height);
			Sprite obj = Sprite.Create(value, spr.rect, pivot, spr.pixelsPerUnit / scaleFactor);
			obj.name = spr.name;
			return obj;
		}).ToArray()).ToArray();
	}

	private static Texture2D ApplyTags(Texture2D image, ICollection<Tag> tags, Rect rect = default(Rect), bool makeNew = true)
	{
		if (!TagsChangeTexture(tags))
		{
			return image;
		}
		if (makeNew)
		{
			string text = image.name;
			if (tags.Contains(Tag.Light))
			{
				image = ImageEffects.ResizeTexture(image, image.width / 2, image.height / 2);
			}
			else if (tags.Contains(Tag.Heavy))
			{
				image = ImageEffects.ResizeTexture(image, image.width * 2, image.height * 2);
			}
			else
			{
				Color[] pixels = image.GetPixels();
				image = new Texture2D(image.width, image.height);
				image.SetPixels(pixels);
			}
			image.filterMode = FilterMode.Point;
			image.name = text;
		}
		using (IEnumerator<Tag> enumerator = tags.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case Tag.Bouncy:
					image = ImageEffects.Recolor(image, Color.green, (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Piercing:
					image = ImageEffects.Recolor(image, new Color(0.65f, 0.06f, 0.12f), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Explosive:
					image = ImageEffects.Recolor(image, Color.black, (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Cold:
					image = ImageEffects.Recolor(image, Color.cyan, (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Lightning:
					image = ImageEffects.Overlay(image, Resources.Load<Texture2D>("Effect/lightning"), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Poison:
					image = ImageEffects.Recolor(image, new Color(0f, 0.7f, 0f), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Electronic:
					image = ImageEffects.Recolor(image, new Color(0.86f, 0.86f, 0.86f), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Metallic:
					image = ImageEffects.Recolor(image, new Color(0.7f, 0.7f, 0.7f), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Crockercorp:
					image = ImageEffects.Recolor(image, Color.red, (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Grimdark:
					image = ImageEffects.Recolor(image, new Color(0.5f, 0f, 0.5f), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Shitty:
					image = ImageEffects.Glitchify(image, (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Nerf:
					image = ImageEffects.Recolor(image, new Color(1f, 0.5f, 0f), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				case Tag.Fragile:
					image = ImageEffects.Overlay(image, Resources.Load<Texture2D>("Effect/fracture"), (int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height);
					break;
				}
			}
		}
		image.Apply();
		return image;
	}

	private void ApplyTagEffects(GameObject sceneObject)
	{
		float num = tags.Count;
		if (!IsWeapon() && !IsArmor())
		{
			using HashSet<Tag>.Enumerator enumerator = tags.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case Tag.Burning:
					if (sceneObject.GetComponent<PermanentBurningEffect>() == null)
					{
						PermanentBurningEffect permanentBurningEffect = sceneObject.AddComponent<PermanentBurningEffect>();
						permanentBurningEffect.duration = 2f;
						permanentBurningEffect.intensity = 1f / num;
						permanentBurningEffect.period = 0.5f;
					}
					break;
				case Tag.Electronic:
					if (sceneObject.GetComponent<OpenPesterchum>() == null)
					{
						sceneObject.AddComponent<OpenPesterchum>();
					}
					break;
				case Tag.Radioactive:
					if (sceneObject.GetComponent<PermanentRadiationEffect>() == null)
					{
						PermanentRadiationEffect permanentRadiationEffect = sceneObject.AddComponent<PermanentRadiationEffect>();
						permanentRadiationEffect.radius = 2f;
						permanentRadiationEffect.intensity = 0.5f / num;
						permanentRadiationEffect.period = 2f;
					}
					break;
				}
			}
		}
		if (HasTag(Tag.Smoky))
		{
			UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Smoke"), sceneObject.transform);
		}
		if (HasTag(Tag.Light))
		{
			sceneObject.GetComponent<Rigidbody>().mass *= 0.75f;
		}
		if (HasTag(Tag.Heavy))
		{
			sceneObject.GetComponent<Rigidbody>().mass *= 1.5f;
		}
		if (HasTag(Tag.Consumable))
		{
			ConsumeAction consumeAction = sceneObject.AddComponent<ConsumeAction>();
			consumeAction.item = this;
			consumeAction.duration = 4f;
		}
	}

	private void ApplyTagMultipliers(ref float powerMult, ref float speedMult)
	{
		if (!IsWeapon() && !IsArmor())
		{
			return;
		}
		using HashSet<Tag>.Enumerator enumerator = tags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case Tag.Rocket:
				speedMult *= 1.5f;
				break;
			case Tag.Piercing:
				powerMult *= 1.25f;
				break;
			case Tag.Light:
				powerMult *= 0.75f;
				speedMult *= 1.5f;
				break;
			case Tag.Metallic:
				powerMult *= 1.5f;
				speedMult *= 0.5f;
				break;
			case Tag.Heavy:
				powerMult *= 1.25f;
				speedMult *= 0.5f;
				break;
			case Tag.Glorious:
				speedMult *= 0.75f;
				powerMult *= 1.5f;
				break;
			case Tag.Nuclear:
				speedMult *= 0.5f;
				break;
			case Tag.Shitty:
				if (Power > 0f)
				{
					powerMult *= -1f;
				}
				break;
			case Tag.Nerf:
				if (IsArmor())
				{
					powerMult *= 0.5f;
				}
				else
				{
					powerMult = 0f;
				}
				break;
			}
		}
	}

	private void Break(Attacking owner)
	{
		Player player = owner as Player;
		bool flag = (object)player != null && player.sylladex != null;
		if (flag)
		{
			flag = (IsArmor() ? player.sylladex.strifeSpecibus.RemoveArmor(this, player.sync) : (IsWeapon() && player.sylladex.strifeSpecibus.RemoveWeapon(this)));
		}
		if (tags.Contains(Tag.Regenerative))
		{
			if (flag)
			{
				player.RegenItem(this, 0.5f);
			}
		}
		else
		{
			Destroy();
		}
	}

	public static void Fragments(Attacking owner, float damage, int count, Bounds bounds)
	{
		Collider[] array = new Collider[count];
		for (int i = 0; i < count; i++)
		{
			Bullet bullet = UnityEngine.Object.Instantiate(Resources.Load<Bullet>("Prefabs/Shard"));
			bullet.damage = damage / 3f;
			bullet.owner = owner;
			Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
			bullet.transform.position = ModelUtility.GetSpawnPos(bullet, bounds, insideUnitSphere);
			bullet.Velocity = insideUnitSphere * 10f;
			array[i] = bullet.GetComponent<Collider>();
			for (int j = 0; j < i; j++)
			{
				Physics.IgnoreCollision(array[i], array[j]);
			}
			UnityEngine.Object.Destroy(bullet.gameObject, 2f);
		}
	}

	public void OnHit(Attacking owner, Attackable target, bool ranged = false)
	{
		TagHit(tags, Power, owner, target, ranged, this);
	}

	public static void TagHit(ISet<Tag> tags, float power, Attacking owner, Attackable target, bool ranged = false, NormalItem item = null)
	{
		if (tags == null)
		{
			if (item == null)
			{
				return;
			}
			tags = item.tags;
		}
		float reduction = tags.Count;
		Rigidbody component = target.GetComponent<Rigidbody>();
		foreach (Tag tag in tags)
		{
			TagHit(tag, reduction, component, power, owner, target, ranged, item);
		}
	}

	public static void TagHit(Tag tag, float power, Attacking owner, Attackable target, bool ranged = false)
	{
		Rigidbody component = target.GetComponent<Rigidbody>();
		TagHit(tag, 1f, component, power, owner, target, ranged, null);
	}

	private static void TagHit(Tag tag, float reduction, Rigidbody body, float power, Attacking owner, Attackable target, bool ranged, NormalItem item)
	{
		switch (tag)
		{
		case Tag.Bouncy:
			Attacking.AddForce(body, (target.transform.position - owner.transform.position).normalized * 5f / reduction, ForceMode.Impulse);
			break;
		case Tag.Lightning:
			if (owner.IsInStrife)
			{
				List<Attackable> list = new List<Attackable>(owner.Enemies);
				list.Remove(target);
				Attacking.ChainAttack(target, list, power / 1.5f / reduction, 576f, 1.5f);
			}
			break;
		case Tag.Explosive:
			Attacking.Explosion(target.transform.position, power / reduction, ranged ? (6f / reduction) : (10f / reduction), target);
			if (!ranged)
			{
				item?.Break(owner);
			}
			break;
		case Tag.Electronic:
			GlobalChat.Pester("<i>Attacked <b>" + target.name + "</b>.</i>");
			break;
		case Tag.Burning:
			target.Affect(new BurningEffect(3f, 1f / reduction, 0.75f), stacking: false);
			break;
		case Tag.Cold:
			target.Affect(new SlowEffect(2f, 2f / reduction), stacking: false);
			break;
		case Tag.Poison:
			target.Affect(new PoisonEffect(4f, 1f / reduction, 1f), stacking: false);
			break;
		case Tag.Fragile:
			Fragments(owner, power, 3, ModelUtility.GetBounds(ranged ? target.gameObject : owner.gameObject));
			if (!ranged)
			{
				item?.Break(owner);
			}
			break;
		case Tag.Menacing:
			target.Affect(new TauntEffect(2f / reduction, owner), stacking: false);
			break;
		case Tag.Radioactive:
			target.Affect(new RadiationEffect(10f, 2f, 1.5f / reduction, 1f), stacking: false);
			break;
		case Tag.Grimdark:
			owner.Damage(power / 4f / reduction);
			target.Damage(power / 2f / reduction);
			break;
		case Tag.Glorious:
			Attacking.AddForce(body, (target.transform.position - owner.transform.position).normalized * 5f / reduction, ForceMode.Impulse);
			break;
		case Tag.Nuclear:
		{
			Collider[] array = Physics.OverlapSphere(target.transform.position, 4f);
			foreach (Collider collider in array)
			{
				if (collider.attachedRigidbody != null && collider.attachedRigidbody.TryGetComponent<Attackable>(out var component))
				{
					component.Affect(new RadiationEffect(20f, 2f, 1.5f / reduction, 1f), stacking: false);
				}
			}
			Attacking.Explosion(target.transform.position, 2f * power / reduction, ranged ? (12f / reduction) : (20f / reduction), target, 6f);
			if (!ranged)
			{
				item?.Break(owner);
			}
			break;
		}
		}
	}

	public void TricksterActions(Player self)
	{
		if (!HasTag(Tag.Trickster))
		{
			return;
		}
		tricksterCooldown -= Time.deltaTime;
		if (tricksterCooldown <= 0f)
		{
			tricksterCooldown = UnityEngine.Random.Range(1f, 10f);
			if (UnityEngine.Random.value > 0.0183f)
			{
				self.abilities[0].Execute();
				return;
			}
			self.sylladex.strifeSpecibus.RemoveWeapon(this);
			self.sylladex.ThrowItem(this);
		}
	}

	public void OnDamage(Attacking self, Attackable assailant, float damage, float multiplier = 1f)
	{
		multiplier /= (float)tags.Count;
		using HashSet<Tag>.Enumerator enumerator = tags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case Tag.Lightning:
				if (self.IsInStrife)
				{
					Attacking.ChainAttack(self, new List<Attackable>(self.Enemies), Power / 1.5f * multiplier, 576f, 1.5f);
				}
				break;
			case Tag.Burning:
				assailant.Affect(new BurningEffect(2f, multiplier, 0.5f), stacking: false);
				break;
			case Tag.Fragile:
				Fragments(self, Power, 3, ModelUtility.GetBounds(self.gameObject));
				Break(self);
				break;
			case Tag.Crockercorp:
				if (self == Player.player && UnityEngine.Random.value < 0.2f)
				{
					GameObject gameObject = new GameObject("Ad");
					gameObject.transform.SetParent(Player.Ui.Find("Overlay"), worldPositionStays: false);
					Image image = gameObject.AddComponent<Image>();
					Sprite[] array = Resources.LoadAll<Sprite>("Ads");
					image.sprite = array[UnityEngine.Random.Range(0, array.Length)];
					image.SetNativeSize();
					gameObject.AddComponent<AutoClose>().destroy = true;
					Rect rect = (gameObject.transform.parent as RectTransform).rect;
					gameObject.transform.localPosition = new Vector3(UnityEngine.Random.Range(rect.xMin, rect.xMax), UnityEngine.Random.Range(rect.yMin, rect.yMax));
				}
				break;
			case Tag.Spiky:
				assailant.Damage(damage / 2f);
				break;
			}
		}
	}

	public void OnDeath(Attackable self, float multiplier = 1f)
	{
		multiplier /= (float)tags.Count;
		foreach (Tag tag in tags)
		{
			if (tag == Tag.Explosive)
			{
				Attacking.Explosion(self.transform.position, Power * multiplier, 10f * multiplier, self);
			}
		}
	}

	public void ArmorUpdate(Attackable self, float multiplier = 1f)
	{
		multiplier /= (float)tags.Count;
		foreach (IStatusEffect statusEffect in GetStatusEffects(2f, multiplier))
		{
			self.Affect(statusEffect);
		}
		foreach (Tag tag in tags)
		{
			if (tag == Tag.Metallic)
			{
				self.Remove<BurningEffect>();
			}
		}
	}

	private IEnumerable<IStatusEffect> GetStatusEffects(float duration, float multiplier = 1f)
	{
		using HashSet<Tag>.Enumerator enumerator = tags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case Tag.Burning:
				yield return new BurningEffect(duration, multiplier, 0.5f);
				break;
			case Tag.Cold:
				yield return new SlowEffect(duration, 2f * multiplier);
				break;
			case Tag.Poison:
				yield return new PoisonEffect(duration, multiplier, 1f);
				break;
			case Tag.Radioactive:
				yield return new RadiationEffect(duration, 2f, 0.5f * multiplier, 1f);
				break;
			case Tag.Lifey:
				yield return new HealthRegenBoost(duration, multiplier * 2f / 3f);
				break;
			case Tag.Mindy:
				yield return new VimRegenBoost(duration, multiplier / 15f);
				break;
			}
		}
	}

	public void OnConsume(Attackable self)
	{
		foreach (Tag tag in tags)
		{
			if (tag == Tag.Candy)
			{
				self.Health += 5f;
			}
		}
	}

	public void ArmorSet(Attackable self, float multiplier = 1f)
	{
		multiplier /= (float)tags.Count;
		using HashSet<Tag>.Enumerator enumerator = tags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case Tag.Bouncy:
			{
				if (self.TryGetComponent<PlayerMovement>(out var component))
				{
					component.jumpSpeed *= 1f + 0.5f * multiplier;
				}
				break;
			}
			case Tag.Rocket:
				self.Speed *= 1f + 0.5f * multiplier;
				break;
			case Tag.Electronic:
				if (self is Player player && player.self)
				{
					GameObject.Find("/Canvas/PlayerUI/Button Bar/Build").SetActive(value: true);
				}
				break;
			case Tag.Smoky:
				if (self.transform.Find("Smoke(Clone)") == null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Smoke"), self.transform);
					gameObject.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
					gameObject.transform.localPosition = new Vector3(0f, -0.5f, 0f);
				}
				break;
			case Tag.Trickster:
				self.Speed *= 1f + 2f * multiplier;
				break;
			}
		}
	}

	private static bool HasArmorTag(Player player, Tag tag, ArmorKind ignore)
	{
		for (int i = 0; i < 5; i++)
		{
			if (i != (int)ignore && player.GetArmor(i) != null && player.GetArmor(i).HasTag(tag))
			{
				return true;
			}
		}
		return false;
	}

	public void ArmorUnset(Attackable self, float multiplier = 1f)
	{
		multiplier /= (float)tags.Count;
		using HashSet<Tag>.Enumerator enumerator = tags.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case Tag.Bouncy:
			{
				if (self.TryGetComponent<PlayerMovement>(out var component))
				{
					component.jumpSpeed /= 1f + 0.5f * multiplier;
				}
				break;
			}
			case Tag.Rocket:
				self.Speed /= 1f + 0.5f * multiplier;
				break;
			case Tag.Electronic:
				if (self is Player player2 && player2.self && !HasArmorTag(player2, Tag.Electronic, armor))
				{
					GameObject.Find("/Canvas/PlayerUI/Button Bar/Build").SetActive(value: false);
				}
				break;
			case Tag.Smoky:
			{
				Transform transform = self.transform.Find("Smoke(Clone)");
				if (transform != null && self is Player player && player.self && !HasArmorTag(player, Tag.Smoky, armor))
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
				break;
			}
			case Tag.Trickster:
				self.Speed /= 1f + 2f * multiplier;
				break;
			}
		}
	}

	public NormalItem(NormalItem item)
		: base(item)
	{
		name = item.name;
		rawPower = item.rawPower;
		rawSpeed = item.rawSpeed;
		size = item.size;
		tags = item.tags;
		customTags = item.customTags;
		FinishTags(ref powerMultiplier, ref speedMultiplier);
		weaponKind = item.weaponKind;
		captchaCode = item.captchaCode;
		equipSprites = item.equipSprites;
		armor = item.armor;
		sceneObjectName = item.sceneObjectName;
		animation = item.animation;
	}

	public static implicit operator NormalItem(string captcha)
	{
		return ItemDownloader.Instance.GetItem(captcha);
	}

	public NormalItem(string captcha, ItemType type = ItemType.Normal)
		: this(ItemDownloader.Instance.GetItem(captcha), type)
	{
	}

	public override Item Copy()
	{
		return new NormalItem(this);
	}

	public override string GetItemName()
	{
		if (customTags == null || customTags.Count == 0)
		{
			return name;
		}
		return string.Join(" ", customTags) + " " + name;
	}

	public override GristCollection GetCost()
	{
		int normalCost = ((!(Power >= 0f)) ? Mathf.CeilToInt(rawPower * rawSpeed) : Mathf.CeilToInt(Mathf.Pow(1.3f, rawPower * rawSpeed) * ((IsWeapon() && !IsRanged()) ? size : 1f)));
		return GetTagCost(normalCost);
	}

	public int GetBoonCost()
	{
		return GetCost()[Grist.SpecialType.Build];
	}

	protected override void FillItemObject()
	{
		base.FillItemObject();
		if (itemObject == null)
		{
			sceneObjectName = null;
			return;
		}
		if (itemType == ItemType.Entry)
		{
			MakeEntryObject();
		}
		else
		{
			ApplyTagEffects(base.SceneObject);
		}
		base.SceneObject.SetActive(value: false);
		if (itemType == ItemType.Normal)
		{
			string prototyping = AbstractSingletonManager<DatabaseManager>.Instance.GetPrototyping(captchaCode);
			if (prototyping != null && !itemObject.TryGetComponent<Prototype>(out var _))
			{
				base.SceneObject.AddComponent<Prototype>().protoName = prototyping;
			}
		}
	}

	private void MakeEntryObject()
	{
		Transform transform = base.ItemObject.transform.Find("Item");
		if ((bool)transform)
		{
			FillItemObjectSprite(transform);
			if (HasTag(Tag.Fragile))
			{
				UnityEngine.Object.Destroy(base.ItemObject.GetComponent<PickupItemAction>());
				UnityEngine.Object.Destroy(base.ItemObject.GetComponent<Interactable>());
				transform.gameObject.AddComponent<EnterAction>();
			}
			else
			{
				UnityEngine.Object.Destroy(base.ItemObject.transform.Find("Source").gameObject, 0.5f);
				base.SceneObject.AddComponent<EnterAction>();
			}
		}
		else
		{
			base.SceneObject.AddComponent<EnterAction>();
		}
		RegionChild component = base.ItemObject.GetComponent<RegionChild>();
		if ((bool)component.Area)
		{
			SetEntryItemColor(null, component.Area);
		}
		else
		{
			component.onAreaChanged += SetEntryItemColor;
		}
	}

	private void SetEntryItemColor(WorldArea old, WorldArea area)
	{
		if (!(area is House house))
		{
			return;
		}
		Color cruxiteColor = house.cruxiteColor;
		SpriteRenderer[] componentsInChildren = base.ItemObject.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].color = cruxiteColor;
		}
		PBColor color = cruxiteColor;
		MeshRenderer[] componentsInChildren2 = base.ItemObject.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			Material[] materials = componentsInChildren2[i].materials;
			foreach (Material material in materials)
			{
				switch (material.shader.name)
				{
				case "Toon/Lit":
				case "Toon/Basic":
					material.color = cruxiteColor;
					break;
				case "Custom/litHSVShader":
				case "Custom/HSVShader":
					ImageEffects.SetShiftColor(material, color);
					break;
				}
			}
		}
	}

	public override void ApplyToImage(Image image)
	{
		List<Sprite[]> list = new List<Sprite[]>();
		float num = 2f;
		float num2 = 2f;
		foreach (Tag tag in tags)
		{
			Sprite[] tagParticles = GetTagParticles(tag);
			if (tagParticles != null)
			{
				Sprite[] array = tagParticles;
				foreach (Sprite sprite in array)
				{
					num = Mathf.Max(num, sprite.bounds.size.x * sprite.pixelsPerUnit / 2f);
					num2 = Mathf.Max(num2, sprite.bounds.size.x * sprite.pixelsPerUnit / 2f);
				}
				list.Add(tagParticles);
			}
		}
		if (list.Count > 0)
		{
			Image image2 = null;
			float width = image.rectTransform.rect.width;
			float height = image.rectTransform.rect.height;
			Transform transform = image.transform;
			for (float num3 = 0f; num3 + num < width; num3 += num)
			{
				for (float num4 = 0f; num4 + num2 < height; num4 += num2)
				{
					if (UnityEngine.Random.Range(0, 2) == 0)
					{
						if (image2 == null)
						{
							image2 = new GameObject("Particle").AddComponent<Image>();
							image2.transform.SetParent(transform, worldPositionStays: false);
							image2.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
							image2.rectTransform.pivot = new Vector2(0f, 1f);
							image2.rectTransform.anchorMin = new Vector2(0f, 1f);
							image2.rectTransform.anchorMax = new Vector2(0f, 1f);
						}
						else
						{
							image2 = UnityEngine.Object.Instantiate(image2, transform);
						}
						image2.rectTransform.anchoredPosition = new Vector2(num3, 0f - num4);
						int index = UnityEngine.Random.Range(0, list.Count);
						image2.sprite = list[index][UnityEngine.Random.Range(0, list[index].Length)];
						image2.SetNativeSize();
					}
				}
			}
			image.enabled = false;
			image = new GameObject("Item").AddComponent<Image>();
			image.transform.SetParent(transform);
			image.rectTransform.anchorMin = new Vector2(0f, 0f);
			image.rectTransform.anchorMax = new Vector2(1f, 1f);
			image.rectTransform.offsetMin = new Vector2(0f, 0f);
			image.rectTransform.offsetMax = new Vector2(0f, 0f);
			image.preserveAspect = true;
		}
		else
		{
			image.enabled = true;
		}
		image.sprite = base.sprite;
		image.material = GetMaterial();
		image.color = GetColor();
	}

	public override Material GetMaterial()
	{
		if (!HasTag(Tag.Colored))
		{
			return null;
		}
		return Player.player.sylladex.GetMaterial();
	}

	public static implicit operator NormalItem(LDBItem item)
	{
		if (item != null)
		{
			return new NormalItem(item);
		}
		return null;
	}

	public NormalItem(LDBItem wwwitem, ItemType type = ItemType.Normal)
		: base(type)
	{
		captchaCode = wwwitem.Code;
		name = wwwitem.Name;
		base.weight = 1f;
		rawSpeed = wwwitem.Speed;
		base.description = wwwitem.Description;
		sceneObjectName = wwwitem.Prefab;
		armor = ArmorKind.None;
		List<WeaponKind> list = new List<WeaponKind>();
		if (!string.IsNullOrEmpty(wwwitem.Strifekind))
		{
			string[] array = wwwitem.Strifekind.Split(',');
			foreach (string text in array)
			{
				if (Enum.TryParse<WeaponKind>(text, out var result))
				{
					list.Add(result);
				}
				else if (!Enum.TryParse<ArmorKind>(text, out armor))
				{
					Debug.LogWarning("Could not parse armor/weapon kind '" + text + "' for item '" + GetItemName() + "'.");
				}
			}
		}
		weaponKind = list.ToArray();
		tags = new HashSet<Tag>();
		if (wwwitem.Tags != null)
		{
			foreach (string tag in wwwitem.Tags)
			{
				if (Enum.TryParse<Tag>(tag, out var result2))
				{
					tags.Add(result2);
					continue;
				}
				Debug.LogWarning("Tried to apply unknown tag '" + tag + "' to item '" + GetItemName() + "'.");
			}
		}
		FinishTags(ref powerMultiplier, ref speedMultiplier);
		base.sprite = GetSprite(wwwitem);
		equipSprites = GetEquipSprite(wwwitem);
		ProcessSprites(ref equipSprites);
		rawPower = ((wwwitem.Custom && wwwitem.Grist > 0 && string.IsNullOrWhiteSpace(wwwitem.Strifekind)) ? Mathf.Log(wwwitem.Grist, 1.3f) : ((float)wwwitem.Grist));
		if (IsWeapon())
		{
			size = FirstEquipSprite.bounds.size.y;
			animation = GetAnimation();
		}
	}

	private string GetAnimation(string suggestion = null)
	{
		if (!IsWeapon())
		{
			return null;
		}
		return GetRangedAnimation(suggestion) ?? GetMeleeAnimation(suggestion);
	}

	private string GetRangedAnimation(string suggestion)
	{
		switch (weaponKind[0])
		{
		case WeaponKind.Rifle:
			if (equipSprites[0].Length != 3)
			{
				return "Shoot";
			}
			return "ChargedShoot";
		case WeaponKind.Card:
		case WeaponKind.Bomb:
		case WeaponKind.Ball:
			return "Throw";
		case WeaponKind.Bow:
			if (suggestion != null)
			{
				return suggestion;
			}
			if (captchaCode != "ViolnBow")
			{
				if (equipSprites[0].Length != 2)
				{
					return "Throw";
				}
				return "BowShoot";
			}
			break;
		}
		return null;
	}

	private string GetMeleeAnimation(string suggestion)
	{
		if (equipSprites.Length == 2)
		{
			return "Dualwield";
		}
		switch (weaponKind[0])
		{
		case WeaponKind.Cake:
		case WeaponKind.Book:
			return "Bluntwield";
		case WeaponKind.Blade:
			if (size < 1.5f || HasTag(Tag.Piercing))
			{
				return "Stab";
			}
			if (HasTag(Tag.Light))
			{
				return "Hit";
			}
			return "Twohandwield";
		case WeaponKind.Hammer:
			if (size < 1.75f)
			{
				return "Hit";
			}
			return "Twohandwield";
		case WeaponKind.Spoon:
			if (size < 1f)
			{
				return "Stab";
			}
			return "Hit";
		case WeaponKind.Pronged:
		case WeaponKind.Needle:
		case WeaponKind.Lance:
			return "Stab";
		case WeaponKind.Sickle:
			if (equipSprites[0].Length == 4)
			{
				return "SickleSwing";
			}
			break;
		}
		if (suggestion != null)
		{
			return suggestion;
		}
		if (!(GetItemName() == "Screwdriver"))
		{
			return "Hit";
		}
		return "Stab";
	}

	public static (string, Color) GetAbilityBox(NormalItem weapon)
	{
		return weapon?.GetAbilityBox() ?? ("Attempt", new Color(0.4f, 0.53f, 1f));
	}

	private (string, Color) GetAbilityBox()
	{
		if (HasTag(Tag.Menacing))
		{
			return ("Annoy", new Color(1f, 0.72f, 0.18f));
		}
		Color item = (IsRanged() ? new Color(0.61f, 0.22f, 0.96f) : new Color(0.22f, 0.96f, 0.24f));
		switch (WeaponKind)
		{
		case WeaponKind.Hammer:
		case WeaponKind.Blade:
		case WeaponKind.Club:
			return ("Aggress", item);
		case WeaponKind.Rifle:
			return ("Arsenalize", item);
		case WeaponKind.Bow:
		case WeaponKind.Card:
			return ("Armamentify", item);
		case WeaponKind.Bomb:
			return ("Artillerate", item);
		default:
			return ("Aggrieve", item);
		}
	}

	public static explicit operator NormalItem(HouseData.Item data)
	{
		return (NormalItem)(Item)data;
	}

	public static explicit operator HouseData.Item(NormalItem item)
	{
		return item;
	}

	public NormalItem(HouseData.NormalItem data, ItemObject itemObject = null)
		: this(data.code, data.isEntry ? ItemType.Entry : ItemType.Normal)
	{
		base.ItemObject = itemObject;
		if (data.contents != null)
		{
			ItemSlots component = base.SceneObject.GetComponent<ItemSlots>();
			for (short num = 0; num < data.contents.Length; num = (short)(num + 1))
			{
				component[num].SetItemDirect(data.contents[num]);
			}
		}
	}

	public override HouseData.Item Save()
	{
		if (itemType == ItemType.Custom)
		{
			return SaveComplete();
		}
		HouseData.Item[] contents = null;
		if (itemObject != null && itemObject.TryGetComponent<ItemSlots>(out var component))
		{
			contents = component.Select((ItemSlot slot) => slot.item?.Save()).ToArray();
		}
		return new HouseData.NormalItem
		{
			code = captchaCode,
			isEntry = (itemType == ItemType.Entry),
			contents = contents
		};
	}

	public NormalItem(HouseData.AlchemyItem data)
		: base(ItemType.Custom)
	{
		armor = data.armor;
		captchaCode = data.code;
		rawPower = data.power;
		rawSpeed = data.speed;
		size = data.size;
		animation = data.animation;
		weaponKind = ((data.weaponKind == WeaponKind.None) ? Array.Empty<WeaponKind>() : new WeaponKind[1] { data.weaponKind });
		IEnumerable<Tag> enumerable = data.tags;
		tags = new HashSet<Tag>(enumerable ?? Enumerable.Empty<Tag>());
		enumerable = data.customTags;
		customTags = new HashSet<Tag>(enumerable ?? Enumerable.Empty<Tag>());
		customTags.TrimExcess();
		tags.UnionWith(customTags);
		FinishTags(ref powerMultiplier, ref speedMultiplier);
		if (!string.IsNullOrEmpty(data.equipSprite))
		{
			equipSprites = GetEquipSprite(data.equipSprite);
		}
		base.sprite = GetSprite(data.sprite);
		ProcessSprites(ref equipSprites);
		name = data.name;
	}

	public NormalItem(Stream stream)
		: base(ItemType.Custom)
	{
		armor = (ArmorKind)stream.ReadByte();
		captchaCode = HouseLoader.readString(stream, 256);
		base.weight = HouseLoader.readFloat(stream);
		rawPower = HouseLoader.readFloat(stream);
		rawSpeed = HouseLoader.readFloat(stream);
		size = HouseLoader.readFloat(stream);
		animation = HouseLoader.readString(stream, 256);
		weaponKind = new WeaponKind[stream.ReadByte()];
		for (int i = 0; i < weaponKind.Length; i++)
		{
			weaponKind[i] = (WeaponKind)stream.ReadByte();
		}
		tags = new HashSet<Tag>();
		int num = stream.ReadByte();
		for (int j = 0; j < num; j++)
		{
			tags.Add((Tag)stream.ReadByte());
		}
		customTags = new HashSet<Tag>();
		num = stream.ReadByte();
		for (int k = 0; k < num; k++)
		{
			Tag item = (Tag)stream.ReadByte();
			tags.Add(item);
			customTags.Add(item);
		}
		customTags.TrimExcess();
		FinishTags(ref powerMultiplier, ref speedMultiplier);
		string text = HouseLoader.readString(stream, 256);
		if (text != string.Empty)
		{
			equipSprites = GetEquipSprite(text);
		}
		base.sprite = GetSprite(HouseLoader.readString(stream, 256));
		ProcessSprites(ref equipSprites);
		name = HouseLoader.readString(stream, 256);
	}

	private void SaveComplete(Stream stream)
	{
		stream.WriteByte((byte)armor);
		HouseLoader.writeString(captchaCode, stream);
		HouseLoader.writeFloat(base.weight, stream);
		HouseLoader.writeFloat(rawPower, stream);
		HouseLoader.writeFloat(rawSpeed, stream);
		HouseLoader.writeFloat(size, stream);
		HouseLoader.writeString(animation ?? string.Empty, stream);
		stream.WriteByte((byte)this.weaponKind.Length);
		WeaponKind[] array = this.weaponKind;
		foreach (WeaponKind weaponKind in array)
		{
			stream.WriteByte((byte)weaponKind);
		}
		Tag[] array2 = tags.Except(customTags).ToArray();
		stream.WriteByte((byte)array2.Length);
		Tag[] array3 = array2;
		foreach (Tag tag in array3)
		{
			stream.WriteByte((byte)tag);
		}
		stream.WriteByte((byte)customTags.Count);
		foreach (Tag customTag in customTags)
		{
			stream.WriteByte((byte)customTag);
		}
		Sprite firstEquipSprite = FirstEquipSprite;
		HouseLoader.writeString((firstEquipSprite == null) ? string.Empty : firstEquipSprite.texture.name, stream);
		HouseLoader.writeString(base.sprite.name, stream);
		HouseLoader.writeString(name, stream);
	}

	private HouseData.AlchemyItem SaveComplete()
	{
		return new HouseData.AlchemyItem
		{
			armor = armor,
			code = captchaCode,
			power = rawPower,
			speed = rawSpeed,
			size = size,
			animation = animation,
			weaponKind = WeaponKind,
			tags = tags.Except(customTags).ToArray(),
			customTags = customTags.ToArray(),
			equipSprite = FirstEquipSprite?.texture.name,
			sprite = base.sprite.name,
			name = name
		};
	}

	private Sprite[][] GetEquipSprite(LDBItem item)
	{
		if (!string.IsNullOrWhiteSpace(item.Weaponsprite))
		{
			return GetEquipSprite(item.Custom ? ("CustomItems/" + item.Weaponsprite) : item.Weaponsprite);
		}
		return null;
	}

	private Sprite[][] GetEquipSprite(string name)
	{
		if (name.StartsWith("CustomItems/"))
		{
			return GetCustomEquipSprites(name) ?? new Sprite[1][] { new Sprite[1] { base.sprite } };
		}
		Sprite[][] array;
		switch (armor)
		{
		case ArmorKind.Hat:
		case ArmorKind.Face:
		case ArmorKind.Shoes:
			array = new Sprite[1][] { ItemDownloader.GetArmor(name, armor) };
			break;
		case ArmorKind.Shirt:
			array = new Sprite[4][]
			{
				ItemDownloader.GetArmor(name, ArmorKind.Shirt),
				ItemDownloader.GetArmor(name + "back", ArmorKind.Shirt),
				ItemDownloader.GetSleeves(name),
				ItemDownloader.GetGloves(name)
			};
			break;
		case ArmorKind.Pants:
			array = new Sprite[2][]
			{
				ItemDownloader.GetArmor(name, ArmorKind.Pants),
				ItemDownloader.GetArmor(name + "over", ArmorKind.Pants)
			};
			break;
		default:
			array = GetEquipSprite(IsWeapon() ? ItemDownloader.GetWeapon(name) : ItemDownloader.GetSpecialItem(name));
			break;
		}
		if (array.All((Sprite[] sprites) => sprites == null || sprites.Length == 0))
		{
			Debug.LogError($"Could not load sprite '{name}' for armor type {armor}!");
			return null;
		}
		return array;
	}

	private static Sprite[][] GetEquipSprite(IEnumerable<Sprite> flatList)
	{
		List<List<Sprite>> list2 = new List<List<Sprite>>();
		foreach (Sprite item in flatList.OrderBy((Sprite spr) => spr.name))
		{
			string text = item.name;
			int num = text.LastIndexOf('_');
			if (num == -1 || !int.TryParse(text.Substring(num + 1), out var result))
			{
				list2.Add(new List<Sprite> { item });
				continue;
			}
			text = text.Substring(0, num);
			num = text.LastIndexOf('_');
			if (num != -1 && int.TryParse(text.Substring(num + 1), out var result2))
			{
				result = result2;
			}
			while (list2.Count <= result)
			{
				list2.Add(new List<Sprite>());
			}
			list2[result].Add(item);
		}
		return list2.Select((List<Sprite> list) => list.ToArray()).ToArray();
	}

	private static Sprite[][] GetCustomEquipSprites(string path)
	{
		if (path.EndsWith(".png"))
		{
			return new Sprite[1][] { new Sprite[1] { GetCustomSprite(path) } };
		}
		Sprite[][] equipSprite = GetEquipSprite(StreamingAssets.GetDirectoryContents(path, "*.png").Select(delegate(FileInfo file)
		{
			Texture2D texture2D = LoadPNG(file.FullName);
			texture2D.name = path;
			Sprite obj = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			obj.name = Path.GetFileNameWithoutExtension(file.Name);
			return obj;
		}));
		if (equipSprite.Length != 0)
		{
			return equipSprite;
		}
		return null;
	}

	private static Sprite GetSprite(LDBItem item)
	{
		return GetSprite(item.Custom ? ("CustomItems/" + item.Icon + ".png") : item.Icon);
	}

	private static Sprite GetSprite(string name)
	{
		if (!name.StartsWith("CustomItems/"))
		{
			return ItemDownloader.GetSprite(name);
		}
		return GetCustomSprite(name);
	}

	private void ProcessSprites(ref Sprite[][] equipSprites)
	{
		if (customTags != null)
		{
			equipSprites = ApplyTags(equipSprites, customTags);
		}
		if ((object)base.sprite == null)
		{
			base.sprite = FirstEquipSprite;
			if ((object)base.sprite == null)
			{
				Sprite sprite2 = (base.sprite = ItemDownloader.GetSprite("GenericObject"));
			}
		}
		else if (customTags != null)
		{
			base.sprite = ApplyTags(base.sprite, customTags);
		}
	}

	private static Sprite GetCustomSprite(string path)
	{
		if (!StreamingAssets.TryGetFile(path, out var path2))
		{
			return ItemDownloader.GetSprite("GenericObject");
		}
		Texture2D texture2D = LoadPNG(path2);
		texture2D.filterMode = FilterMode.Point;
		texture2D.name = path;
		Sprite obj = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		obj.name = path;
		return obj;
	}

	public static bool IsItemInDatabase(string captcha)
	{
		return AbstractSingletonManager<DatabaseManager>.Instance.DoesItemExist(captcha);
	}

	public static Texture2D LoadPNG(string filePath)
	{
		Texture2D texture2D = null;
		if (File.Exists(filePath))
		{
			byte[] data = File.ReadAllBytes(filePath);
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
			texture2D.filterMode = FilterMode.Point;
		}
		return texture2D;
	}

	public NormalItem(NormalItem a, NormalItem b, bool orCombo)
		: base(ItemType.Custom)
	{
		base.weight = (a.weight + b.weight) / 2f;
		if ((a.IsWeapon() || a.IsArmor()) && (b.IsWeapon() || b.IsArmor()))
		{
			rawSpeed = (a.rawSpeed + b.rawSpeed) / 2f;
		}
		else
		{
			rawSpeed = a.rawSpeed;
		}
		rawPower = a.rawPower + Mathf.Sqrt(b.rawPower);
		weaponKind = a.weaponKind;
		armor = a.armor;
		tags = new HashSet<Tag>(a.tags);
		if (orCombo)
		{
			tags.UnionWith(b.tags);
		}
		else
		{
			tags.IntersectWith(b.tags);
		}
		bool flag = IsWeapon();
		CombineTags(ref tags, flag, flag && a.IsRanged(), weaponKind, armor);
		if (tags.Count > 4)
		{
			tags.Add(Tag.Volatile);
		}
		LDBItem lDBItem = null;
		if (GetTagCount() != 0)
		{
			int power = Mathf.RoundToInt(rawPower);
			int speed = Mathf.RoundToInt(rawSpeed);
			if (flag)
			{
				lDBItem = AbstractSingletonManager<DatabaseManager>.Instance.FindItem(weaponKind[0], tags, power, speed);
			}
			if (lDBItem == null && IsArmor())
			{
				lDBItem = AbstractSingletonManager<DatabaseManager>.Instance.FindItem(armor, tags, power, speed);
			}
		}
		if (lDBItem != null)
		{
			customTags = new HashSet<Tag>(tags.Except(lDBItem.Tags.Select((string tag) => (Tag)Enum.Parse(typeof(Tag), tag))));
			name = lDBItem.Name;
			base.sprite = GetSprite(lDBItem);
			equipSprites = GetEquipSprite(lDBItem);
			ProcessSprites(ref equipSprites);
			if (flag)
			{
				SetSizeAnimation(ref animation, ref size, a, b);
			}
			base.description = lDBItem.Description;
			sceneObjectName = lDBItem.Prefab;
		}
		else
		{
			name = b.name.Substring(0, b.name.Length / 2) + a.name.Substring(a.name.Length / 2);
			if (a.HasTag(Tag.Colored))
			{
				tags.Add(Tag.Colored);
			}
			else
			{
				tags.Remove(Tag.Colored);
			}
			HashSet<Tag> hashSet = new HashSet<Tag>(tags.Except(a.tags));
			IEnumerable<Tag> collection;
			if (a.customTags != null)
			{
				collection = a.customTags.Intersect(tags).Union(hashSet);
			}
			else
			{
				IEnumerable<Tag> enumerable = hashSet;
				collection = enumerable;
			}
			customTags = new HashSet<Tag>(collection);
			base.sprite = ApplyTags(a.sprite, hashSet);
			if (a.equipSprites != null)
			{
				equipSprites = ApplyTags(a.equipSprites, hashSet);
			}
			SetSizeAnimation(ref animation, ref size, a, b, a.animation);
			if (a.sceneObjectName == "ItemObjectLarge" || a.sceneObjectName.StartsWith("Animals"))
			{
				sceneObjectName = a.sceneObjectName;
			}
		}
		FinishTags(ref powerMultiplier, ref speedMultiplier);
	}

	private void SetSizeAnimation(ref string animation, ref float size, NormalItem a, NormalItem b, string suggestion = null)
	{
		if (!IsWeapon())
		{
			return;
		}
		animation = GetRangedAnimation(suggestion);
		if (animation != null && IsRanged())
		{
			size = FirstEquipSprite.bounds.size.y;
			return;
		}
		if (!b.IsWeapon())
		{
			size = a.size;
		}
		else
		{
			size = (a.size + b.size) / 2f;
		}
		Sprite firstEquipSprite = FirstEquipSprite;
		float num = (firstEquipSprite.border.y + firstEquipSprite.border.w) / firstEquipSprite.pixelsPerUnit;
		size = Mathf.Clamp(size, (firstEquipSprite.bounds.size.y + num) / 2f, firstEquipSprite.bounds.size.y * 2f - num);
		if (animation == null)
		{
			animation = GetMeleeAnimation(suggestion);
		}
	}

	public static NormalItem operator &(NormalItem a, NormalItem b)
	{
		if (a.Equals(b))
		{
			return a;
		}
		if (GetPrimaryItem(a, b))
		{
			return new NormalItem(a, b, orCombo: false);
		}
		return new NormalItem(b, a, orCombo: false);
	}

	public static NormalItem operator |(NormalItem a, NormalItem b)
	{
		if (a.Equals(b))
		{
			return b;
		}
		if (GetPrimaryItem(a, b))
		{
			return new NormalItem(b, a, orCombo: true);
		}
		return new NormalItem(a, b, orCombo: true);
	}

	private static bool GetPrimaryItem(NormalItem a, NormalItem b)
	{
		if (a.tags.Count != b.tags.Count)
		{
			return a.tags.Count > b.tags.Count;
		}
		if (string.Compare(a.name, b.name) > 0)
		{
			return true;
		}
		if (string.Compare(a.name, b.name) == 0)
		{
			if (a.weight != b.weight)
			{
				return a.weight > b.weight;
			}
			return string.Compare(a.captchaCode, b.captchaCode) > 0;
		}
		return false;
	}

	public bool IsArmor()
	{
		return armor != ArmorKind.None;
	}

	public bool IsWeapon()
	{
		if (weaponKind != null)
		{
			return weaponKind.Length != 0;
		}
		return false;
	}

	public bool IsRanged()
	{
		if (!IsWeapon())
		{
			throw new ArgumentException("Cannot call IsRanged() on a non-weapon!");
		}
		if (!(animation == "Shoot") && !(animation == "Throw") && !(animation == "BowShoot"))
		{
			return animation == "ChargedShoot";
		}
		return true;
	}

	public bool HasTag(Tag tag)
	{
		return tags.Contains(tag);
	}

	public IEnumerable<Tag> GetTags()
	{
		return tags;
	}

	public int GetTagCount()
	{
		return tags.Count;
	}

	public bool[] GetTagArray()
	{
		bool[] array = new bool[46];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = false;
		}
		foreach (Tag tag in tags)
		{
			array[(int)tag] = true;
		}
		return array;
	}

	public void SetTagArray(bool[] tagArray)
	{
		tags.Clear();
		for (int i = 0; i < 46; i++)
		{
			if (tagArray[i])
			{
				tags.Add((Tag)i);
			}
		}
		tags.TrimExcess();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NormalItem normalItem))
		{
			return base.Equals(obj);
		}
		if (itemType != normalItem.itemType)
		{
			return false;
		}
		if (itemType == ItemType.Normal)
		{
			return captchaCode == normalItem.captchaCode;
		}
		if (armor == normalItem.armor && name == normalItem.name && rawPower == normalItem.rawPower && size == normalItem.size && rawSpeed == normalItem.rawSpeed && base.weight == normalItem.weight && (((weaponKind == null || weaponKind.Length == 0) && (normalItem.weaponKind == null || normalItem.weaponKind.Length == 0)) || weaponKind[0] == normalItem.weaponKind[0]))
		{
			return normalItem.tags.SetEquals(normalItem.tags);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (itemType == ItemType.Normal)
		{
			return captchaCode.GetHashCode();
		}
		int num = 475529484;
		num = num * -1521134295 + name.GetHashCode();
		num = num * -1521134295 + base.weight.GetHashCode();
		num = num * -1521134295 + rawPower.GetHashCode();
		num = num * -1521134295 + rawSpeed.GetHashCode();
		num = num * -1521134295 + size.GetHashCode();
		num = num * -1521134295 + armor.GetHashCode();
		if (weaponKind != null && weaponKind.Length != 0)
		{
			num = num * -1521134295 + weaponKind[0].GetHashCode();
		}
		return num;
	}
}
