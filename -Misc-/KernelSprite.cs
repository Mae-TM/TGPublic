using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

public class KernelSprite : NetworkBehaviour
{
	private static readonly List<AssetBundle> allProto;

	private static readonly List<AssetBundle> activeProto;

	[SerializeField]
	private AnimationCurve bobCurve;

	[SerializeField]
	private Sprite tier1Sprite;

	[SerializeField]
	private SpriteRenderer laser;

	private NavMeshAgent nma;

	private SpeakAction speak;

	private GameObject spriteHolder;

	private SpriteRenderer sprite;

	private SpriteRenderer front;

	private SpriteRenderer back;

	private SpriteRenderer head;

	private Player target;

	private Renderer targetRenderer;

	private RegionChild vis;

	private bool filled = true;

	private AssetBundle[] proto;

	private bool entered;

	private bool spoken;

	private float vim = 10f;

	private float vimmax = 10f;

	private float vimregen = 0.5f;

	private const float HEALSPEED = 5f;

	private const float HEALRANGE = 8f;

	public event Action OnPrototype;

	private void Awake()
	{
		nma = GetComponent<NavMeshAgent>();
		spriteHolder = base.transform.Find("SpriteHolder").gameObject;
		sprite = spriteHolder.GetComponent<SpriteRenderer>();
		front = spriteHolder.transform.Find("Front").GetComponent<SpriteRenderer>();
		back = spriteHolder.transform.Find("Back").GetComponent<SpriteRenderer>();
		head = spriteHolder.transform.Find("Head").GetComponent<SpriteRenderer>();
		vis = GetComponent<RegionChild>();
		vis.enabled = false;
	}

	private void Start()
	{
		PBColor pBColor = ColorSelector.GetCruxiteColor(target.sync.np.character.color);
		Material material = new Material(GetComponentInChildren<SpriteRenderer>().material);
		material.SetFloat("_HueShift", pBColor.h * 360f);
		material.SetFloat("_Sat", pBColor.s);
		material.SetFloat("_Val", pBColor.v);
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = material;
		}
		spriteHolder.GetComponent<Light>().color = pBColor;
	}

	private void OnDestroy()
	{
		if (proto == null)
		{
			return;
		}
		AssetBundle[] array = proto;
		foreach (AssetBundle assetBundle in array)
		{
			if (assetBundle == null)
			{
				continue;
			}
			if (entered)
			{
				activeProto.Remove(assetBundle);
			}
			int num = allProto.IndexOf(assetBundle);
			if (num == -1)
			{
				Debug.LogWarning("Could not find prototype bundle " + assetBundle.name + "!");
				continue;
			}
			if (allProto.LastIndexOf(assetBundle) == num)
			{
				proto[0].Unload(unloadAllLoadedObjects: true);
			}
			allProto.RemoveAt(num);
		}
	}

	public bool Warp()
	{
		if (!(target.RegionChild.Area is House house) || house.Owner != target)
		{
			return false;
		}
		if ((bool)target.GetComponentInParent<Planet>())
		{
			return false;
		}
		vis.Area = target.RegionChild.Area;
		vis.SetRegion(target.RegionChild.Region);
		if (NavMesh.SamplePosition(target.transform.position + target.transform.right * 2f, out var hit, 8f, -1))
		{
			return nma.Warp(hit.position);
		}
		return false;
	}

	private void Update()
	{
		if (((Vector3.SqrMagnitude(base.transform.position - target.transform.position) > 6400f || base.transform.parent != target.transform.parent) && !Warp()) || !nma.isOnNavMesh)
		{
			return;
		}
		nma.isStopped = false;
		bool flag = true;
		if (proto == null)
		{
			flag = !AttemptPrototyping();
		}
		else if (entered && !spoken)
		{
			if ((base.transform.position - target.transform.position).sqrMagnitude >= 4f)
			{
				nma.SetDestination(target.transform.position);
			}
			else
			{
				spoken = true;
				if (target.self)
				{
					speak = base.gameObject.AddComponent<SpeakAction>();
					Material sharedMaterial = GetComponentInChildren<SpriteRenderer>().sharedMaterial;
					string text = GetPrototype()[0];
					speak.Set("Sprite/" + text + "/interact", sharedMaterial);
					Dialogue.StartDialogue(Dialogue.GetDialoguePath("Sprite/" + text + "/start"), sharedMaterial, spriteHolder.transform);
				}
			}
			flag = false;
		}
		if (flag)
		{
			if ((base.transform.position - target.transform.position).sqrMagnitude < 64f)
			{
				nma.isStopped = true;
			}
			else if (nma.isOnNavMesh)
			{
				nma.SetDestination(target.transform.position);
			}
		}
		spriteHolder.transform.localPosition = new Vector3(0f, bobCurve.Evaluate(Time.time));
		float num = Vector3.Dot(nma.velocity, MSPAOrthoController.main.transform.right);
		if (Mathf.Abs(num) > nma.speed / 5f)
		{
			sprite.transform.localScale = new Vector3((num < 0f) ? (-1f) : 1f, 1f, 1f);
		}
		if (vim + vimregen * Time.deltaTime < vimmax)
		{
			vim += vimregen * Time.deltaTime;
		}
		else
		{
			vim = vimmax;
		}
		float num2 = Mathf.Min(5f * Time.deltaTime, target.HealthMax - target.Health, vim);
		if (filled && num2 > 0f && (base.transform.position - target.transform.position).sqrMagnitude < 64f)
		{
			target.Health += num2;
			vim -= num2;
			laser.gameObject.SetActive(value: true);
		}
		else
		{
			filled = false;
			laser.gameObject.SetActive(value: false);
		}
		if (vim >= vimmax)
		{
			filled = true;
		}
	}

	private bool AttemptPrototyping()
	{
		float num = float.PositiveInfinity;
		Prototype prototype = null;
		foreach (Prototype item in Prototype.total)
		{
			if (item.gameObject.activeInHierarchy)
			{
				float sqrMagnitude = (item.transform.position - base.transform.position).sqrMagnitude;
				if (num > sqrMagnitude)
				{
					num = sqrMagnitude;
					prototype = item;
				}
			}
		}
		if (num < 1f)
		{
			if (base.isServer)
			{
				SetPrototype(new string[2] { prototype.protoName, null });
				UnityEngine.Object.Destroy(prototype.gameObject);
			}
			return true;
		}
		if (num < 16f)
		{
			NavMeshObstacle component = prototype.transform.GetComponent<NavMeshObstacle>();
			if (component != null)
			{
				component.enabled = false;
			}
			if (nma.SetDestination(prototype.transform.position))
			{
				return true;
			}
			if (component != null)
			{
				component.enabled = true;
			}
		}
		return false;
	}

	private void LateUpdate()
	{
		if (laser.gameObject.activeSelf)
		{
			Vector3 forward = MSPAOrthoController.main.transform.forward;
			Vector3 position = spriteHolder.transform.position;
			Vector3 vector = targetRenderer.bounds.center + forward * 0.2f;
			Transform obj = laser.transform;
			obj.position = vector;
			obj.LookAt(position, -forward);
			obj.Rotate(-90f, 90f, 0f);
			Vector3 vector2 = laser.size;
			vector2.x = Vector3.Distance(vector, position);
			laser.size = vector2;
		}
	}

	public void SetTarget(Player player)
	{
		target = player;
		targetRenderer = player.GetComponentInChildren<Renderer>();
	}

	[Server]
	public void SetPrototype(string[] name)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void KernelSprite::SetPrototype(System.String[])' called when server was not active");
			return;
		}
		RpcSetPrototype(name);
		SetPrototypeLocal(name);
	}

	[ClientRpc]
	private void RpcSetPrototype(string[] name)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EString_005B_005D(writer, name);
		SendRPCInternal(typeof(KernelSprite), "RpcSetPrototype", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void SetPrototypeLocal(string[] name)
	{
		if (name == null)
		{
			proto = null;
			return;
		}
		proto = new AssetBundle[name.Length];
		for (int i = 0; i < name.Length; i++)
		{
			if (name[i] == null)
			{
				proto[i] = null;
				continue;
			}
			string assetBundleName = "prototypes/" + name[i].ToLowerInvariant();
			proto[i] = allProto.Find((AssetBundle b) => b.name == assetBundleName) ?? AssetBundleExtensions.Load(assetBundleName);
			allProto.Add(proto[i]);
		}
		if (proto[0] != null)
		{
			front.sprite = proto[0].LoadAsset<Sprite>("Kernel");
		}
		spriteHolder.GetComponent<Animator>().SetTrigger("Prototyped");
		this.OnPrototype?.Invoke();
		if (target == Player.player)
		{
			Exile.SetAction(Exile.Action.sprite, instant: true, repeat: false);
		}
	}

	public string[] GetPrototype()
	{
		if (proto == null)
		{
			return null;
		}
		string[] array = new string[proto.Length];
		for (int i = 0; i < proto.Length; i++)
		{
			if (proto[i] == null)
			{
				array[i] = null;
			}
			else
			{
				array[i] = proto[i].name.Substring("prototypes/".Length);
			}
		}
		return array;
	}

	public void Entered(bool fromload = false)
	{
		if (entered)
		{
			return;
		}
		entered = true;
		if (proto == null && base.isServer)
		{
			float num = float.PositiveInfinity;
			Prototype prototype = null;
			foreach (Prototype item in Prototype.total)
			{
				float sqrMagnitude = (base.transform.position - item.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					prototype = item;
				}
			}
			if (prototype == null)
			{
				Debug.LogWarning("Could not find anything to prototype with!");
				return;
			}
			SetPrototype(new string[2] { prototype.protoName, null });
			UnityEngine.Object.Destroy(prototype.gameObject);
		}
		if (proto == null)
		{
			Debug.LogWarning("Prototype is null!");
			return;
		}
		Sprite[] array = proto[0].LoadAssetWithSubAssets<Sprite>("Tier1");
		if (array.Length >= 3)
		{
			front.sprite = array[0];
			head.sprite = array[1];
			back.sprite = array[2];
			sprite.sprite = tier1Sprite;
			if (sprite.sprite != null)
			{
				nma.baseOffset = sprite.sprite.bounds.extents.y;
				spriteHolder.GetComponent<Animator>().enabled = false;
			}
		}
		else
		{
			Debug.LogWarning("Tier 1 prototypes for " + proto[0].name + " do not exist!");
		}
		activeProto.Add(proto[0]);
		if (fromload)
		{
			spoken = true;
		}
	}

	public bool HasEntered()
	{
		return entered;
	}

	public bool IsPrototyped()
	{
		return proto != null;
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (!initialState)
		{
			return base.OnSerialize(writer, initialState: false);
		}
		writer.Write(target.netIdentity);
		writer.Write(Save());
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (!initialState)
		{
			base.OnDeserialize(reader, initialState: false);
			return;
		}
		SetTarget(reader.Read<NetworkIdentity>().GetComponent<Player>());
		target.SetKernelSprite(this);
		Load(reader.Read<KernelSpriteData>());
	}

	public KernelSpriteData Save()
	{
		return new KernelSpriteData
		{
			hasEntered = HasEntered(),
			prototypes = GetPrototype()
		};
	}

	public void Load(KernelSpriteData data)
	{
		if (data.prototypes != null)
		{
			SetPrototypeLocal(data.prototypes);
		}
		if (data.hasEntered)
		{
			Entered(fromload: true);
		}
	}

	public static AssetBundle GetRandomProto()
	{
		return activeProto[UnityEngine.Random.Range(0, activeProto.Count)];
	}

	public static int GetProtoCount()
	{
		return activeProto.Count;
	}

	static KernelSprite()
	{
		allProto = new List<AssetBundle>();
		activeProto = new List<AssetBundle>();
		RemoteCallHelper.RegisterRpcDelegate(typeof(KernelSprite), "RpcSetPrototype", InvokeUserCode_RpcSetPrototype);
	}

	private void MirrorProcessed()
	{
	}

	private void UserCode_RpcSetPrototype(string[] name)
	{
		if (!base.isServer)
		{
			SetPrototypeLocal(name);
			if (entered)
			{
				entered = false;
				Entered();
			}
		}
	}

	protected static void InvokeUserCode_RpcSetPrototype(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPrototype called on server.");
		}
		else
		{
			((KernelSprite)obj).UserCode_RpcSetPrototype(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}
}
