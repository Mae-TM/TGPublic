using UnityEngine;
using UnityEngine.AI;

public class SpawnAction : InteractableAction
{
	public GameObject template;

	public Transform spawn;

	private void Start()
	{
		if (!NetcodeManager.Instance.offline)
		{
			GetComponent<Interactable>().RemoveOption(this);
		}
	}

	public override void Execute()
	{
		if (NetcodeManager.Instance.offline || !NetcodeManager.Instance.enabled)
		{
			GameObject obj = Object.Instantiate(template, spawn.position, Quaternion.identity);
			NavMeshAgent component = obj.GetComponent<NavMeshAgent>();
			if (component != null)
			{
				component.Warp(spawn.position);
			}
			obj.name = template.name;
			obj.SetActive(value: true);
		}
		else
		{
			SpawnHelper.instance.Spawn(template.name, base.transform.root.GetComponent<WorldArea>(), spawn.position);
		}
	}
}
