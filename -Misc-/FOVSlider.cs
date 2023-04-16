using UnityEngine;
using UnityEngine.UI;

public class FOVSlider : MonoBehaviour
{
	public Slider targetSlider;

	private void Start()
	{
		if (targetSlider == null)
		{
			targetSlider = GetComponent<Slider>();
		}
	}

	public void OnValueChanged()
	{
		Camera.main.fieldOfView = targetSlider.value;
	}
}
