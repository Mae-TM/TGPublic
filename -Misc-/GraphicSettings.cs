using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicSettings : MonoBehaviour
{
	[SerializeField]
	private TMP_Dropdown resolutions;

	[SerializeField]
	private Toggle fullscreen;

	[SerializeField]
	private Toggle vsync;

	[SerializeField]
	private Toggle anisotropic;

	[SerializeField]
	private Toggle hardwareCursor;

	private void Start()
	{
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			Resolution resolution = Screen.resolutions[i];
			resolutions.options.Add(new TMP_Dropdown.OptionData(resolution.ToString()));
			if (resolution.width == Screen.width && resolution.height == Screen.height && resolution.refreshRate == Screen.currentResolution.refreshRate)
			{
				resolutions.value = i;
			}
		}
		resolutions.RefreshShownValue();
		fullscreen.isOn = Screen.fullScreen;
		anisotropic.isOn = PlayerPrefs.GetInt("AnisotropicFiltering", 1) == 1;
		vsync.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
		hardwareCursor.isOn = PlayerPrefs.GetInt("HardwareCursor", 0) == 1;
	}

	public void SetFullscreen(bool to)
	{
		Screen.SetResolution(Screen.width, Screen.height, to, Screen.currentResolution.refreshRate);
	}

	public void SetResolution(int index)
	{
		Screen.SetResolution(Screen.resolutions[index].width, Screen.resolutions[index].height, Screen.fullScreen, Screen.resolutions[index].refreshRate);
	}

	public void SetAnisotropic(bool to)
	{
		QualitySettings.anisotropicFiltering = (to ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Disable);
		PlayerPrefs.SetInt("AnisotropicFiltering", to ? 1 : 0);
	}

	public void SetVsync(bool to)
	{
		QualitySettings.vSyncCount = (to ? 1 : 0);
		PlayerPrefs.SetInt("VSync", to ? 1 : 0);
	}

	public void SetHardwareCursor(bool to)
	{
		PlayerPrefs.SetInt("HardwareCursor", to ? 1 : 0);
	}
}
