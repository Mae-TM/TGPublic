using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
	private Action<Attackable> action;

	private readonly HashSet<Attackable> hit = new HashSet<Attackable>();

	private bool singleUse;

	public static void Add(ParticleSystem system, Action<Attackable> action)
	{
		ParticleSystem.CollisionModule collision = system.collision;
		collision.enabled = true;
		system.gameObject.AddComponent<ParticleCollision>().action = action;
	}

	public static void SetSprites(ParticleSystem system, Sprite[] sprites)
	{
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = system.textureSheetAnimation;
		textureSheetAnimation.enabled = true;
		textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
		foreach (Sprite sprite in sprites)
		{
			textureSheetAnimation.AddSprite(sprite);
		}
		textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0f, sprites.Length);
	}

	public static ParticleSystem Ring(GameObject source, Color color, float duration, float radius, float arc = 360f, Vector3 forward = default(Vector3), Vector3? position = null, bool reverse = false)
	{
		Vector3 up = source.transform.up;
		position = (position ?? ModelUtility.GetBottom(source)) + 0.5f * up;
		Quaternion rotation = Quaternion.AngleAxis((0f - arc) / 2f, up);
		if (forward == Vector3.zero)
		{
			rotation *= source.transform.rotation;
		}
		else
		{
			rotation *= Quaternion.LookRotation(forward, up);
		}
		ParticleSystem particleSystem = UnityEngine.Object.Instantiate(Resources.Load<ParticleSystem>("Ring"), position.Value, rotation);
		ParticleSystem.MainModule main = particleSystem.main;
		main.startLifetime = duration;
		main.duration = duration;
		main.startSpeed = radius / duration * (float)((!reverse) ? 1 : (-1));
		main.startColor = color;
		ParticleSystem.ShapeModule shape = particleSystem.shape;
		shape.arc = arc;
		if (reverse)
		{
			shape.radius = radius;
		}
		else
		{
			shape.radius = Vector3.ProjectOnPlane(ModelUtility.GetBounds(source).extents, up).magnitude + 0.5f;
		}
		particleSystem.Play();
		return particleSystem;
	}

	public static ParticleSystem Spot(Vector3 position, Quaternion rotation, Sprite sprite, float duration, IEnumerable<Attackable> targets, Action<Attackable> action)
	{
		ParticleSystem particleSystem = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Particles").GetComponentInChildren<ParticleSystem>(), position, rotation);
		UnityEngine.Object.Destroy(particleSystem.gameObject, duration);
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		emission.rateOverTime = 5f;
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = particleSystem.textureSheetAnimation;
		textureSheetAnimation.AddSprite(sprite);
		textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0f, 1f);
		ParticleSystem.ShapeModule shape = particleSystem.shape;
		shape.position = Vector3.zero;
		shape.scale = 0.5f * Vector3.one;
		ParticleSystem.CollisionModule collision = particleSystem.collision;
		collision.enabled = true;
		ParticleCollision particleCollision = particleSystem.gameObject.AddComponent<ParticleCollision>();
		particleCollision.action = action;
		particleCollision.singleUse = true;
		particleCollision.hit.UnionWith(targets);
		particleSystem.Play();
		return particleSystem;
	}

	private void OnParticleCollision(GameObject other)
	{
		Attackable componentInParent = other.GetComponentInParent<Attackable>();
		if (componentInParent != null && (singleUse ? hit.Contains(componentInParent) : hit.Add(componentInParent)))
		{
			action(componentInParent);
			if (singleUse)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
