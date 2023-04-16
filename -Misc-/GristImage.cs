using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GristImage : MonoBehaviour
{
	private Sprite[] sprites;

	[SerializeField]
	private Image image;

	[SerializeField]
	private Tooltipped tooltip;

	private static readonly object delay = new WaitForSeconds(0.1f);

	private IEnumerator Animate()
	{
		int index = 0;
		while (true)
		{
			image.sprite = sprites[index];
			yield return delay;
			index = (index + 1) % sprites.Length;
		}
	}

	private void OnEnable()
	{
		if (sprites != null)
		{
			StartCoroutine(Animate());
		}
	}

	public void SetGrist(int gristIndex)
	{
		StopAllCoroutines();
		if ((object)tooltip != null)
		{
			tooltip.tooltip = Grist.GetName(gristIndex) + " Grist";
		}
		sprites = Grist.GetSprites(gristIndex);
		if (sprites.Length == 1)
		{
			image.sprite = sprites[0];
			sprites = null;
		}
		else
		{
			StartCoroutine(Animate());
		}
	}
}
