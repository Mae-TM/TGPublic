using System;
using UnityEngine;

public class LobbyClasspectWizard : MonoBehaviour
{
	public GameObject MethodPicker;

	public GameObject Flowchart;

	public GameObject BiasedRandom;

	public GameObject AspectWheel;

	public event EventHandler OnDone;

	public void TriggerOnDone()
	{
		this.OnDone?.Invoke(this, EventArgs.Empty);
	}

	public void ResetWizard()
	{
		Flowchart.SetActive(value: false);
		BiasedRandom.SetActive(value: false);
		AspectWheel.SetActive(value: false);
		MethodPicker.SetActive(value: true);
		Flowchart.GetComponent<ClasspectQuiz>().ResetQuiz();
	}
}
