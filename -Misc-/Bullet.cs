using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float damage = 1f;

	public Vector3 target;

	public Attacking owner;

	public bool isOnlyDamage;

	public NormalItem weapon;

	private ISet<NormalItem.Tag> tags;

	private float speedMultiplier = 1f;

	private readonly List<Collider> targets = new List<Collider>();

	private Rigidbody body;

	[SerializeField]
	private Sprite[] sprites;

	public Vector3 Velocity
	{
		set
		{
			if ((object)body == null)
			{
				body = GetComponent<Rigidbody>();
			}
			body.isKinematic = false;
			body.velocity = value;
			Rotate(value);
		}
	}

	public static void Make(Attacking caster, Sprite[] sprites, float speed, float damage, Vector3 target, ISet<NormalItem.Tag> tags = null)
	{
		GameObject obj = new GameObject("Projectile");
		obj.transform.SetParent(caster.transform, worldPositionStays: false);
		obj.AddComponent<SpriteRenderer>();
		obj.AddComponent<CapsuleCollider>().direction = 0;
		Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		rigidbody.useGravity = false;
		Bullet bullet = obj.AddComponent<Bullet>();
		bullet.damage = damage;
		bullet.target = target;
		bullet.tags = tags;
		bullet.SetSprites(sprites);
		bullet.Velocity = (target - bullet.transform.position).normalized * speed;
		bullet.Make(caster);
	}

	public static Collider Make(Attacking caster, GameObject prefab, float speed, float damage, Vector3 target)
	{
		Bullet bullet = Object.Instantiate(prefab).AddComponent<Bullet>();
		bullet.damage = damage;
		bullet.target = target;
		Vector3 normalized = (target - caster.transform.position).normalized;
		bullet.Velocity = normalized * speed;
		bullet.transform.position = ModelUtility.GetSpawnPos(bullet, caster, normalized);
		return bullet.Make(caster);
	}

	public static Collider Make(Attacking caster, Bullet prefab, float speed, float damagePortion = 1f, float angle = 0f)
	{
		Bullet bullet = Object.Instantiate(prefab, caster.transform, worldPositionStays: true);
		bullet.weapon = prefab.weapon;
		bullet.damage = prefab.damage * damagePortion;
		Vector3 normalized = (bullet.target - bullet.transform.position).normalized;
		bullet.Velocity = Quaternion.AngleAxis(angle, Vector3.up) * normalized * speed;
		return bullet.Make(caster);
	}

	private Collider Make(Attacking caster)
	{
		owner = caster;
		if (owner.TryGetAttackTag(out var item))
		{
			if (tags == null)
			{
				tags = new HashSet<NormalItem.Tag> { item };
			}
			else
			{
				tags.Add(item);
			}
		}
		if (TryGetComponent<Collider>(out var component))
		{
			component.enabled = true;
			Collider[] componentsInChildren = owner.GetComponentsInChildren<Collider>(includeInactive: true);
			foreach (Collider collider in componentsInChildren)
			{
				Physics.IgnoreCollision(component, collider);
			}
		}
		Transform transform = base.transform;
		if ((bool)transform.parent)
		{
			Vector3 localScale = transform.localScale;
			if (transform.parent.localScale.x * localScale.x < 0f)
			{
				localScale.x = 0f - Mathf.Abs(localScale.x);
			}
			else
			{
				localScale.x = Mathf.Abs(localScale.x);
			}
			transform.localScale = localScale;
			transform.SetParent(transform.parent.parent, worldPositionStays: true);
		}
		ApplyTagEffects();
		base.gameObject.SetActive(value: true);
		base.enabled = true;
		Object.Destroy(base.gameObject, 2f);
		return component;
	}

	private void ApplyTagEffects()
	{
		if (HasTag(NormalItem.Tag.Rocket) || HasTag(NormalItem.Tag.Seeking))
		{
			body.velocity = Vector3.zero;
		}
		if (HasTag(NormalItem.Tag.Metallic))
		{
			body.velocity *= 1.5f;
		}
		if (HasTag(NormalItem.Tag.Seeking))
		{
			SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
			sphereCollider.isTrigger = true;
			sphereCollider.radius = 20f;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!targets.Contains(collider) && collider.TryGetComponent<Attackable>(out var component) && !(component is Player))
		{
			targets.Add(collider);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (targets.Contains(collider))
		{
			targets.Remove(collider);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Collider collider = collision.collider;
		Attackable componentInParent = collider.GetComponentInParent<Attackable>();
		ContactPoint contact = collision.GetContact(0);
		Collide(componentInParent, collider, contact.point, contact.normal, collision.impulse.magnitude / body.mass);
	}

	public bool HasTag(NormalItem.Tag tag)
	{
		return tags?.Contains(tag) ?? weapon?.HasTag(tag) ?? false;
	}

	public int GetTagCount()
	{
		return tags?.Count ?? weapon?.GetTagCount() ?? 0;
	}

	public void RefreshSprite()
	{
		if (weapon?.equipSprites != null && weapon.equipSprites.Length != 0)
		{
			int num = ((weapon.equipSprites.Length != 1) ? Random.Range(1, weapon.equipSprites.Length) : 0);
			SetSprites(weapon.equipSprites[num]);
		}
	}

	private void SetSprites(Sprite[] newSprites)
	{
		if (sprites != null && newSprites.SequenceEqual(sprites))
		{
			return;
		}
		sprites = newSprites;
		Sprite sprite = sprites[0];
		if (!TryGetComponent<SpriteRenderer>(out var component))
		{
			return;
		}
		component.sprite = sprite;
		if (!TryGetComponent<CapsuleCollider>(out var component2))
		{
			return;
		}
		Vector3 localScale = base.transform.localScale;
		Bounds bounds = sprite.bounds;
		component2.radius = bounds.extents.y / localScale.y;
		component2.height = 2f * bounds.extents.x / localScale.x;
		component2.center = bounds.center;
		if (!TryGetComponent<ParticleManager>(out var component3))
		{
			return;
		}
		component3.ClearParticles(remove: true);
		IEnumerable<Sprite[]> enumerable;
		if (tags != null)
		{
			enumerable = from s in tags.Select(NormalItem.GetTagParticles)
				where s != null
				select s;
		}
		else
		{
			if (weapon == null)
			{
				return;
			}
			enumerable = weapon.GetTagParticles();
		}
		foreach (Sprite[] item in enumerable)
		{
			component3.AddParticles(item);
		}
		component3.SetBounds(bounds.center, new Vector3(2f * bounds.extents.x, 2f * bounds.extents.y, 0.2f));
	}

	private void Rotate(Vector3 direction)
	{
		if (!(direction == Vector3.zero))
		{
			Vector3 forward = MSPAOrthoController.main.transform.forward;
			base.transform.rotation = Quaternion.LookRotation(direction, -forward) * Quaternion.Euler(-90f, 0f, -90f);
		}
	}

	public void Collide(Attackable target, Collider collider, Vector3 point, Vector3 normal, float impact = 0f)
	{
		if (HasTag(NormalItem.Tag.Explosive) && body.useGravity)
		{
			return;
		}
		if (target != null && (owner == null || owner.IsValidTarget(target)))
		{
			damage *= speedMultiplier;
			NormalItem.TagHit(tags, damage, owner, target, ranged: true, weapon);
			if (!isOnlyDamage && owner != null)
			{
				damage = owner.Attack(target, damage, ranged: true);
			}
			else
			{
				damage = target.Damage(damage);
			}
			if (damage > 0f)
			{
				WeaponCollision.ShowHitMarker(collider, base.transform.position);
			}
			if (!HasTag(NormalItem.Tag.Piercing))
			{
				isOnlyDamage = true;
				Object.Destroy(base.gameObject);
			}
		}
		else if (HasTag(NormalItem.Tag.Bouncy) && damage >= 1.5f)
		{
			Vector3 velocity = body.velocity;
			base.transform.RotateAround(point, Vector3.Cross(normal, velocity), 2f * Vector3.Angle(velocity, normal));
			Velocity = Vector3.Reflect(velocity, normal);
			damage /= 1.5f;
		}
		else if (HasTag(NormalItem.Tag.Explosive) && impact < 10f)
		{
			body.useGravity = true;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator OnBecameVisible()
	{
		if (sprites == null || sprites.Length <= 1 || !TryGetComponent<SpriteRenderer>(out var renderer))
		{
			yield break;
		}
		int index = 0;
		WaitForSeconds delay = new WaitForSeconds(0.25f);
		while (true)
		{
			index %= sprites.Length;
			renderer.sprite = sprites[index++];
			yield return delay;
		}
	}

	private void FixedUpdate()
	{
		if (HasTag(NormalItem.Tag.Rocket))
		{
			Vector3 force = base.transform.right * 37.5f;
			body.AddForce(force, ForceMode.Acceleration);
			speedMultiplier += Time.fixedDeltaTime * 1f;
		}
		if (HasTag(NormalItem.Tag.Seeking))
		{
			Vector3 force2 = base.transform.right * 18.75f;
			body.AddForce(force2, ForceMode.Acceleration);
			speedMultiplier += Time.fixedDeltaTime * 0.5f;
			Vector3 velocity = body.velocity;
			if (TryGetNearestTarget(ref target))
			{
				Vector3 b = velocity.magnitude * (target - base.transform.position).normalized;
				Velocity = Vector3.Slerp(velocity, b, Time.fixedDeltaTime * 8f);
			}
		}
		else
		{
			Rotate(body.velocity);
		}
	}

	private bool TryGetNearestTarget(ref Vector3 result)
	{
		targets.RemoveAll((Collider collider) => collider == null);
		float num = float.PositiveInfinity;
		Collider collider2 = null;
		foreach (Collider target in targets)
		{
			float sqrMagnitude = (target.transform.position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				collider2 = target;
			}
		}
		if ((object)collider2 == null)
		{
			return false;
		}
		result = collider2.bounds.center;
		return true;
	}

	private void OnDestroy()
	{
		if (!isOnlyDamage)
		{
			float num = GetTagCount();
			if (HasTag(NormalItem.Tag.Explosive))
			{
				Attacking.Explosion(base.transform.position, damage / num, 6f / num);
			}
			if (HasTag(NormalItem.Tag.Fragile))
			{
				Transform transform = base.transform;
				NormalItem.Fragments(owner, damage, 3, new Bounds(transform.position - 0.1f * transform.right, Vector3.zero));
			}
		}
	}
}
