using UnityEngine;

public class MusicHolder : MonoBehaviour
{
	private static MusicHolder instance;

	[SerializeField]
	private AudioClip timerMusic;

	[SerializeField]
	private AudioClip planetStrifeMusic;

	[SerializeField]
	private AudioClip[] planetMusic;

	public static AudioClip PlanetStrifeMusic => instance.planetStrifeMusic;

	public static AudioClip GetPlanetMusic()
	{
		return instance.planetMusic[Random.Range(0, instance.planetMusic.Length)];
	}

	private void Awake()
	{
		instance = this;
	}

	public static bool PlayTimerMusic(float time)
	{
		AudioClip audioClip = instance.timerMusic;
		if (time >= audioClip.length)
		{
			return false;
		}
		BackgroundMusic.instance.PlayEvent(audioClip, loop: false, 0.1f, audioClip.length - time, interruptable: false);
		return true;
	}

	public static void EndTimerMusic()
	{
		BackgroundMusic.instance.ResumeNormal();
	}
}
