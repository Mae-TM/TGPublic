using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class FurnitureObjective : QuestObjective
{
	private readonly string furniture;

	public FurnitureObjective(string furniture, string description, bool isBonus)
		: base("Deploy the " + furniture, description, isBonus)
	{
		this.furniture = furniture;
		BuildExploreSwitcher.Instance.houseBuilder.OnDeployFurniture += OnDeployFurniture;
	}

	private void OnDeployFurniture(string name)
	{
		if (name == furniture)
		{
			BuildExploreSwitcher.Instance.houseBuilder.OnDeployFurniture -= OnDeployFurniture;
			Status = ObjectiveStatus.Completed;
		}
	}
}
