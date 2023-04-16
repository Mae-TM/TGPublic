using System;

namespace Lidgren.Network;

public static class NetRandomSeed
{
	private static int m_seedIncrement = -1640531527;

	public static uint GetUInt32()
	{
		ulong uInt = GetUInt64();
		uint num = (uint)uInt;
		uint num2 = (uint)(uInt >> 32);
		return num ^ num2;
	}

	public static ulong GetUInt64()
	{
		byte[] array = Guid.NewGuid().ToByteArray();
		return (array[0] | ((ulong)array[1] << 8) | ((ulong)array[2] << 16) | ((ulong)array[3] << 24) | ((ulong)array[4] << 32) | ((ulong)array[5] << 40) | ((ulong)array[6] << 48) | ((ulong)array[7] << 56)) ^ NetUtility.GetPlatformSeed(m_seedIncrement);
	}
}
