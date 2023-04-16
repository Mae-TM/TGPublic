using System;
using UnityEngine;
using UnityEngine.UI;

public class StarRater : MonoBehaviour
{
	[SerializeField]
	private Toggle[] toggles;

	[SerializeField]
	private int max = 5;

	private int rating;

	public int Rating
	{
		get
		{
			return rating;
		}
		set
		{
			rating = value;
			for (int i = 0; i < toggles.Length; i++)
			{
				toggles[i].SetIsOnWithoutNotify(i <= rating);
			}
		}
	}

	private void Awake()
	{
		Array.Resize(ref toggles, max);
		for (int i = 1; i < max; i++)
		{
			if (toggles[i] == null)
			{
				toggles[i] = UnityEngine.Object.Instantiate(toggles[0], toggles[0].transform.parent);
			}
		}
		for (int j = 0; j < max; j++)
		{
			int value = j;
			toggles[j].onValueChanged.AddListener(delegate
			{
				Rating = value;
			});
		}
		Reset();
	}

	public void Reset()
	{
		Rating = Mathf.RoundToInt((float)max / 2f);
	}
}
