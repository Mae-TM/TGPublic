using System.IO;
using UnityEngine;

public struct Classpect
{
	private static Color[] color;

	public Class role;

	public Aspect aspect;

	public static Classpect Convert(int number)
	{
		return Create(number % 12, number / 12);
	}

	public static Classpect Create(int role, int aspect)
	{
		return new Classpect((Class)(role % 12), (Aspect)(aspect % 12));
	}

	public Classpect(Class role, Aspect aspect)
	{
		this.role = role;
		this.aspect = aspect;
	}

	public override string ToString()
	{
		return role.ToString() + " of " + aspect;
	}

	public static Color GetColor(Aspect aspect)
	{
		if (color == null)
		{
			color = new Color[12];
			for (Aspect aspect2 = Aspect.Time; aspect2 < Aspect.Count; aspect2++)
			{
				StreamReader streamReader;
				using (streamReader = File.OpenText(Application.streamingAssetsPath + "/Aspects/" + aspect2.ToString() + "/info.txt"))
				{
					streamReader.ReadLine();
					if (!ColorUtility.TryParseHtmlString(streamReader.ReadLine(), out color[(int)aspect2]))
					{
						Debug.LogError("Failed to load colour for aspect " + aspect2.ToString() + "!");
					}
				}
			}
		}
		return color[(int)aspect];
	}

	public static Sprite GetIcon(Aspect aspect)
	{
		return Resources.LoadAll<Sprite>("Aspects")[(int)aspect];
	}
}
