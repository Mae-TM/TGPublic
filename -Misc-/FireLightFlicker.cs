using System.Collections;
using UnityEngine;

public class FireLightFlicker : MonoBehaviour
{
	public float minFlickerSpeed = 0.1f;

	public float maxFlickerSpeed = 0.1f;

	public float minIntensity = 0.5f;

	public float maxIntensity = 1f;

	public Light lightcomp;

	public float intensityAvg;

	public void Start()
	{
		lightcomp = GetComponent<Light>();
		intensityAvg = (maxIntensity + minIntensity) / 2f;
		StartCoroutine(randomLight());
	}

	public void Update()
	{
	}

	private IEnumerator randomLight()
	{
		while (true)
		{
			lightcomp.intensity = Random.Range(minIntensity, intensityAvg);
			yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
			lightcomp.intensity = Random.Range(intensityAvg, maxIntensity);
			yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
		}
	}
}
