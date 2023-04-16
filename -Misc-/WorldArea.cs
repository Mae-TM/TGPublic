using Mirror;
using UnityEngine;

public abstract class WorldArea : NetworkBehaviour
{
	public int Id { get; private set; }

	public abstract Vector3 SpawnPosition { get; }

	public void Init(int id)
	{
		Id = id;
		AbstractSingletonManager<WorldManager>.Instance.AddArea(this);
		Visibility.Set(base.gameObject, value: false);
		AfterInit();
	}

	protected virtual void AfterInit()
	{
	}

	private void OnDestroy()
	{
		AbstractSingletonManager<WorldManager>.Instance.RemoveArea(this);
	}

	public abstract WorldRegion GetRegion(Vector3 position);

	private void MirrorProcessed()
	{
	}
}
