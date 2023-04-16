using Mirror;
using UnityEngine;

public static class ExtraWriters
{
	public static void WriteNullable(this NetworkWriter writer, Vector3? input)
	{
		writer.Write(input.HasValue);
		if (input.HasValue)
		{
			writer.Write(input.Value);
		}
	}

	public static Vector3? ReadNullable(this NetworkReader reader)
	{
		if (!reader.Read<bool>())
		{
			return null;
		}
		return reader.Read<Vector3>();
	}

	public static void WriteRectInt(this NetworkWriter writer, RectInt rect)
	{
		writer.Write(rect.position);
		writer.Write(rect.size);
	}

	public static RectInt ReadRectInt(this NetworkReader reader)
	{
		return new RectInt(reader.Read<Vector2Int>(), reader.Read<Vector2Int>());
	}
}
