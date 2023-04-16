using UnityEngine;

public class Dungeon : Building
{
	public House World { get; private set; }

	protected override void AfterInit()
	{
		World = (House)AbstractSingletonManager<WorldManager>.Instance.GetArea(GetOwner(base.Id));
		if (World.HasPlanet)
		{
			base.material = new Material(base.material);
			Material material = World.planet.Material;
			base.material.SetColor("_ColorStart", material.GetColor("_MountainColorStart") / 2f + Color.grey / 2f);
			base.material.SetColor("_ColorEnd", material.GetColor("_MountainColorEnd") / 2f + Color.grey / 2f);
		}
	}

	public static int GetID(int owner, int chunk)
	{
		return -36 * owner - chunk - 1;
	}

	public static int GetOwner(int id)
	{
		return -(id + 1) / 36;
	}

	public static int GetChunk(int id)
	{
		return -(id + 1) % 36;
	}

	public BossRoom.Data SaveProgress()
	{
		return BossRoom.Save(this);
	}

	public void LoadProgress(BossRoom.Data data)
	{
		BossRoom.Load(this, data);
	}

	private void MirrorProcessed()
	{
	}
}
