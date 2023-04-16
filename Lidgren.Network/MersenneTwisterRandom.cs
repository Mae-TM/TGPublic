namespace Lidgren.Network;

public sealed class MersenneTwisterRandom : NetRandom
{
	public new static readonly MersenneTwisterRandom Instance = new MersenneTwisterRandom();

	private const int N = 624;

	private const int M = 397;

	private const uint MATRIX_A = 2567483615u;

	private const uint UPPER_MASK = 2147483648u;

	private const uint LOWER_MASK = 2147483647u;

	private const uint TEMPER1 = 2636928640u;

	private const uint TEMPER2 = 4022730752u;

	private const int TEMPER3 = 11;

	private const int TEMPER4 = 7;

	private const int TEMPER5 = 15;

	private const int TEMPER6 = 18;

	private uint[] mt;

	private int mti;

	private uint[] mag01;

	private const double c_realUnitInt = 4.656612873077393E-10;

	public MersenneTwisterRandom()
	{
		Initialize(NetRandomSeed.GetUInt32());
	}

	public MersenneTwisterRandom(uint seed)
	{
		Initialize(seed);
	}

	public override void Initialize(uint seed)
	{
		mt = new uint[624];
		mti = 625;
		mag01 = new uint[2] { 0u, 2567483615u };
		mt[0] = seed;
		for (int i = 1; i < 624; i++)
		{
			mt[i] = (uint)(1812433253 * (mt[i - 1] ^ (mt[i - 1] >> 30)) + i);
		}
	}

	public override uint NextUInt32()
	{
		if (mti >= 624)
		{
			GenRandAll();
			mti = 0;
		}
		uint num = mt[mti++];
		uint num2 = num ^ (num >> 11);
		int num3 = (int)num2 ^ ((int)(num2 << 7) & -1658038656);
		int num4 = num3 ^ ((num3 << 15) & -272236544);
		return (uint)num4 ^ ((uint)num4 >> 18);
	}

	private void GenRandAll()
	{
		int num = 1;
		uint num2 = mt[0] & 0x80000000u;
		uint num3;
		do
		{
			num3 = mt[num];
			mt[num - 1] = mt[num + 396] ^ ((num2 | (num3 & 0x7FFFFFFF)) >> 1) ^ mag01[num3 & 1];
			num2 = num3 & 0x80000000u;
		}
		while (++num < 228);
		do
		{
			num3 = mt[num];
			mt[num - 1] = mt[num + -228] ^ ((num2 | (num3 & 0x7FFFFFFF)) >> 1) ^ mag01[num3 & 1];
			num2 = num3 & 0x80000000u;
		}
		while (++num < 624);
		num3 = mt[0];
		mt[623] = mt[396] ^ ((num2 | (num3 & 0x7FFFFFFF)) >> 1) ^ mag01[num3 & 1];
	}
}
