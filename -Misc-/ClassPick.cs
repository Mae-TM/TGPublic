using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ClassPick : MonoBehaviour
{
	public static Class chosen = Class.Count;

	public static int[] score = new int[12];

	[SerializeField]
	private Sprite[] sprites;

	[SerializeField]
	private Text text;

	private void Start()
	{
		Image image = base.transform.GetChild(0).GetComponent<Image>();
		image.sprite = sprites[0];
		image.SetNativeSize();
		image.rectTransform.pivot = new Vector2(image.sprite.pivot.x / image.sprite.rect.width, image.sprite.pivot.y / image.sprite.rect.height);
		image.name = Class.Maid.ToString();
		for (int i = 1; i < sprites.Length; i++)
		{
			image = Object.Instantiate(image, base.transform);
			image.sprite = sprites[i];
			image.SetNativeSize();
			image.rectTransform.pivot = new Vector2(image.sprite.pivot.x / image.sprite.rect.width, image.sprite.pivot.y / image.sprite.rect.height);
			Image image2 = image;
			Class @class = (Class)i;
			image2.name = @class.ToString();
		}
		base.transform.GetChild(1).SetAsLastSibling();
		StartCoroutine(UpdatePositionDelayed());
	}

	private IEnumerator UpdatePositionDelayed()
	{
		yield return null;
		UpdatePosition();
	}

	public void Next()
	{
		if (chosen == Class.Count)
		{
			chosen = Class.Maid;
		}
		else
		{
			chosen++;
		}
		UpdatePosition();
		UpdateScore();
	}

	public void Prev()
	{
		if (chosen == Class.Maid)
		{
			chosen = Class.Count;
		}
		else
		{
			chosen--;
		}
		UpdatePosition();
		UpdateScore();
	}

	public static int[] CalculateScore()
	{
		for (int i = 0; i < 12; i++)
		{
			score[i] = ((chosen == (Class)i) ? 1 : 0);
		}
		return score;
	}

	private void UpdateScore()
	{
		for (int i = 0; i < 12; i++)
		{
			score[i] = ((chosen == (Class)i) ? 1 : 0);
		}
	}

	private void UpdatePosition()
	{
		base.transform.localPosition = -base.transform.GetChild((int)chosen).localPosition - new Vector3(0f, 20f, 0f);
		text.text = ((chosen == Class.Count) ? "" : chosen.ToString());
	}
}
