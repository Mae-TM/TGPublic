using System.Globalization;
using UnityEngine;

public class CustomCharacter : MonoBehaviour
{
	public SpriteRenderer[] sprite;

	public uint[] indexOffset;

	public bool[] spritesheetMode;

	public int[] copyAnimationFrom;

	private Sprite[][] spritesheet;

	private void Awake()
	{
		spritesheet = new Sprite[sprite.Length][];
		for (int i = 0; i < copyAnimationFrom.Length; i++)
		{
			if (copyAnimationFrom[i] == -1)
			{
				copyAnimationFrom[i] = i;
			}
		}
	}

	public void SetSpriteSheet(int index, Sprite[] to)
	{
		if (spritesheet != null)
		{
			spritesheet[index] = ((to == null || to.Length == 0) ? null : to);
			sprite[index].enabled = true;
		}
	}

	public void SetSpriteSheet(int index, Sprite[] to, Material material)
	{
		SetSpriteSheet(index, to);
		sprite[index].sharedMaterial = material;
	}

	public static int GetSpriteIndex(string spriteName)
	{
		int num = spriteName.LastIndexOf('_');
		if (num <= 0)
		{
			Debug.LogWarning("Sprite name " + spriteName + " does not contain a '_'!");
			return -1;
		}
		string text = spriteName.Substring(num + 1);
		if (int.TryParse(text, NumberStyles.None, NumberFormatInfo.InvariantInfo, out var result))
		{
			return result;
		}
		Debug.LogWarning("Sprite name " + spriteName + " ending " + text + " is not a number!");
		return -1;
	}

	private void LateUpdate()
	{
		int[] array = new int[sprite.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1;
		}
		for (uint num = 0u; num < sprite.Length && num < spritesheet.Length; num++)
		{
			if (copyAnimationFrom[num] == -1 || !sprite[num].enabled || spritesheet[num] == null)
			{
				continue;
			}
			if (sprite[copyAnimationFrom[num]].sprite == null)
			{
				sprite[num].sprite = null;
				continue;
			}
			int num2;
			if (array[copyAnimationFrom[num]] != -1)
			{
				num2 = array[copyAnimationFrom[num]];
			}
			else
			{
				string text = sprite[copyAnimationFrom[num]].sprite.name;
				if (spritesheetMode[num])
				{
					num2 = int.Parse(text.Substring(text.Length - 1), NumberStyles.None);
				}
				else
				{
					num2 = GetSpriteIndex(text);
					if (num2 == -1)
					{
						break;
					}
				}
			}
			array[num] = num2;
			if (num2 + indexOffset[num] < spritesheet[num].Length)
			{
				sprite[num].sprite = spritesheet[num][num2 + indexOffset[num]];
			}
			else
			{
				Debug.LogWarning(spritesheet[num][0].name + " does not have enough sprites!");
			}
			if (num != copyAnimationFrom[num])
			{
				sprite[num].flipX = sprite[copyAnimationFrom[num]].flipX;
				sprite[num].flipY = sprite[copyAnimationFrom[num]].flipY;
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
