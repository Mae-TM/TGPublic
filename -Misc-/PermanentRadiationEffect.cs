using UnityEngine;

public class PermanentRadiationEffect : MonoBehaviour
{
	private static Material spriteMat;

	private static Sprite[] sprites;

	public float radius;

	public float intensity;

	public float period;

	private float lastTime;

	private void Awake()
	{
		if (!TryGetComponent<Light>(out var component))
		{
			component = base.gameObject.AddComponent<Light>();
		}
		component.color = Color.green;
		if (TryGetComponent<Shadow>(out var component2))
		{
			Object.Destroy(component2.shadow.gameObject);
			Object.Destroy(component2);
		}
		if (spriteMat == null)
		{
			spriteMat = new Material(Shader.Find("Sprites/Default"));
		}
		if (sprites == null)
		{
			sprites = Resources.LoadAll<Sprite>("Effect/Radiation");
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.sharedMaterial.name == "Diffuse")
			{
				spriteRenderer.sharedMaterial = spriteMat;
			}
		}
		if (!TryGetComponent<ParticleManager>(out var component3))
		{
			component3 = base.gameObject.AddComponent<ParticleManager>();
			component3.prefab = Resources.Load<Transform>("Particles");
		}
		component3.AddParticles(sprites, intensity / period);
	}

	private void Update()
	{
		lastTime += Time.deltaTime;
		if (!(lastTime > period))
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		for (int i = 0; i < array.Length; i++)
		{
			Attackable componentInParent = array[i].GetComponentInParent<Attackable>();
			if (componentInParent != null)
			{
				componentInParent.Damage(intensity);
			}
		}
		lastTime -= period;
	}
}
