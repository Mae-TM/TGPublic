using System.Collections.Generic;
using UnityEngine;

public class BuildingChanges
{
	public class Change
	{
		public int story;

		public int room;

		public AAPoly changes;
	}

	public class RoomTransfer
	{
		public int story;

		public int from;

		public int to;

		public AAPoly changes;
	}

	public readonly List<Change> changes = new List<Change>();

	public readonly List<RoomTransfer> transfers = new List<RoomTransfer>();

	private int newCells;

	public static implicit operator BuildingChanges((int, Change) pair)
	{
		BuildingChanges buildingChanges = new BuildingChanges();
		buildingChanges.Add(pair);
		return buildingChanges;
	}

	public void Add((int, Change) pair)
	{
		var (num, item) = pair;
		changes.Add(item);
		newCells += num;
	}

	private AAPoly GetRoom(int story, int room)
	{
		Change change2 = changes.Find((Change change) => change.room == room && change.story == story);
		if (change2 != null)
		{
			return change2.changes;
		}
		change2 = new Change
		{
			room = room,
			story = story,
			changes = new AAPoly()
		};
		changes.Add(change2);
		return change2.changes;
	}

	public void Add(Vector2Int cell, int story, int room)
	{
		newCells++;
		GetRoom(story, room).Add(cell);
	}

	public void Remove(Vector2Int cell, int story, int room)
	{
		newCells--;
		GetRoom(story, room).Remove(cell);
	}

	public void Transfer(Vector2Int cell, int story, int from, int to)
	{
		if (from != to)
		{
			RoomTransfer roomTransfer = transfers.Find((RoomTransfer transfer) => transfer.from == from && transfer.to == to && transfer.story == story);
			if (roomTransfer == null)
			{
				roomTransfer = new RoomTransfer
				{
					from = from,
					to = to,
					story = story,
					changes = new AAPoly()
				};
				transfers.Add(roomTransfer);
			}
			roomTransfer.changes.Add(cell);
		}
	}

	public void Finish()
	{
		changes.RemoveAll(delegate(Change change)
		{
			change.changes.CancelOpposites();
			return change.changes.Count == 0;
		});
		transfers.RemoveAll(delegate(RoomTransfer transfer)
		{
			transfer.changes.CancelOpposites();
			return transfer.changes.Count == 0;
		});
	}

	public void Invert()
	{
		foreach (Change change in changes)
		{
			change.changes.Invert();
		}
		foreach (RoomTransfer transfer in transfers)
		{
			RoomTransfer roomTransfer;
			RoomTransfer current;
			RoomTransfer roomTransfer2 = (roomTransfer = (current = transfer));
			int to = roomTransfer2.to;
			int from = roomTransfer2.from;
			current.from = to;
			roomTransfer.to = from;
		}
		newCells = -4 * Mathf.CeilToInt((float)newCells / 4f);
	}

	public int GetCost()
	{
		return Mathf.CeilToInt((float)newCells / 4f);
	}
}
