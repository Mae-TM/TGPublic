using UnityEngine;

public class SoundEffects : MonoBehaviour
{
	[SerializeField]
	private AudioSource kachunkSound;

	[SerializeField]
	private AudioSource pickupSound;

	[SerializeField]
	private AudioSource nopeSound;

	[SerializeField]
	private AudioClip gristClip;

	[SerializeField]
	private Shakable shakable;

	public static SoundEffects Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	public void Kachunk(WorldArea area)
	{
		if (Visibility.Get(area.gameObject))
		{
			if (kachunkSound != null)
			{
				kachunkSound.Play();
			}
			if (shakable != null)
			{
				shakable.Shake(1f, 25f, 0.25f);
			}
		}
	}

	public void Shake(float initial, float amplitude, float duration)
	{
		shakable.Shake(initial, amplitude, duration);
	}

	public void Pickup()
	{
		if (pickupSound != null)
		{
			pickupSound.Play();
		}
	}

	public void Nope()
	{
		if (nopeSound != null)
		{
			nopeSound.Play();
		}
	}

	public void Grist(Vector3 position)
	{
		AudioSource.PlayClipAtPoint(gristClip, position, 75f);
	}
}
