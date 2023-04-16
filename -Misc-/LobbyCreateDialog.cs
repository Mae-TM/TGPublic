using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateDialog : MonoBehaviour
{
	[SerializeField]
	private InputField txtPass;

	[SerializeField]
	private Dropdown visibility;

	public event Action<string> OnSubmit;

	private void Start()
	{
		List<string> options = new List<string> { "Private", "Friends-Only", "Public" };
		visibility.AddOptions(options);
		txtPass.text = "";
	}

	public void Show()
	{
		txtPass.text = "";
		base.gameObject.SetActive(value: true);
	}

	public void onSubmit()
	{
		this.OnSubmit?.Invoke(txtPass.text);
		txtPass.text = "";
		base.gameObject.SetActive(value: false);
	}

	public void onCancel()
	{
		txtPass.text = "";
		base.gameObject.SetActive(value: false);
	}
}
