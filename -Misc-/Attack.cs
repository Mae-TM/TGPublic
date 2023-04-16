public class Attack
{
	public delegate void Handler(Attack attack);

	public Attacking source;

	public Attackable target;

	public float damage;

	public bool isRanged;

	private float critMultiplier = 1f;

	public float CritMultiplier
	{
		get
		{
			return critMultiplier;
		}
		set
		{
			damage *= value / critMultiplier;
			critMultiplier = value;
		}
	}

	public bool WasLethal => target.Health <= 0f;
}
