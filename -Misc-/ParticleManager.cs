using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
	public Transform prefab;

	[SerializeField]
	private bool calculateBounds = true;

	private readonly Stack<ParticleSystem[]> freeSystems = new Stack<ParticleSystem[]>();

	private readonly IDictionary<Sprite[], ParticleSystem[]> usedSystems = new Dictionary<Sprite[], ParticleSystem[]>();

	private Transform parent;

	private Transform Parent
	{
		get
		{
			if ((object)parent == null)
			{
				parent = new GameObject("Particles").transform;
				if (calculateBounds)
				{
					Bounds bounds = ModelUtility.GetBounds(base.gameObject);
					SetBounds(bounds.center, bounds.size);
				}
				parent.SetParent(base.transform, worldPositionStays: true);
				parent.localRotation = Quaternion.identity;
			}
			return parent;
		}
	}

	public void SetBounds(Vector3 center, Vector3 size)
	{
		Parent.localPosition = center;
		Parent.localScale = size;
	}

	private void OnEnable()
	{
		foreach (KeyValuePair<Sprite[], ParticleSystem[]> usedSystem in usedSystems)
		{
			ParticleSystem[] value = usedSystem.Value;
			for (int i = 0; i < value.Length; i++)
			{
				value[i].Play();
			}
		}
	}

	private void OnDisable()
	{
		foreach (KeyValuePair<Sprite[], ParticleSystem[]> usedSystem in usedSystems)
		{
			ParticleSystem[] value = usedSystem.Value;
			for (int i = 0; i < value.Length; i++)
			{
				value[i].Stop();
			}
		}
	}

	public void AddParticles(Sprite[] sprites, float intensity = 0f, bool light = false)
	{
		ParticleSystem[] value;
		bool flag = !usedSystems.TryGetValue(sprites, out value);
		if (flag)
		{
			value = ((freeSystems.Count != 0) ? freeSystems.Pop() : Object.Instantiate(prefab, Parent, worldPositionStays: false).GetComponentsInChildren<ParticleSystem>());
			usedSystems.Add(sprites, value);
		}
		ParticleSystem[] array = value;
		foreach (ParticleSystem particleSystem in array)
		{
			if (intensity != 0f)
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
				float num = intensity + (flag ? 0f : (velocityOverLifetime.yMultiplier * rateOverTime.constant));
				if (Mathf.Approximately(num, 0f))
				{
					usedSystems.Remove(sprites);
					FreeParticleSystems(value);
					break;
				}
				velocityOverLifetime.yMultiplier = Mathf.Sign(num);
				rateOverTime.constant = Mathf.Abs(num);
				emission.rateOverTime = rateOverTime;
			}
			if (flag)
			{
				particleSystem.Clear();
				ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = particleSystem.textureSheetAnimation;
				while (textureSheetAnimation.spriteCount != 0)
				{
					textureSheetAnimation.RemoveSprite(0);
				}
				foreach (Sprite sprite in sprites)
				{
					textureSheetAnimation.AddSprite(sprite);
				}
				textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0f, sprites.Length);
			}
			ParticleSystem.LightsModule lights = particleSystem.lights;
			lights.enabled = light;
			if (base.enabled)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}
		}
	}

	private void FreeParticleSystems(ParticleSystem[] particleSystems)
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Stop();
		}
		freeSystems.Push(particleSystems);
	}

	public void RemoveParticles(Sprite[] sprites)
	{
		if (usedSystems.TryGetValue(sprites, out var value))
		{
			usedSystems.Remove(sprites);
			FreeParticleSystems(value);
		}
	}

	public void ClearParticles(bool remove = false)
	{
		foreach (KeyValuePair<Sprite[], ParticleSystem[]> usedSystem in usedSystems)
		{
			if (remove)
			{
				Object.Destroy(usedSystem.Value[0].transform.parent.gameObject);
			}
			else
			{
				FreeParticleSystems(usedSystem.Value);
			}
		}
		usedSystems.Clear();
	}
}
