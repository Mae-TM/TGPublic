using UnityEngine;
using UnityEngine.AI;

public class Transportalizer : MonoBehaviour
{
	[SerializeField]
	private Transportalizer link;

	private float lastUsed;

	private void Start()
	{
		OffMeshLink offMeshLink = base.gameObject.AddComponent<OffMeshLink>();
		offMeshLink.startTransform = base.transform;
		offMeshLink.endTransform = link.transform;
		offMeshLink.area = 0;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!(Time.fixedUnscaledTime - lastUsed < Time.fixedUnscaledDeltaTime * 10f) && base.enabled && link.enabled)
		{
			Player componentInParent = other.GetComponentInParent<Player>();
			if (!(componentInParent == null))
			{
				link.lastUsed = Time.fixedUnscaledTime;
				componentInParent.SetPosition(link.transform.position);
			}
		}
	}
}
