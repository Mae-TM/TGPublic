using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class PauseMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject[] tabs;

	[SerializeField]
	private Button[] buttons;

	private BlurOptimized blur;

	private void Awake()
	{
		blur = MSPAOrthoController.main.GetComponent<BlurOptimized>();
		blur.enabled = false;
	}

	private void OnEnable()
	{
		blur.enabled = true;
	}

	private void OnDisable()
	{
		if (blur != null)
		{
			blur.enabled = false;
		}
	}

	public void PauseGame()
	{
		base.gameObject.SetActive(value: true);
	}

	public void OpenTab(int index)
	{
		base.gameObject.SetActive(value: true);
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].SetActive(i == index);
		}
		for (int j = 0; j < buttons.Length; j++)
		{
			buttons[j].interactable = j != index;
		}
	}

	public void OpenTab(GameObject tab)
	{
		base.gameObject.SetActive(value: true);
		GameObject[] array = tabs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		Button[] array2 = buttons;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].interactable = true;
		}
		tab.SetActive(value: true);
	}

	public void ResumeGame()
	{
		base.gameObject.SetActive(value: false);
	}
}
