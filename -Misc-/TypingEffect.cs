using UnityEngine.UI;

internal class TypingEffect
{
	private string toType;

	private Text target;

	public void SetText(string toSet)
	{
		toType = toSet;
	}

	public TypingEffect(Text targetToSet)
	{
		target = targetToSet;
	}

	public bool TypeNext()
	{
		if (toType == "")
		{
			return false;
		}
		target.text += toType[0];
		toType = toType.Substring(1);
		return true;
	}

	public void Reset()
	{
		target.text = "";
	}
}
