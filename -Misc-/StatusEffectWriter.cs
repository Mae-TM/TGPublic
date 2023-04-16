using Mirror;

public static class StatusEffectWriter
{
	public static void WriteStatusEffect(this NetworkWriter writer, IStatusEffect effect)
	{
		writer.Write(StatusEffect.Save(effect));
	}

	public static IStatusEffect ReadStatusEffect(this NetworkReader reader)
	{
		return StatusEffect.Load(reader.Read<StatusEffect.Data>());
	}
}
