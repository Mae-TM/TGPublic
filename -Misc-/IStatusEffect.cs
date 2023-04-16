public interface IStatusEffect
{
	float EndTime { get; }

	void Begin(Attackable att);

	float Update(Attackable att);

	bool OnAttack(Attack attack);

	bool AfterAttack(Attack attack);

	bool OnAttacked(Attack attack);

	void Stop(Attackable att);
}
