using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
	private enum Type
	{
		Global,
		Music
	}

	[SerializeField]
	private Type type;

	public Slider targetSlider;

	public AudioSource[] target;

	private void Start()
	{
		if (targetSlider == null)
		{
			targetSlider = GetComponent<Slider>();
		}
		if (type == Type.Global)
		{
			targetSlider.value = Fader.origVolume;
			target = null;
		}
		else if (type == Type.Music)
		{
			targetSlider.value = BackgroundMusic.Volume;
		}
	}

	public void OnValueChanged(float newValue)
	{
		if (type == Type.Global)
		{
			AudioListener.volume = newValue;
			Fader.origVolume = newValue;
		}
		else
		{
			AudioSource[] array = target;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].volume = newValue;
			}
		}
		if (type == Type.Music)
		{
			BackgroundMusic.Volume = newValue;
		}
	}

	private void OnDestroy()
	{
		if (target == null)
		{
			PlayerPrefs.SetFloat("Volume", targetSlider.value);
		}
	}
}
