using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class DungeonEntrance : MonoBehaviour
{
	public static float lastUsed;

	[SerializeField]
	private Sprite mapSprite;

	[SerializeField]
	private Sprite finishedSprite;

	[SerializeField]
	private string boss;

	private Image[] marker;

	private bool home;

	private Dungeon dungeon;

	private int chunk;

	private int difficulty;

	private Vector3 LocalPosition => base.transform.root.InverseTransformPoint(base.transform.position);

	private void Start()
	{
		dungeon = GetComponentInParent<Dungeon>();
		home = dungeon != null;
		if (home)
		{
			chunk = Dungeon.GetChunk(dungeon.Id);
			return;
		}
		Transform transform = base.transform;
		Transform parent = transform.parent;
		DisplacementMapFlat.Chunk component = null;
		bool flag = false;
		while (parent != null)
		{
			if (parent.TryGetComponent<DisplacementMapFlat.Chunk>(out component))
			{
				flag = true;
				break;
			}
			transform = parent;
			parent = transform.parent;
		}
		if (flag)
		{
			chunk = component.ID;
			difficulty = 8 + Mathf.CeilToInt(12f * LocalPosition.sqrMagnitude / 460800f);
			component.transform.parent.GetComponent<Planet>().AddDungeon(this);
			marker = FlatMap.AddMarker(mapSprite, transform);
		}
		else if (MultiplayerSettings.hosting)
		{
			chunk = 0;
			difficulty = Random.Range(6, 15);
		}
	}

	[ServerCallback]
	private void OnTriggerEnter(Collider coll)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		Player componentInParent = coll.GetComponentInParent<Player>();
		if (!componentInParent || Time.fixedUnscaledTime - lastUsed < Time.fixedUnscaledDeltaTime * 10f)
		{
			return;
		}
		lastUsed = Time.fixedUnscaledTime;
		if (home)
		{
			if (chunk == 0)
			{
				componentInParent.MoveToSpawn(dungeon.World);
				return;
			}
			DungeonEntrance componentInChildren = dungeon.World.planet.GetChunk(chunk).GetComponentInChildren<DungeonEntrance>();
			componentInParent.SetPosition(componentInChildren.LocalPosition, dungeon.World);
			return;
		}
		if (dungeon == null)
		{
			House component = base.transform.root.GetComponent<House>();
			int iD = Dungeon.GetID(component.Id, chunk);
			dungeon = (Dungeon)AbstractSingletonManager<WorldManager>.Instance.GetArea(iD);
			if (dungeon == null)
			{
				MakeDungeon(component);
			}
		}
		componentInParent.MoveToSpawn(dungeon);
	}

	private void MakeDungeon(House house)
	{
		dungeon = (string.IsNullOrWhiteSpace(boss) ? DungeonManager.Build(house, chunk, difficulty) : DungeonManager.BuildBoss(house, chunk, boss));
		Debug.Log($"Generated dungeon {dungeon.Id}.");
	}
}
