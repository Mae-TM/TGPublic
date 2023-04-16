using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour, AutoPickup
{
	public enum TrapEffect
	{
		Slow,
		Burning,
		Radiation,
		Poison,
		Weaken
	}

	public float damage;

	public float effectDuration;

	public Animator anim;

	public AudioSource audio;

	public bool continuousDamage;

	public bool pushesPlayer;

	public bool resetsAfterSpring;

	private bool canTrigger = true;

	private bool active;

	public TrapEffect trapEffect;

	public bool areaOfEffect;

	public bool spawnsEnemies;

	private int trapEffectIntensity;

	private int trapEffectPeriod;

	private ParticleSystem ps;

	private bool isEffectCloudActive;

	private float effectCloudStartTime;

	private Player triggerer;

	private static Sprite[] sprites;

	public void Pickup(Player player)
	{
		if (!canTrigger)
		{
			return;
		}
		triggerer = player;
		player.Damage(damage);
		if (effectDuration > 0f)
		{
			trapEffectIntensity = 5;
			trapEffectPeriod = 1;
			ApplyEffect(player, trapEffect, effectDuration, trapEffectIntensity, trapEffectPeriod);
			if (areaOfEffect)
			{
				MakeEffectCloud();
			}
			player.SetPosition(base.transform.position);
		}
		if (spawnsEnemies)
		{
			SpawnEnemies();
		}
		if (!pushesPlayer)
		{
			Spring();
		}
	}

	public void Spring()
	{
		if (anim != null)
		{
			anim.SetTrigger("sprung");
			active = true;
		}
		if (audio != null)
		{
			audio.Play();
		}
		if (!continuousDamage && !resetsAfterSpring && !isEffectCloudActive)
		{
			Object.Destroy(this);
		}
		if (resetsAfterSpring)
		{
			canTrigger = false;
			StartCoroutine(StartWait());
		}
	}

	private IEnumerator StartWait()
	{
		yield return StartCoroutine(Wait(1f));
		ResetTrap();
	}

	private IEnumerator Wait(float seconds)
	{
		yield return new WaitForSeconds(seconds);
	}

	public void ResetTrap()
	{
		if (anim != null)
		{
			anim.SetTrigger("reset");
		}
		active = false;
		canTrigger = true;
	}

	public void Update()
	{
		if (resetsAfterSpring && !canTrigger && !active)
		{
			canTrigger = true;
		}
		if (!isEffectCloudActive)
		{
			return;
		}
		if (effectCloudStartTime == 0f)
		{
			effectCloudStartTime = Time.time;
		}
		if (Time.time >= effectCloudStartTime + effectDuration)
		{
			isEffectCloudActive = false;
			Object.Destroy(this);
			return;
		}
		HashSet<Attackable> hashSet = new HashSet<Attackable>();
		Collider[] array = Physics.OverlapSphere(base.transform.position, 5f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetComponent<Attackable>(out var component))
			{
				hashSet.Add(component);
			}
		}
		foreach (Attackable item in hashSet)
		{
			if (item != null && !(item == triggerer.GetComponent<Attackable>()))
			{
				ApplyEffect(item, trapEffect, effectDuration, trapEffectIntensity, trapEffectPeriod);
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!canTrigger)
		{
			return;
		}
		if (!pushesPlayer)
		{
			Player componentInParent = GetComponentInParent<Player>();
			if (componentInParent != null && componentInParent.self)
			{
				return;
			}
		}
		canTrigger = false;
		if (pushesPlayer)
		{
			Attacking.Explosion(base.transform.position, 0f, 15f, null, 1f, visual: false);
		}
		Spring();
	}

	public void ApplyEffect(Attackable target, TrapEffect effect, float duration, float intensity, float period)
	{
		switch (effect)
		{
		case TrapEffect.Burning:
			if (target.GetEffect<BurningEffect>() == null)
			{
				target.Affect(new BurningEffect(duration, intensity, period));
			}
			break;
		case TrapEffect.Slow:
			target.Affect(new SlowEffect(duration, 100f));
			break;
		case TrapEffect.Radiation:
			target.Affect(new RadiationEffect(duration, 10f, intensity, period));
			break;
		case TrapEffect.Poison:
			target.Affect(new PoisonEffect(duration, intensity, period));
			break;
		case TrapEffect.Weaken:
			target.Affect(new WeakenEffect(duration, intensity, period));
			break;
		}
	}

	public void MakeEffectCloud()
	{
		isEffectCloudActive = true;
		ps = GetComponentInParent<ParticleSystem>();
		ParticleSystem.MainModule main = ps.main;
		main.maxParticles = 1000;
		main.duration = effectDuration;
		if (sprites == null)
		{
			sprites = Resources.LoadAll<Sprite>("Effect/Fire");
		}
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = ps.textureSheetAnimation;
		while (textureSheetAnimation.spriteCount != 0)
		{
			textureSheetAnimation.RemoveSprite(0);
		}
		Sprite[] array = sprites;
		foreach (Sprite sprite in array)
		{
			textureSheetAnimation.AddSprite(sprite);
		}
		textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0f, sprites.Length);
		ps.Play();
		canTrigger = false;
	}

	public void SpawnEnemies()
	{
		WorldArea component = base.transform.root.GetComponent<WorldArea>();
		Enemy[] creatures = SpawnHelper.instance.GetCreatures<Enemy>(new string[5] { "Giclops", "Ogre", "Basilisk", "Imp", "Lich" });
		int num = Mathf.CeilToInt(1f * 26f);
		while (Random.Range(0, num + 1) != 0)
		{
			Enemy[] array = creatures;
			foreach (Enemy enemy in array)
			{
				int cost = enemy.GetCost();
				if (num > cost && Random.Range(0, 2) == 0)
				{
					SpawnHelper.instance.Spawn(enemy, component, base.transform.position);
					num -= cost;
				}
			}
		}
	}
}
