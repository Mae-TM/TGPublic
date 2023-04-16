using System.Collections.Generic;
using UnityEngine;

public class CustomCharacterComplex : MonoBehaviour
{
	public SpriteRenderer[] sprite;

	private Dictionary<string, Sprite>[] spritesheet;

	private static Sprite spr;

	public void Init(Sprite[][] sheets)
	{
		if (sheets == null)
		{
			return;
		}
		spritesheet = new Dictionary<string, Sprite>[sheets.Length];
		for (uint num = 0u; num < sheets.Length; num++)
		{
			if (sheets[num] == null)
			{
				continue;
			}
			if (sheets[num].Length != 0)
			{
				spritesheet[num] = new Dictionary<string, Sprite>();
				Sprite[] array = sheets[num];
				foreach (Sprite sprite in array)
				{
					spritesheet[num][sprite.name] = sprite;
				}
			}
			else
			{
				MonoBehaviour.print("Failed to load sprite part " + num + "!");
			}
		}
		sheets = null;
	}

	public void SetSpriteSheet(int index, Sprite[] to)
	{
		if (spritesheet != null)
		{
			spritesheet[index] = new Dictionary<string, Sprite>();
			foreach (Sprite sprite in to)
			{
				spritesheet[index][sprite.name] = sprite;
			}
			this.sprite[index].enabled = true;
		}
	}

	private void LateUpdate()
	{
		for (uint num = 0u; num < sprite.Length && num < spritesheet.Length; num++)
		{
			if (sprite[num].sprite != null && spritesheet[num] != null && spritesheet[num].TryGetValue(sprite[num].sprite.name, out spr))
			{
				sprite[num].sprite = spr;
			}
		}
	}

	private void OnBecameVisible()
	{
		base.enabled = true;
	}

	private void OnBecameInvisible()
	{
		base.enabled = false;
	}
}
