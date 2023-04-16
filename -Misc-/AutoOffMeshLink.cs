using UnityEngine;
using UnityEngine.AI;

internal class AutoOffMeshLink : MonoBehaviour
{
	public OffMeshLink link;

	private LocalNavMeshBuilder builder;

	private bool started;

	public static OffMeshLink AddOffMeshLink(GameObject gameObject)
	{
		return gameObject.AddComponent<AutoOffMeshLink>().link;
	}

	private void Awake()
	{
		link = base.gameObject.AddComponent<OffMeshLink>();
	}

	private void Start()
	{
		started = true;
		OnEnable();
	}

	private void OnEnable()
	{
		if (started)
		{
			builder = base.transform.root.GetComponent<LocalNavMeshBuilder>();
			if (builder != null)
			{
				builder.ForceUpdate();
			}
		}
	}

	private void OnDisable()
	{
		if (builder != null)
		{
			builder.ForceUpdate();
		}
	}
}
