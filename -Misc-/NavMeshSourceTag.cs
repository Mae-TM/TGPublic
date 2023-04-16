using UnityEngine;

[DefaultExecutionOrder(-200)]
public class NavMeshSourceTag : MonoBehaviour
{
	public bool removeOnDisable = true;

	private LocalNavMeshBuilder builder;

	private bool started;

	private void Start()
	{
		started = true;
		Add();
	}

	private void OnEnable()
	{
		if (started && removeOnDisable)
		{
			Add();
		}
	}

	private void OnTransformParentChanged()
	{
		LocalNavMeshBuilder component = base.transform.root.GetComponent<LocalNavMeshBuilder>();
		if (!(component == builder) && TryGetComponent<Collider>(out var component2))
		{
			if ((bool)builder)
			{
				builder.RemoveSource(component2);
			}
			if ((bool)component)
			{
				component.AddSource(component2);
			}
			builder = component;
		}
	}

	private void Add()
	{
		if (base.transform.root.TryGetComponent<LocalNavMeshBuilder>(out builder) && TryGetComponent<Collider>(out var component))
		{
			builder.AddSource(component);
		}
	}

	private void OnDisable()
	{
		if (removeOnDisable && builder != null && builder.gameObject.activeInHierarchy && TryGetComponent<Collider>(out var component))
		{
			builder.RemoveSource(component);
		}
	}

	private void OnDestroy()
	{
		if (builder != null && TryGetComponent<Collider>(out var component))
		{
			builder.RemoveSourceImmediate(component);
		}
	}

	public void Refresh()
	{
		if ((bool)builder)
		{
			builder.ForceUpdate();
		}
	}
}
