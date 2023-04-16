using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

internal static class ProtobufHelpers
{
	public static object ProtoDeserialize(byte[] data, Type type)
	{
		using MemoryStream source = new MemoryStream(data);
		return Serializer.NonGeneric.Deserialize(type, source);
	}

	public static T ProtoDeserialize<T>(byte[] data)
	{
		if (data == null)
		{
			data = Array.Empty<byte>();
		}
		using MemoryStream source = new MemoryStream(data);
		return Serializer.Deserialize<T>(source);
	}

	public static byte[] ProtoSerialize<T>(T obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		Serializer.Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	public static byte[] ProtoSerialize(object obj, out short length)
	{
		using MemoryStream memoryStream = new MemoryStream();
		Serializer.NonGeneric.Serialize(memoryStream, obj);
		length = (short)memoryStream.Length;
		return memoryStream.ToArray();
	}

	public static (T1[], int[]) Unjag<T1>(IEnumerable<T1[]> array)
	{
		if (array == null)
		{
			return (null, null);
		}
		List<T1> list = new List<T1>();
		List<int> list2 = new List<int>();
		foreach (T1[] item in array)
		{
			list.AddRange(item);
			list2.Add(item.Length);
		}
		return (list.ToArray(), list2.ToArray());
	}

	public static IEnumerable<T1[]> Rejag<T1>(T1[] array, IEnumerable<int> lengths)
	{
		if (array == null)
		{
			yield break;
		}
		int index = 0;
		foreach (int length in lengths)
		{
			T1[] array2 = new T1[length];
			Array.Copy(array, index, array2, 0, length);
			yield return array2;
			index += length;
		}
	}
}
