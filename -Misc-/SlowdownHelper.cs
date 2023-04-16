using System.Linq;
using UnityEngine;

internal class SlowdownHelper : MonoBehaviour
{
	private static AudioSource[] sources;

	private void Start()
	{
		sources = Object.FindObjectsOfType<AudioSource>();
	}

	public static void SetTimeScale(float scale)
	{
		Time.timeScale = scale;
		for (int i = 0; i < sources.Count(); i++)
		{
			sources[i].pitch = scale;
		}
	}
}
