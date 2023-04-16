using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class CreatureSpawn : FloorFurniture
{
	private void SetCreature(string creature)
	{
		SpawnHelper.instance.GetCreatureAsync(creature, delegate(Attackable prefab)
		{
			GameObject obj = Object.Instantiate(prefab.gameObject, base.transform);
			Object.Destroy(obj.GetComponent<Attackable>());
			Object.Destroy(obj.GetComponent<NavMeshAgent>());
			Object.Destroy(obj.GetComponent<RegionChild>());
			Object.Destroy(obj.GetComponent<NetworkIdentity>());
		});
	}

	public static Furniture GetInstance(string name, Building building, Vector3Int coords)
	{
		string text = name.Substring("Prefabs/".Length);
		if (!BuildExploreSwitcher.cheatMode)
		{
			SpawnHelper.instance.Spawn(text, building, building.GetWorldPosition(coords));
			return null;
		}
		CreatureSpawn creatureSpawn = Object.Instantiate((CreatureSpawn)HouseManager.Instance.GetFurniturePrefab("CreatureSpawn"));
		creatureSpawn.SetCreature(text);
		return creatureSpawn;
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		bool result = base.OnSerialize(writer, initialState);
		if (initialState)
		{
			writer.Write(base.name.Substring("Prefabs/".Length));
		}
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			SetCreature(reader.Read<string>());
		}
	}

	private void MirrorProcessed()
	{
	}
}
