using System;
using System.Collections.Generic;
using UnityEngine;

public class Guardian : StrifeAI
{
	private enum Type
	{
		Dad,
		Mom,
		Count
	}

	private static AssetBundle bundle;

	private static int count;

	[SerializeField]
	private GUIStyle noteStyle;

	[SerializeField]
	private SpriteRenderer shirt;

	[SerializeField]
	private SpriteRenderer backShirt;

	[SerializeField]
	private SpriteRenderer torso;

	[SerializeField]
	private SpriteRenderer backTorso;

	[SerializeField]
	private SpriteRenderer head;

	[SerializeField]
	private SpriteRenderer backHead;

	[SerializeField]
	private SpriteRenderer hair;

	[SerializeField]
	private SpriteRenderer backHair;

	[SerializeField]
	private SpriteRenderer glasses;

	[SerializeField]
	private SpriteRenderer backGlasses;

	[SerializeField]
	private CustomCharacter cust;

	private bool stern;

	private Type guardianType;

	private float walkTime;

	public string Name => guardianType.ToString();

	public event Action OnDefeat;

	protected override void Start()
	{
		base.Start();
		base.Faction.Parent = "Prospit";
		base.HealthMax = 40f;
		base.Health = base.HealthMax;
		base.OnHurt += OnDamage;
		Player owner = GetComponentInParent<House>().Owner;
		if (owner != null)
		{
			if (!NetcodeManager.Instance.offline)
			{
				SessionRandom.Seed(owner.sync.np.id);
			}
			hair.color = owner.sync.np.character.whiteHair * Color.white + Color.black;
			backHair.color = hair.color;
		}
		else
		{
			Debug.LogWarning("Guardian could not find corresponding player!");
		}
		glasses.enabled = UnityEngine.Random.Range(0, 2) == 0;
		backGlasses.enabled = glasses.enabled;
		guardianType = (Type)UnityEngine.Random.Range(0, 2);
		stern = UnityEngine.Random.Range(0, 2) == 0;
		Sprite[] array = bundle.LoadAssetWithSubAssets<Sprite>(GetGuardianName());
		hair.sprite = array[1];
		backHair.sprite = array[4];
		shirt.sprite = array[6];
		backShirt.sprite = array[8];
		cust.SetSpriteSheet(1, array);
		cust.SetSpriteSheet(3, array);
		array = bundle.LoadAssetWithSubAssets<Sprite>(guardianType.ToString() + "base");
		head.sprite = array[1];
		backHead.sprite = array[4];
		torso.sprite = array[6];
		backTorso.sprite = array[8];
		cust.SetSpriteSheet(0, array);
		cust.SetSpriteSheet(2, array);
		if (glasses.enabled)
		{
			array = bundle.LoadAssetWithSubAssets<Sprite>(guardianType.ToString() + "glasses");
			glasses.sprite = array[1];
			backGlasses.sprite = array[4];
		}
		clip = bundle.LoadAsset<AudioClip>(GetGuardianName());
	}

	private string GetGuardianName()
	{
		if (!stern)
		{
			return guardianType.ToString() + "smothering";
		}
		return guardianType.ToString() + "stern";
	}

	protected override void Awake()
	{
		if (bundle == null)
		{
			bundle = AssetBundleExtensions.Load("guardians");
		}
		count++;
		base.Awake();
	}

	private void OnDestroy()
	{
		if (--count <= 0)
		{
			bundle.Unload(unloadAllLoadedObjects: true);
			bundle = null;
		}
	}

	protected new void Update()
	{
		if ((target == null || !(target is Enemy)) && !flee)
		{
			if (base.transform.parent == null)
			{
				return;
			}
			Enemy componentInChildren = base.transform.parent.gameObject.GetComponentInChildren<Enemy>();
			if (componentInChildren != null)
			{
				target = componentInChildren;
			}
		}
		base.Update();
		if (!navAgent.isOnNavMesh || (!(target != null) && !flee) || !base.IsInStrife || flee)
		{
			return;
		}
		if (base.Health < 10f && target == Player.player)
		{
			if (Exile.SetAction(Exile.Action.victory))
			{
				Bounds bounds = base.Collider.bounds;
				Item item = new NormalItem("CD000000");
				DropLoot(item, bounds);
				Item item2 = new NormalItem("Nooo!!te");
				Book book = item2.SceneObject.GetComponent<Interactable>().AddOption<Book>();
				book.file = (stern ? "SternStrife" : "SmotheringStrife");
				book.vars = new string[2]
				{
					Player.player.sylladex.PlayerName.ToUpper(),
					(guardianType == Type.Dad) ? "Daddy" : "Mommy"
				};
				book.font = noteStyle.font;
				book.fontSize = noteStyle.fontSize;
				book.alignment = noteStyle.alignment;
				DropLoot(item2, bounds);
			}
			this.OnDefeat?.Invoke();
		}
		if (base.Health < 10f || (target is Player && target.Health < AttackDamage * 2f))
		{
			Transform transform = FindNearestExit(100f);
			if (transform != null)
			{
				navAgent.SetDestination(transform.position);
				navAgent.isStopped = false;
				flee = true;
				LeaveStrife();
				target = null;
				OnStrifeEnd();
			}
		}
	}

	protected override void HandleIdle()
	{
		base.HandleIdle();
		navAgent.isStopped = false;
		if ((navAgent.remainingDistance < 0.75f || !navAgent.hasPath) && !navAgent.pathPending)
		{
			navAgent.isStopped = true;
			walkTime -= Time.deltaTime;
		}
		if (Time.time > walkTime)
		{
			walkTime = Time.time + 8f;
			Furniture[] componentsInChildren = base.transform.parent.GetComponentsInChildren<Furniture>();
			GetNewTarget(componentsInChildren);
		}
	}

	private static void OnDamage(Attack attack)
	{
		if (attack.source == Player.player)
		{
			Exile.StopAction(Exile.Action.guardian);
		}
	}

	private new void OnCollisionEnter(Collision collision)
	{
		Enemy component = collision.gameObject.GetComponent<Enemy>();
		if (component != null && !base.IsInStrife)
		{
			target = component;
			EngageStrife(component);
		}
		else
		{
			base.OnCollisionEnter(collision);
		}
	}

	private void GetNewTarget(IReadOnlyList<Component> objects)
	{
		if (objects.Count == 0)
		{
			return;
		}
		float num = float.PositiveInfinity;
		int num2 = 0;
		Vector3 position = base.transform.position;
		for (int i = 0; i < objects.Count; i++)
		{
			float sqrMagnitude = (objects[i].transform.position - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				num2 = i;
			}
		}
		Component component = objects[++num2 % objects.Count];
		navAgent.SetDestination(component.transform.position);
	}

	private void MirrorProcessed()
	{
	}
}
