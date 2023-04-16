using Mirror;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	public string prefab;

	public int spawnCount;

	public Consort.Job consortJob;

	private void Start()
	{
		if (NetworkServer.active)
		{
			DisplacementMapFlat.Chunk componentInParent = GetComponentInParent<DisplacementMapFlat.Chunk>();
			WorldArea component = base.transform.root.GetComponent<WorldArea>();
			if (!componentInParent.WasFilled)
			{
				for (int i = 0; i < spawnCount; i++)
				{
					Vector3 location = base.transform.position + new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f) * 4f;
					SpawnHelper.instance.Spawn(prefab, component, location, OnCreate);
				}
			}
		}
		Object.Destroy(this);
	}

	private void OnCreate(Attackable att)
	{
		if (att is Consort consort)
		{
			consort.SetJob(consortJob);
		}
	}
}
