using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : StrifeAI, IComparable<Enemy>
{
	private static GameObject nameTagPrefab;

	protected override void Awake()
	{
		base.Awake();
		base.Faction.Parent = "Derse";
	}

	protected override void Start()
	{
		base.Start();
		switch (base.name)
		{
		case "Imp":
			base.HealthMax = 20f;
			if (UnityEngine.Random.value > 0.4f)
			{
				healthDrop = 0;
			}
			break;
		case "Lich":
			base.HealthMax = 25f;
			base.Defense = 3f;
			if (UnityEngine.Random.value > 0.5f)
			{
				healthDrop = 0;
			}
			break;
		case "Kheprit":
			base.HealthMax = 5f;
			HealthRegen = 0f;
			break;
		}
		base.Health = base.HealthMax;
		if (base.GristType == 0)
		{
			UnityEngine.Random.InitState((int)base.netId);
			Aspect type = (Aspect)UnityEngine.Random.Range(0, 12);
			base.GristType = Grist.GetIndex(0, type);
		}
		if (base.Faction.IsChildOf("Derse"))
		{
			SetMaterial();
		}
		if (base.name.EndsWith("(Clone)"))
		{
			base.name = base.name.Remove(base.name.Length - "(Clone)".Length);
		}
		MakeNameTag();
		UnityEngine.Object.Destroy(base.transform.GetComponentInChildren<MeshRenderer>());
		UnityEngine.Object.Destroy(base.transform.GetComponentInChildren<MeshFilter>());
	}

	private void MakeNameTag()
	{
		if (nameTagPrefab == null)
		{
			nameTagPrefab = Resources.Load<GameObject>("Name tag");
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(nameTagPrefab);
		gameObject.transform.GetChild(1).GetComponent<Text>().text = Grist.GetName(base.GristType) + " " + base.name;
		((HealthVialBasic)healthVial).SetNameTag(gameObject);
	}

	protected Material GetMaterial()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.material.shader.name == "Custom/litHSVShader")
			{
				return spriteRenderer.sharedMaterial;
			}
		}
		return null;
	}

	public void SetMaterial(Material mat = null)
	{
		if (mat == null)
		{
			mat = Grist.GetMaterial(base.GristType);
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.material.shader.name == "Custom/litHSVShader")
			{
				spriteRenderer.sharedMaterial = mat;
			}
		}
	}

	protected override void HandleIdle()
	{
		if (base.Faction.IsChildOf("Prospit"))
		{
			Player player = GetNearestPlayers(10f).FirstOrDefault();
			if ((object)player != null)
			{
				navAgent.destination = player.transform.position;
				return;
			}
		}
		base.HandleIdle();
	}

	public static int EnemyCount()
	{
		return ((Faction)"Derse").GetMembers().Count();
	}

	public virtual int GetCost()
	{
		return base.name switch
		{
			"Imp" => 1, 
			"Lich" => 4, 
			_ => 1, 
		};
	}

	public int CompareTo(Enemy other)
	{
		return GetCost().CompareTo(other.GetCost());
	}

	protected override HouseData.Attackable SaveSpecific()
	{
		return new HouseData.Enemy
		{
			type = base.GristType
		};
	}

	protected override void LoadSpecific(HouseData.Attackable data)
	{
		base.GristType = ((HouseData.Enemy)data).type;
	}

	private void MirrorProcessed()
	{
	}
}
