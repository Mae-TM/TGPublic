using System;
using UnityEngine;

public class Fader : MonoBehaviour
{
	public static Fader instance;

	public Texture2D fadeOutTexture;

	public float fadeSpeed = 0.8f;

	public Action fadedToBlack;

	public Action fadedToNone;

	public static float origVolume = -1f;

	private static AudioClip prevClip = null;

	private static float prevTime = 0f;

	private int drawDepth = -1000;

	private float alpha = 1f;

	private int fadeDir = -1;

	private void OnGUI()
	{
		alpha += (float)fadeDir * fadeSpeed * Time.deltaTime;
		if (alpha > 1f)
		{
			alpha = 1f;
			fadeDir = 0;
			if (fadedToBlack != null)
			{
				fadedToBlack();
				SaveMusic();
			}
		}
		else if (alpha < 0f)
		{
			alpha = 0f;
			fadeDir = 0;
			if (fadedToNone != null)
			{
				fadedToNone();
			}
		}
		if (alpha != 0f)
		{
			GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
			GUI.depth = drawDepth;
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), fadeOutTexture);
			if (fadeDir != 0)
			{
				AudioListener.volume = origVolume - alpha * origVolume;
			}
		}
	}

	public float BeginFade(int direction)
	{
		fadeDir = direction;
		return fadeSpeed;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			if (origVolume == -1f)
			{
				origVolume = PlayerPrefs.GetFloat("Volume", 1f);
				AudioListener.volume = origVolume;
			}
			AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
			foreach (AudioSource audioSource in array)
			{
				if (audioSource.clip == prevClip)
				{
					audioSource.time = prevTime;
					break;
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void SaveMusic()
	{
		AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
		foreach (AudioSource audioSource in array)
		{
			if (audioSource.loop)
			{
				prevClip = audioSource.clip;
				prevTime = audioSource.time;
				break;
			}
		}
	}

	public bool IsFinished()
	{
		return fadeSpeed == 0f;
	}
}
