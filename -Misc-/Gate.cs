using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gate : MonoBehaviour
{
	private static Gate prefab;

	private static readonly Dictionary<(WorldArea, byte), Gate> houseGates = new Dictionary<(WorldArea, byte), Gate>();

	public byte index;

	public Gate link;

	private float lastUsed;

	private void Start()
	{
		Transform transform = base.transform;
		transform.localScale *= transform.localScale.x / transform.lossyScale.x;
		transform.localPosition += Vector3.up;
		if (index == 0)
		{
			Color ownerColor = transform.root.GetComponent<House>().ownerColor;
			ImageEffects.SetShiftColor(GetComponent<SpriteRenderer>().material, ownerColor);
			GetComponent<Light>().color = ownerColor;
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
		if (link != null)
		{
			Transport(componentInParent, link);
			return;
		}
		House component = base.transform.root.GetComponent<House>();
		if (index == 0)
		{
			componentInParent.MoveToSpawn(component);
		}
		else if (index == 13)
		{
			WorldArea relativeHouse = AbstractSingletonManager<WorldManager>.Instance.GetRelativeHouse(-2, component);
			Transport(componentInParent, houseGates[(relativeHouse, 6)]);
		}
		else if (index == 6)
		{
			WorldArea relativeHouse2 = AbstractSingletonManager<WorldManager>.Instance.GetRelativeHouse(2, component);
			Transport(componentInParent, houseGates[(relativeHouse2, 13)]);
		}
		else if (index > 10)
		{
			WorldArea relativeHouse3 = AbstractSingletonManager<WorldManager>.Instance.GetRelativeHouse(1, component);
			Transport(componentInParent, houseGates[(relativeHouse3, (byte)(2 * (index - 10)))]);
		}
		else if ((int)index % 2 == 0)
		{
			WorldArea relativeHouse4 = AbstractSingletonManager<WorldManager>.Instance.GetRelativeHouse(-1, component);
			Transport(componentInParent, houseGates[(relativeHouse4, (byte)(10 + (int)index / 2))]);
		}
	}

	private static void Transport(Player player, Gate gate)
	{
		player.SetPosition(gate.transform);
		gate.lastUsed = Time.fixedUnscaledTime;
	}

	public static IEnumerable<Vector3> MakeGates(House house, DisplacementMapFlat planet, IReadOnlyList<Vector3> keyLocations)
	{
		if (keyLocations.Count < 3)
		{
			Debug.LogError($"Not enough key locations to place gates at! (3 required, {keyLocations.Count} given)");
		}
		int num = (keyLocations.Count - 1) / 2;
		Vector3[] gateLocations = new Vector3[3]
		{
			keyLocations[0],
			keyLocations[num],
			keyLocations[keyLocations.Count - 1]
		};
		Gate nodePrefab = Resources.Load<Gate>("Prefabs/Return Node");
		for (int j = 1; j < keyLocations.Count - 1; j++)
		{
			if (j != num)
			{
				Object.Instantiate(nodePrefab, planet.transform).transform.localPosition = keyLocations[j];
			}
		}
		byte i = 1;
		while (i <= 6)
		{
			Vector3 position = (2f + 5f * (float)(int)i - 0.9f) * 3f * 1.5f * Vector3.up;
			Gate gate = MakeGate(house.ownerColor, house, house.transform, position, i);
			if ((int)i % 2 == 1)
			{
				Gate returnNode = Object.Instantiate(nodePrefab, planet.transform);
				Transform transform = returnNode.transform;
				transform.position = planet.GetRandomValidPosition(gateLocations[(int)i / 2], 256f);
				yield return transform.localPosition;
				gate.link = returnNode;
			}
			byte b = (byte)(i + 1);
			i = b;
		}
		for (byte b2 = 0; b2 < 3; b2 = (byte)(b2 + 1))
		{
			MakeGate(house.ownerColor, house, planet.transform, gateLocations[b2], (byte)(11 + b2));
		}
	}

	private static Gate MakeGate(Color color, House house, Transform parent, Vector3 position, byte index)
	{
		if (prefab == null)
		{
			prefab = Resources.Load<Gate>("Prefabs/Gate");
		}
		Gate gate = Object.Instantiate(prefab, parent.TransformPoint(position), prefab.transform.rotation, parent);
		gate.index = index;
		gate.GetComponent<MeshRenderer>().material.color = color;
		gate.GetComponent<Light>().color = color;
		houseGates.Add((house, index), gate);
		return gate;
	}
}
