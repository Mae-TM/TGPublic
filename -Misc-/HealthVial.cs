using System.Text;
using UnityEngine;
using UnityEngine.UI;

public abstract class HealthVial : MonoBehaviour
{
	[SerializeField]
	private Text barText;

	[SerializeField]
	private Text critText;

	private readonly StringBuilder sb = new StringBuilder();

	private int prevHealth;

	private int prevMax;

	public void ShowCrit(bool positive = true)
	{
		if (!(critText == null))
		{
			critText.text = (positive ? "Critical Hit" : "Critical Fail");
			critText.color = (positive ? Color.yellow : Color.magenta);
			Transform obj = critText.transform;
			obj.localPosition = Vector3.zero;
			Transform parent = obj.parent;
			parent.GetComponent<Animator>().Play(0);
			parent.gameObject.SetActive(value: true);
		}
	}

	public void Set(float health, float shield, float max)
	{
		if ((object)barText != null && !barText.isActiveAndEnabled)
		{
			return;
		}
		int num = Mathf.FloorToInt(health);
		int num2 = Mathf.FloorToInt(max);
		if (num == prevHealth && num2 == prevMax)
		{
			if (health >= max && shield <= 0f)
			{
				return;
			}
		}
		else
		{
			prevHealth = num;
			prevMax = num2;
			if ((object)barText != null)
			{
				sb.Append(num).Append('/').Append(num2);
				barText.text = sb.ToString();
				sb.Clear();
			}
		}
		SetVialSize(health, shield, max);
	}

	protected abstract void SetVialSize(float health, float shield, float max);

	public virtual void Enable(float duration = float.PositiveInfinity)
	{
	}

	public virtual void Disable()
	{
	}
}
