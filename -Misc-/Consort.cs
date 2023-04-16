using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Consort : Attackable
{
	public enum Species
	{
		Salamander,
		Turtle,
		Nakodile,
		Iguana,
		Count
	}

	public enum Job
	{
		None,
		Merchant
	}

	[SerializeField]
	private Sprite[] nameTagSprites;

	[SerializeField]
	private SpriteRenderer nameTag;

	private NavMeshAgent agent;

	private SpeakAction speak;

	private Species species = Species.Count;

	private Job job;

	protected override IEnumerable<Item> Loot => Enumerable.Repeat(new NormalItem(species switch
	{
		Species.Salamander => "salamand", 
		Species.Turtle => "turtleCO", 
		Species.Nakodile => "nakkdile", 
		Species.Iguana => "iguanaCO", 
		_ => throw new InvalidOperationException(), 
	}), 1);

	protected override void Awake()
	{
		speak = base.gameObject.AddComponent<SpeakAction>();
		base.Awake();
	}

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		LocalNavMeshBuilder component = base.transform.root.GetComponent<LocalNavMeshBuilder>();
		if (component != null)
		{
			component.EnableOnMeshBuilt(agent);
		}
		else
		{
			agent.enabled = true;
		}
		base.OnHurt += OnDamage;
	}

	private void OnTransformParentChanged()
	{
		if (species != Species.Count || base.transform.parent == null)
		{
			return;
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		Material material = new Material(componentsInChildren[0].material);
		House component = base.transform.root.GetComponent<House>();
		if (!(component == null))
		{
			if (component.planet == null)
			{
				UnityEngine.Random.InitState((int)base.netId);
				species = (Species)UnityEngine.Random.Range(0, 4);
				material.SetFloat("_HueShift", UnityEngine.Random.Range(0f, 360f));
				material.SetFloat("_Sat", UnityEngine.Random.Range(0.5f, 1f));
				material.SetFloat("_Val", UnityEngine.Random.Range(0.5f, 1f));
			}
			else
			{
				species = component.planet.consorts;
				material.SetFloat("_HueShift", component.planet.hue);
				material.SetFloat("_Sat", component.planet.saturation);
				material.SetFloat("_Val", component.planet.value);
			}
			GetComponentInChildren<Animator>().SetInteger("Type", (int)(species + 1));
			SpriteRenderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material = material;
			}
			speak.Set(Dialogue.GetDialoguePath($"Consort/{species}/{job}") ?? $"Consort/{species}", material);
		}
	}

	private void OnDamage(Attack attack)
	{
		if (attack.source != null)
		{
			agent.SetDestination(base.transform.position - 8f * (attack.source.transform.position - base.transform.position).normalized);
		}
	}

	protected override void OnEnable()
	{
		if (species != Species.Count)
		{
			GetComponentInChildren<Animator>().SetInteger("Type", (int)(species + 1));
		}
		base.OnEnable();
	}

	public void SetJob(Job to)
	{
		if (job != to)
		{
			if (job != 0)
			{
				throw new Exception($"Tried to give consort {base.netId} job {to}, but it already has job {job}!");
			}
			job = to;
			if (job == Job.Merchant)
			{
				nameTag.transform.parent.gameObject.SetActive(value: true);
				nameTag.sprite = nameTagSprites[(long)base.netId % (long)nameTagSprites.Length];
			}
		}
	}

	protected override HouseData.Attackable SaveSpecific()
	{
		return new HouseData.Consort
		{
			job = job,
			quests = speak.Save().ToArray()
		};
	}

	protected override void LoadSpecific(HouseData.Attackable data)
	{
		HouseData.Consort consort = (HouseData.Consort)data;
		SetJob(consort.job);
		speak.Load(consort.quests);
	}

	private void MirrorProcessed()
	{
	}
}
