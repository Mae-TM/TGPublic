using System;
using System.IO;
using System.Text;
using ProtoBuf;
using UnityEngine;

public static class HouseLoader
{
	public static void writeInt(int i, Stream stream)
	{
		stream.Write(BitConverter.GetBytes(i), 0, 4);
	}

	public static void writeUint(uint i, Stream stream)
	{
		stream.Write(BitConverter.GetBytes(i), 0, 4);
	}

	public static void writeShort(short i, Stream stream)
	{
		stream.Write(BitConverter.GetBytes(i), 0, 2);
	}

	public static void writeShort(int i, Stream stream)
	{
		writeShort((short)i, stream);
	}

	public static void writeBool(bool b, Stream stream)
	{
		stream.Write(BitConverter.GetBytes(b), 0, 1);
	}

	public static void writeString(string s, Stream stream)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		stream.Write(bytes, 0, bytes.Length);
		stream.WriteByte(0);
	}

	public static void writeFloat(float f, Stream stream)
	{
		stream.Write(BitConverter.GetBytes(f), 0, 4);
	}

	public static void writeVector3(Vector3 v, Stream stream)
	{
		writeFloat(v.x, stream);
		writeFloat(v.y, stream);
		writeFloat(v.z, stream);
	}

	public static void Save(this Transform t, Stream stream, House house, bool includeRot = true)
	{
		Vector3 position = t.position;
		if ((object)house != null)
		{
			position -= house.transform.position;
		}
		writeVector3(position, stream);
		if (includeRot)
		{
			Quaternion rotation = t.rotation;
			writeFloat(rotation.x, stream);
			writeFloat(rotation.y, stream);
			writeFloat(rotation.z, stream);
			writeFloat(rotation.w, stream);
		}
	}

	public static int readInt(Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return BitConverter.ToInt32(array, 0);
	}

	public static uint readUint(Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return BitConverter.ToUInt32(array, 0);
	}

	public static short readShort(Stream stream)
	{
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		return BitConverter.ToInt16(array, 0);
	}

	public static bool readBool(Stream stream)
	{
		byte[] array = new byte[1];
		stream.Read(array, 0, 1);
		return BitConverter.ToBoolean(array, 0);
	}

	public static string readString(Stream stream, short maxLength = 256)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (short num = 0; num < maxLength; num = (short)(num + 1))
		{
			short num2 = (byte)stream.ReadByte();
			if (num2 == 0 || num2 == -1)
			{
				break;
			}
			stringBuilder.Append((char)num2);
		}
		return stringBuilder.ToString();
	}

	public static float readFloat(Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return BitConverter.ToSingle(array, 0);
	}

	public static Vector3 readVector3(Stream stream, House house = null)
	{
		return new Vector3(readFloat(stream), readFloat(stream), readFloat(stream));
	}

	public static void Load(this Transform t, Stream stream, House house = null, bool includeRot = true)
	{
		t.position = readVector3(stream, house);
		if (includeRot)
		{
			t.rotation = new Quaternion(readFloat(stream), readFloat(stream), readFloat(stream), readFloat(stream));
		}
	}

	public static void FakeLoadTransform(Stream stream, bool includeRot = true)
	{
		readVector3(stream);
		if (includeRot)
		{
			readFloat(stream);
			readFloat(stream);
			readFloat(stream);
			readFloat(stream);
		}
	}

	public static void writeProtoBuf<T>(Stream stream, T data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		Serializer.Serialize(memoryStream, data);
		byte[] array = memoryStream.ToArray();
		writeInt(array.Length, stream);
		stream.Write(array, 0, array.Length);
	}

	public static T readProtoBuf<T>(Stream stream)
	{
		int num = readInt(stream);
		byte[] buffer = new byte[num];
		stream.Read(buffer, 0, num);
		using MemoryStream source = new MemoryStream(buffer);
		return Serializer.Deserialize<T>(source);
	}

	public static object readProtoBuf(Type type, Stream stream)
	{
		int num = readInt(stream);
		byte[] buffer = new byte[num];
		stream.Read(buffer, 0, num);
		using MemoryStream source = new MemoryStream(buffer);
		return Serializer.Deserialize(type, source);
	}
}
