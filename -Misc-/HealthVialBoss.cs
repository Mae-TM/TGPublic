public class HealthVialBoss : HealthVialPlayer
{
	public override void Enable(float duration = float.PositiveInfinity)
	{
		if (float.IsPositiveInfinity(duration))
		{
			base.transform.parent.gameObject.SetActive(value: true);
		}
	}

	public override void Disable()
	{
		base.transform.parent.gameObject.SetActive(value: false);
	}
}
