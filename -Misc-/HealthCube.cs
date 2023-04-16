using UnityEngine;

public class HealthCube : MonoBehaviour, AutoPickup
{
	private static AudioClip pickupClip;

	public int value = 1;

	private void Start()
	{
		if (pickupClip == null)
		{
			pickupClip = Resources.Load<AudioClip>("Music/GelPickup");
		}
		Object.Destroy(base.gameObject, 300f);
	}

	public void Pickup(Player player)
	{
		AudioSource.PlayClipAtPoint(pickupClip, base.transform.position);
		player.SyncedHeal(value);
		Object.Destroy(base.gameObject);
	}
}
