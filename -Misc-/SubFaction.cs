using System.Collections.Generic;
using System.Linq;

public abstract class SubFaction
{
	protected abstract IEnumerable<Faction> GetAscendants();

	public bool IsChildOf(Faction faction)
	{
		return GetAscendants().Contains(faction);
	}

	protected SubFaction GetAlliance()
	{
		return GetAscendants().LastOrDefault((Faction faction) => faction.isAlliance) ?? this;
	}

	public IEnumerable<Attackable> GetAllies()
	{
		return GetAlliance().GetMembers();
	}

	public bool IsAllyOf(SubFaction faction)
	{
		return GetAlliance() == faction.GetAlliance();
	}

	public abstract IEnumerable<Attackable> GetMembers(SubFaction exclude = null);

	public abstract IEnumerable<Attackable> GetEnemies();
}
