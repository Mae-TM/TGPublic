using UnityEngine;

public class BossEntrance : MonoBehaviour
{
	public BossRoom target;

	public void OnTriggerEnter(Collider other)
	{
		Player componentInParent = other.GetComponentInParent<Player>();
		if (!(componentInParent == null))
		{
			componentInParent.SetPosition(target.transform.position);
		}
	}
}
