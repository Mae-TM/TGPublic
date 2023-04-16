using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class DungeonObjective : QuestObjective
{
	private readonly int dungeonID;

	public DungeonObjective(string title, string description, bool isBonus, int dungeonID = 0)
		: base(title, description, isBonus)
	{
		this.dungeonID = dungeonID;
	}

	public override ObjectiveStatus CheckProgress()
	{
		WorldArea area = Player.player.RegionChild.Area;
		if (area is Dungeon && (dungeonID >= 0 || area.Id == dungeonID) && !area.GetComponentInChildren<Enemy>())
		{
			Status = ObjectiveStatus.Completed;
		}
		return Status;
	}
}
