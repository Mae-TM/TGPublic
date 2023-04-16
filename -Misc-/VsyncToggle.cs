using UnityEngine;
using UnityEngine.UI;

public class VsyncToggle : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Toggle>().isOn = QualitySettings.vSyncCount == 1;
	}

	public void SetVsync(bool enabled)
	{
		QualitySettings.vSyncCount = (enabled ? 1 : 0);
	}
}
