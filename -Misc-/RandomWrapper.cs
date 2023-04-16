using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class RandomWrapper
{
	private Random random = new Random();

	public float value => (float)random.NextDouble();

	public byte[] Save()
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, random);
		return memoryStream.ToArray();
	}

	public void Load(byte[] array)
	{
		using MemoryStream serializationStream = new MemoryStream(array);
		random = (Random)new BinaryFormatter().Deserialize(serializationStream);
	}

	public int Next()
	{
		return random.Next();
	}

	public int Next(int max)
	{
		return random.Next(max);
	}

	public int Next(int min, int max)
	{
		return random.Next(min, max);
	}

	public double NextDouble()
	{
		return random.NextDouble();
	}
}
