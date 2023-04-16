using Mirror;

public static class SidesSerializer
{
	public static void WriteSides(this NetworkWriter writer, AAPoly poly)
	{
		writer.Write((AAPoly.ShortChains)poly);
	}

	public static AAPoly ReadSides(this NetworkReader reader)
	{
		return reader.Read<AAPoly.ShortChains>();
	}
}
