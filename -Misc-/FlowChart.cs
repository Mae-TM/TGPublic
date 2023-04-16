using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class FlowChart : MonoBehaviour
{
	public FlowState flowState;

	public AudioSource audioSource;

	public AudioClip[] audioClip;

	public Text[] buttonText;

	public Text content;

	private TypingEffect typeEffect;

	public void Start()
	{
		for (int i = 0; i < buttonText.Length; i++)
		{
			if (i < flowState.next.Length)
			{
				buttonText[i].text = flowState.next[i].choice;
			}
			else
			{
				buttonText[i].transform.parent.gameObject.SetActive(value: false);
			}
		}
		if (flowState.audioClip != -1)
		{
			audioSource.clip = audioClip[flowState.audioClip];
			audioSource.Play();
		}
		typeEffect = new TypingEffect(content);
		typeEffect.SetText(flowState.content.text);
		StartCoroutine(Type());
	}

	private void Redraw()
	{
		typeEffect.SetText(flowState.content.text);
		StopAllCoroutines();
		StartCoroutine(Type());
		Debug.Log(flowState.next.Count());
		for (int i = 0; i < buttonText.Length; i++)
		{
			if (i < flowState.next.Length)
			{
				buttonText[i].transform.parent.gameObject.SetActive(value: true);
				buttonText[i].text = flowState.next[i].choice;
			}
			else
			{
				buttonText[i].transform.parent.gameObject.SetActive(value: false);
			}
		}
		if (flowState.audioClip != -1)
		{
			audioSource.clip = audioClip[flowState.audioClip];
			audioSource.Play();
		}
	}

	public void ButtonClick(int i)
	{
		flowState = flowState.next[i];
		if (flowState.result != null)
		{
			TestFinished(flowState.result);
		}
		Redraw();
	}

	private IEnumerator Type()
	{
		bool flag = true;
		typeEffect.Reset();
		while (flag)
		{
			yield return new WaitForSeconds(0.05f);
			flag = typeEffect.TypeNext();
		}
	}

	public abstract void TestFinished(Enum result);
}
