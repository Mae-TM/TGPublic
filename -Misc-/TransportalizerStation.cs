using UnityEngine;

public class TransportalizerStation : MonoBehaviour, AutoPickup
{
	private void Start()
	{
		FlatMap.AddTransportalizer(base.transform.parent.parent, base.transform);
	}

	public void Pickup(Player player)
	{
		FlatMap.OpenTransportalizerMode();
	}
}
