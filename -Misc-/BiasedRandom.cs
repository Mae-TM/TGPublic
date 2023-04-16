using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BiasedRandom : MonoBehaviour
{
	[SerializeField]
	private Text[] text;

	[SerializeField]
	private StarRater[] rater;

	[SerializeField]
	private Text content;

	private int[] aspect;

	private readonly KeyValuePair<string, Aspect[]>[] aspectRaters = new KeyValuePair<string, Aspect[]>[6]
	{
		new KeyValuePair<string, Aspect[]>("Accuracy", new Aspect[2]
		{
			Aspect.Mind,
			Aspect.Time
		}),
		new KeyValuePair<string, Aspect[]>("Strength", new Aspect[2]
		{
			Aspect.Rage,
			Aspect.Doom
		}),
		new KeyValuePair<string, Aspect[]>("Defense", new Aspect[2]
		{
			Aspect.Heart,
			Aspect.Void
		}),
		new KeyValuePair<string, Aspect[]>("Health", new Aspect[2]
		{
			Aspect.Life,
			Aspect.Blood
		}),
		new KeyValuePair<string, Aspect[]>("Magic", new Aspect[2]
		{
			Aspect.Light,
			Aspect.Hope
		}),
		new KeyValuePair<string, Aspect[]>("Ranged Attack", new Aspect[2]
		{
			Aspect.Space,
			Aspect.Breath
		})
	};

	private readonly KeyValuePair<string, Class[]>[] classRaters = new KeyValuePair<string, Class[]>[4]
	{
		new KeyValuePair<string, Class[]>("Tank", new Class[3]
		{
			Class.Witch,
			Class.Bard,
			Class.Heir
		}),
		new KeyValuePair<string, Class[]>("Damage", new Class[3]
		{
			Class.Knight,
			Class.Thief,
			Class.Prince
		}),
		new KeyValuePair<string, Class[]>("Support", new Class[3]
		{
			Class.Seer,
			Class.Mage,
			Class.Page
		}),
		new KeyValuePair<string, Class[]>("Healer", new Class[3]
		{
			Class.Sylph,
			Class.Maid,
			Class.Rogue
		})
	};

	public LobbyClasspectWizard LobbyClasspectWizard;

	public void OnEnable()
	{
		AspectQuestion();
	}

	private void SetQuestion<T>(string question, IReadOnlyList<KeyValuePair<string, T>> options)
	{
		content.text = question;
		for (int i = 0; i < options.Count; i++)
		{
			text[i].text = options[i].Key;
			rater[i].Reset();
			text[i].transform.parent.gameObject.SetActive(value: true);
		}
		for (int j = options.Count; j < text.Length; j++)
		{
			text[j].transform.parent.gameObject.SetActive(value: false);
		}
	}

	private void AspectQuestion()
	{
		SetQuestion("In a standard RPG, how well does each stat fit your playstyle?", aspectRaters);
	}

	private void ClassQuestion()
	{
		SetQuestion("How much do you like each role?", classRaters);
	}

	public void NextQuestion()
	{
		if (this.aspect == null)
		{
			this.aspect = new int[12];
			for (int i = 0; i < aspectRaters.Length; i++)
			{
				Aspect[] value = aspectRaters[i].Value;
				foreach (Aspect aspect in value)
				{
					this.aspect[(int)aspect] += rater[i].Rating;
				}
			}
			ClassQuestion();
			return;
		}
		int[] array = new int[12];
		for (int k = 0; k < classRaters.Length; k++)
		{
			Class[] value2 = classRaters[k].Value;
			foreach (Class @class in value2)
			{
				array[(int)@class] += rater[k].Rating;
			}
		}
		ClassPick.score = array;
		AspectPick.score = this.aspect;
		LobbyClasspectWizard.TriggerOnDone();
		this.aspect = null;
	}
}
