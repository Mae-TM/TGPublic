using System;
using Mirror;
using ProtoBuf;
using UnityEngine;

public abstract class StatusEffect : IStatusEffect
{
	[ProtoContract]
	public struct Data : NetworkMessage
	{
		[ProtoMember(1)]
		public string type;

		[ProtoMember(2)]
		public byte[] protoData;

		[ProtoMember(3)]
		public float endTime;
	}

	public float EndTime { get; private set; }

	public static Data Save(IStatusEffect effect)
	{
		Data result = default(Data);
		result.type = effect.GetType().FullName;
		result.protoData = ProtobufHelpers.ProtoSerialize(effect);
		result.endTime = ((effect is StatusEffect) ? (effect.EndTime - Time.time) : 0f);
		return result;
	}

	public static IStatusEffect Load(Data data)
	{
		Type type = Type.GetType(data.type);
		IStatusEffect obj = (IStatusEffect)ProtobufHelpers.ProtoDeserialize(data.protoData, type);
		if (obj is StatusEffect statusEffect)
		{
			statusEffect.EndTime = data.endTime + Time.time;
		}
		return obj;
	}

	public static T Clone<T>(T effect) where T : IStatusEffect
	{
		T val = Serializer.DeepClone(effect);
		if ((object)val is StatusEffect statusEffect)
		{
			statusEffect.EndTime = effect.EndTime;
		}
		return val;
	}

	protected StatusEffect(float duration)
	{
		EndTime = Time.time + duration;
	}

	public void ReduceTime(float factor)
	{
		if (factor < 1f)
		{
			throw new ArgumentException("factor should be at least 1!");
		}
		EndTime = Time.time + (EndTime - Time.time) / factor;
	}

	public void Stop(Attackable att)
	{
		EndTime = 0f;
		End(att);
	}

	public virtual void Begin(Attackable att)
	{
	}

	public virtual float Update(Attackable att)
	{
		return float.PositiveInfinity;
	}

	public virtual bool OnAttack(Attack attack)
	{
		return false;
	}

	public virtual bool AfterAttack(Attack attack)
	{
		return false;
	}

	public virtual bool OnAttacked(Attack attack)
	{
		return false;
	}

	public virtual void End(Attackable att)
	{
	}

	protected static void AddParticles(Attackable att, Sprite[] sprites, float intensity = 2f, bool light = false)
	{
		if (att.TryGetComponent<ParticleManager>(out var component))
		{
			component.AddParticles(sprites, intensity, light);
		}
	}

	protected static void RemoveParticles(Attackable att, Sprite[] sprites, float intensity = 2f)
	{
		if (att.TryGetComponent<ParticleManager>(out var component))
		{
			component.AddParticles(sprites, 0f - intensity);
		}
	}

	protected static SpriteRenderer MakeShield(Attackable att, SpriteRenderer prefab, PBColor? color = null)
	{
		Bounds bounds = ModelUtility.GetBounds(att.gameObject);
		SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate(prefab, bounds.center, Quaternion.identity);
		Transform transform = spriteRenderer.transform;
		transform.localScale = Vector3.Scale(bounds.size, transform.localScale);
		BillboardSprite componentInChildren = att.GetComponentInChildren<BillboardSprite>();
		if (componentInChildren == null)
		{
			transform.SetParent(att.transform, worldPositionStays: true);
			spriteRenderer.gameObject.AddComponent<BillboardSprite>();
		}
		else
		{
			transform.SetParent(componentInChildren.transform, worldPositionStays: true);
			transform.localRotation = Quaternion.identity;
		}
		if (color.HasValue)
		{
			spriteRenderer.material = ImageEffects.SetShiftColor(spriteRenderer.material, color.Value);
		}
		return spriteRenderer;
	}
}
