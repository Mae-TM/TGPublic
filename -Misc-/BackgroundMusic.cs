using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
	public static BackgroundMusic instance;

	[SerializeField]
	private AudioSource eventMusic;

	private AudioClip[] basicClips;

	private int index;

	private AudioSource bgMusic;

	private float soundFade;

	private bool canInterrupt = true;

	private static float volume = -1f;

	public static float Volume
	{
		get
		{
			if (volume < 0f)
			{
				volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
			}
			return volume;
		}
		set
		{
			if (instance != null)
			{
				if (instance.bgMusic != null && volume == instance.bgMusic.volume)
				{
					instance.bgMusic.volume = value;
				}
				if (instance.eventMusic != null && volume == instance.eventMusic.volume)
				{
					instance.eventMusic.volume = value;
				}
			}
			volume = value;
			PlayerPrefs.SetFloat("MusicVolume", volume);
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogError("Two BackgroundMusic components in scene?");
			Object.Destroy(this);
		}
		if (TryGetComponent<AudioSource>(out bgMusic))
		{
			bgMusic.volume = Volume;
		}
	}

	private AudioClip GetBasicClip()
	{
		if (index >= basicClips.Length)
		{
			index = 0;
		}
		if (index < 0)
		{
			index = basicClips.Length - 1;
		}
		return basicClips[index];
	}

	private void Update()
	{
		if (soundFade > 0f)
		{
			bgMusic.volume += soundFade * Time.deltaTime / ((Volume > 0f) ? Volume : 1f);
			eventMusic.volume = Volume - bgMusic.volume;
			if (eventMusic.volume <= 0f)
			{
				bgMusic.volume = Volume;
				eventMusic.volume = 0f;
				eventMusic.Stop();
				soundFade = 0f;
			}
		}
		else if (soundFade < 0f)
		{
			eventMusic.volume -= soundFade * Time.deltaTime / ((Volume > 0f) ? Volume : 1f);
			bgMusic.volume = Volume - eventMusic.volume;
			if (bgMusic.volume <= 0f)
			{
				eventMusic.volume = Volume;
				bgMusic.volume = 0f;
				bgMusic.Pause();
				soundFade = 0f;
			}
		}
		if (!bgMusic.isPlaying && ((object)eventMusic == null || !eventMusic.isPlaying))
		{
			NextBasic();
		}
	}

	public void PlayEvent(AudioClip clip, bool loop = true, float fadeSpeed = 0f, float time = 0f, bool interruptable = true)
	{
		if (((eventMusic.clip != clip || !eventMusic.isPlaying) && canInterrupt) || (!interruptable && !eventMusic.isPlaying))
		{
			canInterrupt = interruptable;
			eventMusic.clip = clip;
			eventMusic.time = time;
			eventMusic.loop = loop;
			eventMusic.Play();
			if (fadeSpeed == 0f)
			{
				bgMusic.Stop();
				eventMusic.volume = Volume;
			}
			else
			{
				soundFade = 0f - fadeSpeed;
				eventMusic.volume = 0f;
			}
		}
	}

	public void ResumeNormal(float fadeSpeed = 0f)
	{
		if (!bgMusic.Equals(null) && !bgMusic.isPlaying)
		{
			bgMusic.UnPause();
			if (fadeSpeed == 0f)
			{
				eventMusic.Stop();
				bgMusic.volume = Volume;
			}
			else
			{
				soundFade = fadeSpeed;
				bgMusic.volume = 0f;
			}
		}
	}

	public void PlayBasic(AudioClip[] clips, float fadeSpeed = 0f, bool forceful = false)
	{
		if ((basicClips != clips || !bgMusic.isPlaying) && (canInterrupt || forceful))
		{
			canInterrupt = true;
			basicClips = clips;
			AudioClip basicClip = GetBasicClip();
			if (bgMusic.clip != basicClip)
			{
				bgMusic.Play();
				bgMusic.clip = basicClip;
			}
			else
			{
				bgMusic.UnPause();
			}
			if (fadeSpeed == 0f)
			{
				eventMusic.Stop();
				bgMusic.volume = Volume;
			}
			else
			{
				soundFade = fadeSpeed;
				bgMusic.volume = 0f;
			}
		}
	}

	public void NextBasic()
	{
		if (!eventMusic.isPlaying)
		{
			index++;
			bgMusic.clip = GetBasicClip();
			bgMusic.time = 0f;
			bgMusic.Play();
		}
	}

	public void PrevBasic()
	{
		if (!eventMusic.isPlaying)
		{
			if (bgMusic.time > 2f)
			{
				bgMusic.time = 0f;
			}
			else
			{
				index--;
				bgMusic.clip = GetBasicClip();
				bgMusic.time = 0f;
			}
			bgMusic.Play();
		}
	}
}
