using Mirror;
using ProtoBuf;
using UnityEngine;

public class BossRoom : MonoBehaviour
{
	[ProtoContract]
	public struct Data
	{
		[ProtoMember(1)]
		public string name;

		[ProtoMember(2)]
		public float health;

		[ProtoMember(3)]
		public StatusEffect.Data[] statusEffects;
	}

	private static AssetBundle bundle;

	[SerializeField]
	private Boss boss;

	[SerializeField]
	private Transform camera;

	[SerializeField]
	private GameObject exit;

	public static void RegisterPrefabs()
	{
		if (bundle == null)
		{
			bundle = AssetBundleExtensions.Load("bosses");
		}
		GameObject[] array = bundle.LoadAllAssets<GameObject>();
		foreach (GameObject gameObject in array)
		{
			BossRoom bossRoom = gameObject.GetComponent<BossRoom>();
			NetworkClient.RegisterPrefab(bossRoom.boss.gameObject, (SpawnMessage msg) => Object.Instantiate(bossRoom).boss.gameObject, UnspawnBoss);
		}
	}

	private static void UnspawnBoss(GameObject spawned)
	{
		Object.Destroy(spawned.transform.parent.GetComponent<BossRoom>().gameObject);
	}

	public static BossRoom Build(string name, WorldArea area)
	{
		GameObject obj = Object.Instantiate(bundle.LoadAsset<GameObject>(name));
		obj.name = name;
		BossRoom component = obj.GetComponent<BossRoom>();
		component.boss.RegionChild.Area = area;
		NetworkServer.Spawn(component.boss.gameObject);
		return component;
	}

	private void Start()
	{
		Transform parent = boss.RegionChild.Region.transform;
		base.transform.SetParent(parent, worldPositionStays: false);
		boss.transform.SetParent(base.transform, worldPositionStays: false);
		boss.RegionChild.Area.GetComponentInChildren<BossEntrance>().target = this;
		boss.OnDeath += delegate
		{
			exit.SetActive(value: true);
		};
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Player>(out var component) && component.self)
		{
			BuildExploreSwitcher.Instance.OnBossEnter(camera);
		}
	}

	public static Data Save(WorldArea area)
	{
		BossRoom componentInChildren = area.GetComponentInChildren<BossRoom>();
		if (!componentInChildren)
		{
			return default(Data);
		}
		Data result = default(Data);
		result.name = componentInChildren.name;
		result.health = componentInChildren.boss.Health;
		result.statusEffects = componentInChildren.boss.StatusEffects.Save();
		return result;
	}

	public static BossRoom Load(WorldArea area, Data data)
	{
		if (string.IsNullOrEmpty(data.name))
		{
			return null;
		}
		BossRoom bossRoom = Build(data.name, area);
		bossRoom.boss.Health = data.health;
		bossRoom.boss.StatusEffects.Load(data.statusEffects);
		NetworkServer.Spawn(bossRoom.boss.gameObject);
		return bossRoom;
	}
}
