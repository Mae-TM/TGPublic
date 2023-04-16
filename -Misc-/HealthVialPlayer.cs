using UnityEngine;
using UnityEngine.UI;

public class HealthVialPlayer : HealthVial
{
	[SerializeField]
	private Image healthBar;

	[SerializeField]
	private Image shieldBar;

	protected override void SetVialSize(float health, float shield, float healthMax)
	{
		float num = Mathf.Max(healthMax, health + shield);
		shieldBar.fillAmount = (health + shield) / num;
		healthBar.fillAmount = health / num;
	}
}
