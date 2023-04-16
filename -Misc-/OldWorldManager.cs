using System.Collections.Generic;
using UnityEngine;

public class OldWorldManager : MonoBehaviour
{
	private List<Transform> rooms = new List<Transform>();

	public static OldWorldManager manager;

	[SerializeField]
	private Transform room;

	private int roomID = -1;

	public Transform Room
	{
		get
		{
			return room;
		}
		set
		{
			if (MultiplayerSettings.hosting)
			{
				EnableRendering(value);
				DisableRendering(room);
			}
			else
			{
				value.gameObject.SetActive(value: true);
				room.gameObject.SetActive(value: false);
			}
			room = value;
			roomID = rooms.IndexOf(value);
		}
	}

	private void Awake()
	{
		manager = this;
	}

	public void RegisterRoom(Transform room)
	{
		if (!rooms.Contains(room))
		{
			rooms.Add(room);
		}
		if (MultiplayerSettings.hosting && !room.gameObject.activeSelf)
		{
			room.gameObject.SetActive(value: true);
			DisableRendering(room);
		}
	}

	private void EnableRendering(Transform transform)
	{
		Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.gameObject.layer != 12)
			{
				renderer.enabled = true;
				if (renderer.gameObject.layer == 2 || renderer.gameObject.layer == 9)
				{
					renderer.gameObject.layer = 0;
				}
			}
		}
		Animator[] componentsInChildren2 = transform.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = true;
		}
	}

	private void DisableRendering(Transform transform)
	{
		Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!renderer.enabled)
			{
				renderer.gameObject.layer = 12;
				continue;
			}
			renderer.enabled = false;
			if (renderer.gameObject.layer == 0 || renderer.gameObject.layer == 9)
			{
				renderer.gameObject.layer = 2;
			}
		}
		Animator[] componentsInChildren2 = transform.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
	}

	public void SwitchRoom(int id)
	{
		Room = rooms[id];
	}

	public int GetRoomID()
	{
		if (roomID == -1)
		{
			roomID = rooms.IndexOf(room);
		}
		return roomID;
	}

	public int GetRoomID(Transform toGet)
	{
		return rooms.IndexOf(toGet);
	}

	public Transform GetRoom(int id)
	{
		if (id == -1)
		{
			return rooms[roomID];
		}
		return rooms[id];
	}

	public void AddToRoom(int id, Transform obj)
	{
		obj.SetParent(GetRoom(id));
		if (MultiplayerSettings.hosting)
		{
			if (roomID == id)
			{
				EnableRendering(obj);
			}
			else
			{
				DisableRendering(obj);
			}
		}
	}

	public void AddToRoom(Transform newRoom, Transform obj)
	{
		obj.SetParent(newRoom);
		if (MultiplayerSettings.hosting)
		{
			if (room == newRoom)
			{
				EnableRendering(obj);
			}
			else
			{
				DisableRendering(obj);
			}
		}
	}
}
