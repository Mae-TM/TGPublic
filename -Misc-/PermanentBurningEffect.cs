using UnityEngine;

public class PermanentBurningEffect : MonoBehaviour, AutoPickup
{
	private static Material spriteMat;

	private static Sprite[] sprites;

	public float duration;

	public float intensity;

	public float period;

	public bool includeEffects = true;

	private void Awake()
	{
		if (!includeEffects)
		{
			return;
		}
		base.gameObject.AddComponent<Light>().color = new Color(1f, 0.5f, 0f);
		if (TryGetComponent<Shadow>(out var component))
		{
			Object.Destroy(component.shadow.gameObject);
			Object.Destroy(component);
		}
		if (spriteMat == null)
		{
			spriteMat = new Material(Shader.Find("Sprites/Default"));
		}
		if (sprites == null)
		{
			sprites = Resources.LoadAll<Sprite>("Effect/Fire");
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.sharedMaterial.name == "Diffuse")
			{
				spriteRenderer.sharedMaterial = spriteMat;
			}
		}
		if (!TryGetComponent<ParticleManager>(out var component2))
		{
			component2 = base.gameObject.AddComponent<ParticleManager>();
			component2.prefab = Resources.Load<Transform>("Particles");
		}
		component2.AddParticles(sprites, intensity / period);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Attackable componentInParent = collision.gameObject.GetComponentInParent<Attackable>();
		if (componentInParent != null)
		{
			componentInParent.Affect(new BurningEffect(duration, intensity, period), stacking: false);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		Attackable componentInParent = other.GetComponentInParent<Attackable>();
		if (componentInParent != null)
		{
			componentInParent.Affect(new BurningEffect(duration, intensity, period), stacking: false);
		}
	}

	public void Pickup(Player player)
	{
		player.Affect(new BurningEffect(duration, intensity, period), stacking: false);
	}
}
