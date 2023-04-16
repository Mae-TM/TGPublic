using UnityEngine;

public class Shakable : MonoBehaviour
{
	private float initial;

	private float falloff;

	private float amplitude;

	public void Shake(float initial, float amplitude, float duration)
	{
		base.transform.localPosition += initial * Random.onUnitSphere;
		this.amplitude = amplitude;
		falloff = amplitude / duration;
	}

	private void Update()
	{
		if (amplitude > 0f)
		{
			base.transform.localPosition += amplitude * Time.deltaTime * Random.insideUnitSphere;
			amplitude -= falloff * Time.deltaTime;
		}
	}
}
