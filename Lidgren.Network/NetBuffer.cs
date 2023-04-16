using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Lidgren.Network;

public class NetBuffer
{
	private const string c_readOverflowError = "Trying to read past the buffer size - likely caused by mismatching Write/Reads, different size or order.";

	protected const int c_overAllocateAmount = 4;

	private static readonly Dictionary<Type, MethodInfo> s_readMethods;

	private static readonly Dictionary<Type, MethodInfo> s_writeMethods;

	internal byte[] m_data;

	internal int m_bitLength;

	internal int m_readPosition;

	public byte[] Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public int LengthBytes
	{
		get
		{
			return m_bitLength + 7 >> 3;
		}
		set
		{
			m_bitLength = value * 8;
			InternalEnsureBufferSize(m_bitLength);
		}
	}

	public int LengthBits
	{
		get
		{
			return m_bitLength;
		}
		set
		{
			m_bitLength = value;
			InternalEnsureBufferSize(m_bitLength);
		}
	}

	public long Position
	{
		get
		{
			return m_readPosition;
		}
		set
		{
			m_readPosition = (int)value;
		}
	}

	public int PositionInBytes => m_readPosition / 8;

	public byte[] PeekDataBuffer()
	{
		return m_data;
	}

	public bool PeekBoolean()
	{
		if (NetBitWriter.ReadByte(m_data, 1, m_readPosition) <= 0)
		{
			return false;
		}
		return true;
	}

	public byte PeekByte()
	{
		return NetBitWriter.ReadByte(m_data, 8, m_readPosition);
	}

	public sbyte PeekSByte()
	{
		return (sbyte)NetBitWriter.ReadByte(m_data, 8, m_readPosition);
	}

	public byte PeekByte(int numberOfBits)
	{
		return NetBitWriter.ReadByte(m_data, numberOfBits, m_readPosition);
	}

	public byte[] PeekBytes(int numberOfBytes)
	{
		byte[] array = new byte[numberOfBytes];
		NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, array, 0);
		return array;
	}

	public void PeekBytes(byte[] into, int offset, int numberOfBytes)
	{
		NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, into, offset);
	}

	public short PeekInt16()
	{
		return (short)NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
	}

	public ushort PeekUInt16()
	{
		return NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
	}

	public int PeekInt32()
	{
		return (int)NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
	}

	public int PeekInt32(int numberOfBits)
	{
		uint num = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
		if (numberOfBits == 32)
		{
			return (int)num;
		}
		int num2 = 1 << numberOfBits - 1;
		if ((num & num2) == 0L)
		{
			return (int)num;
		}
		uint num3 = uint.MaxValue >> 33 - numberOfBits;
		return (int)(0 - ((num & num3) + 1));
	}

	public uint PeekUInt32()
	{
		return NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
	}

	public uint PeekUInt32(int numberOfBits)
	{
		return NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
	}

	public ulong PeekUInt64()
	{
		long num = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		ulong num2 = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition + 32);
		return (ulong)num + (num2 << 32);
	}

	public long PeekInt64()
	{
		return (long)PeekUInt64();
	}

	public ulong PeekUInt64(int numberOfBits)
	{
		if (numberOfBits <= 32)
		{
			return NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
		}
		ulong num = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		return num | ((ulong)NetBitWriter.ReadUInt32(m_data, numberOfBits - 32, m_readPosition + 32) << 32);
	}

	public long PeekInt64(int numberOfBits)
	{
		return (long)PeekUInt64(numberOfBits);
	}

	public float PeekFloat()
	{
		return PeekSingle();
	}

	public float PeekSingle()
	{
		if ((m_readPosition & 7) == 0)
		{
			return BitConverter.ToSingle(m_data, m_readPosition >> 3);
		}
		return BitConverter.ToSingle(PeekBytes(4), 0);
	}

	public double PeekDouble()
	{
		if ((m_readPosition & 7) == 0)
		{
			return BitConverter.ToDouble(m_data, m_readPosition >> 3);
		}
		return BitConverter.ToDouble(PeekBytes(8), 0);
	}

	public string PeekString()
	{
		int readPosition = m_readPosition;
		string result = ReadString();
		m_readPosition = readPosition;
		return result;
	}

	public void ReadAllFields(object target)
	{
		ReadAllFields(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public void ReadAllFields(object target, BindingFlags flags)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		MemberInfo[] fields;
		MemberInfo[] array = (fields = target.GetType().GetFields(flags));
		NetUtility.SortMembersList(fields);
		FieldInfo[] array2 = (FieldInfo[])array;
		foreach (FieldInfo fieldInfo in array2)
		{
			if (s_readMethods.TryGetValue(fieldInfo.FieldType, out var value))
			{
				object value2 = value.Invoke(this, null);
				fieldInfo.SetValue(target, value2);
			}
		}
	}

	public void ReadAllProperties(object target)
	{
		ReadAllProperties(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public void ReadAllProperties(object target, BindingFlags flags)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		MemberInfo[] properties;
		MemberInfo[] array = (properties = target.GetType().GetProperties(flags));
		NetUtility.SortMembersList(properties);
		PropertyInfo[] array2 = (PropertyInfo[])array;
		foreach (PropertyInfo propertyInfo in array2)
		{
			if (s_readMethods.TryGetValue(propertyInfo.PropertyType, out var value))
			{
				object obj = value.Invoke(this, null);
				MethodInfo setMethod = propertyInfo.GetSetMethod();
				if (setMethod != null)
				{
					setMethod.Invoke(target, new object[1] { obj });
				}
			}
		}
	}

	public bool ReadBoolean()
	{
		byte num = NetBitWriter.ReadByte(m_data, 1, m_readPosition);
		m_readPosition++;
		if (num <= 0)
		{
			return false;
		}
		return true;
	}

	public byte ReadByte()
	{
		byte result = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
		m_readPosition += 8;
		return result;
	}

	public bool ReadByte(out byte result)
	{
		if (m_bitLength - m_readPosition < 8)
		{
			result = 0;
			return false;
		}
		result = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
		m_readPosition += 8;
		return true;
	}

	public sbyte ReadSByte()
	{
		byte num = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
		m_readPosition += 8;
		return (sbyte)num;
	}

	public byte ReadByte(int numberOfBits)
	{
		byte result = NetBitWriter.ReadByte(m_data, numberOfBits, m_readPosition);
		m_readPosition += numberOfBits;
		return result;
	}

	public byte[] ReadBytes(int numberOfBytes)
	{
		byte[] array = new byte[numberOfBytes];
		NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, array, 0);
		m_readPosition += 8 * numberOfBytes;
		return array;
	}

	public bool ReadBytes(int numberOfBytes, out byte[] result)
	{
		if (m_bitLength - m_readPosition + 7 < numberOfBytes * 8)
		{
			result = null;
			return false;
		}
		result = new byte[numberOfBytes];
		NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, result, 0);
		m_readPosition += 8 * numberOfBytes;
		return true;
	}

	public void ReadBytes(byte[] into, int offset, int numberOfBytes)
	{
		NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, into, offset);
		m_readPosition += 8 * numberOfBytes;
	}

	public void ReadBits(byte[] into, int offset, int numberOfBits)
	{
		int num = numberOfBits / 8;
		int num2 = numberOfBits - num * 8;
		NetBitWriter.ReadBytes(m_data, num, m_readPosition, into, offset);
		m_readPosition += 8 * num;
		if (num2 > 0)
		{
			into[offset + num] = ReadByte(num2);
		}
	}

	public short ReadInt16()
	{
		ushort num = NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
		m_readPosition += 16;
		return (short)num;
	}

	public ushort ReadUInt16()
	{
		ushort num = NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
		m_readPosition += 16;
		return num;
	}

	public int ReadInt32()
	{
		uint result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		m_readPosition += 32;
		return (int)result;
	}

	public bool ReadInt32(out int result)
	{
		if (m_bitLength - m_readPosition < 32)
		{
			result = 0;
			return false;
		}
		result = (int)NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		m_readPosition += 32;
		return true;
	}

	public int ReadInt32(int numberOfBits)
	{
		uint num = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
		m_readPosition += numberOfBits;
		if (numberOfBits == 32)
		{
			return (int)num;
		}
		int num2 = 1 << numberOfBits - 1;
		if ((num & num2) == 0L)
		{
			return (int)num;
		}
		uint num3 = uint.MaxValue >> 33 - numberOfBits;
		return (int)(0 - ((num & num3) + 1));
	}

	public uint ReadUInt32()
	{
		uint result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		m_readPosition += 32;
		return result;
	}

	public bool ReadUInt32(out uint result)
	{
		if (m_bitLength - m_readPosition < 32)
		{
			result = 0u;
			return false;
		}
		result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		m_readPosition += 32;
		return true;
	}

	public uint ReadUInt32(int numberOfBits)
	{
		uint result = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
		m_readPosition += numberOfBits;
		return result;
	}

	public ulong ReadUInt64()
	{
		long num = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		m_readPosition += 32;
		ulong num2 = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
		long result = num + (long)(num2 << 32);
		m_readPosition += 32;
		return (ulong)result;
	}

	public long ReadInt64()
	{
		return (long)ReadUInt64();
	}

	public ulong ReadUInt64(int numberOfBits)
	{
		ulong result;
		if (numberOfBits <= 32)
		{
			result = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
		}
		else
		{
			result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			result |= (ulong)NetBitWriter.ReadUInt32(m_data, numberOfBits - 32, m_readPosition + 32) << 32;
		}
		m_readPosition += numberOfBits;
		return result;
	}

	public long ReadInt64(int numberOfBits)
	{
		return (long)ReadUInt64(numberOfBits);
	}

	public float ReadFloat()
	{
		return ReadSingle();
	}

	public float ReadSingle()
	{
		if ((m_readPosition & 7) == 0)
		{
			float result = BitConverter.ToSingle(m_data, m_readPosition >> 3);
			m_readPosition += 32;
			return result;
		}
		return BitConverter.ToSingle(ReadBytes(4), 0);
	}

	public bool ReadSingle(out float result)
	{
		if (m_bitLength - m_readPosition < 32)
		{
			result = 0f;
			return false;
		}
		if ((m_readPosition & 7) == 0)
		{
			result = BitConverter.ToSingle(m_data, m_readPosition >> 3);
			m_readPosition += 32;
			return true;
		}
		byte[] value = ReadBytes(4);
		result = BitConverter.ToSingle(value, 0);
		return true;
	}

	public double ReadDouble()
	{
		if ((m_readPosition & 7) == 0)
		{
			double result = BitConverter.ToDouble(m_data, m_readPosition >> 3);
			m_readPosition += 64;
			return result;
		}
		return BitConverter.ToDouble(ReadBytes(8), 0);
	}

	public uint ReadVariableUInt32()
	{
		int num = 0;
		int num2 = 0;
		while (m_bitLength - m_readPosition >= 8)
		{
			byte b = ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
			if ((b & 0x80) == 0)
			{
				return (uint)num;
			}
		}
		return (uint)num;
	}

	public bool ReadVariableUInt32(out uint result)
	{
		int num = 0;
		int num2 = 0;
		while (m_bitLength - m_readPosition >= 8)
		{
			if (!ReadByte(out var result2))
			{
				result = 0u;
				return false;
			}
			num |= (result2 & 0x7F) << num2;
			num2 += 7;
			if ((result2 & 0x80) == 0)
			{
				result = (uint)num;
				return true;
			}
		}
		result = (uint)num;
		return false;
	}

	public int ReadVariableInt32()
	{
		uint num = ReadVariableUInt32();
		return (int)((num >> 1) ^ (0 - (num & 1)));
	}

	public long ReadVariableInt64()
	{
		ulong num = ReadVariableUInt64();
		return (long)((num >> 1) ^ (0L - (num & 1)));
	}

	public ulong ReadVariableUInt64()
	{
		ulong num = 0uL;
		int num2 = 0;
		while (m_bitLength - m_readPosition >= 8)
		{
			byte b = ReadByte();
			num |= ((ulong)b & 0x7FuL) << num2;
			num2 += 7;
			if ((b & 0x80) == 0)
			{
				return num;
			}
		}
		return num;
	}

	public float ReadSignedSingle(int numberOfBits)
	{
		uint num = ReadUInt32(numberOfBits);
		int num2 = (1 << numberOfBits) - 1;
		return ((float)(num + 1) / (float)(num2 + 1) - 0.5f) * 2f;
	}

	public float ReadUnitSingle(int numberOfBits)
	{
		uint num = ReadUInt32(numberOfBits);
		int num2 = (1 << numberOfBits) - 1;
		return (float)(num + 1) / (float)(num2 + 1);
	}

	public float ReadRangedSingle(float min, float max, int numberOfBits)
	{
		float num = max - min;
		int num2 = (1 << numberOfBits) - 1;
		float num3 = (float)ReadUInt32(numberOfBits) / (float)num2;
		return min + num3 * num;
	}

	public int ReadRangedInteger(int min, int max)
	{
		int numberOfBits = NetUtility.BitsToHoldUInt((uint)(max - min));
		uint num = ReadUInt32(numberOfBits);
		return (int)(min + num);
	}

	public long ReadRangedInteger(long min, long max)
	{
		int numberOfBits = NetUtility.BitsToHoldUInt64((ulong)(max - min));
		ulong num = ReadUInt64(numberOfBits);
		return min + (long)num;
	}

	public string ReadString()
	{
		int num = (int)ReadVariableUInt32();
		if (num <= 0)
		{
			return string.Empty;
		}
		if ((ulong)(m_bitLength - m_readPosition) < (ulong)((long)num * 8L))
		{
			m_readPosition = m_bitLength;
			return null;
		}
		if ((m_readPosition & 7) == 0)
		{
			string @string = Encoding.UTF8.GetString(m_data, m_readPosition >> 3, num);
			m_readPosition += 8 * num;
			return @string;
		}
		byte[] array = ReadBytes(num);
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}

	public bool ReadString(out string result)
	{
		if (!ReadVariableUInt32(out var result2))
		{
			result = string.Empty;
			return false;
		}
		if (result2 == 0)
		{
			result = string.Empty;
			return true;
		}
		if (m_bitLength - m_readPosition < result2 * 8)
		{
			result = string.Empty;
			return false;
		}
		if ((m_readPosition & 7) == 0)
		{
			result = Encoding.UTF8.GetString(m_data, m_readPosition >> 3, (int)result2);
			m_readPosition += (int)(8 * result2);
			return true;
		}
		if (!ReadBytes((int)result2, out var result3))
		{
			result = string.Empty;
			return false;
		}
		result = Encoding.UTF8.GetString(result3, 0, result3.Length);
		return true;
	}

	public double ReadTime(NetConnection connection, bool highPrecision)
	{
		double num = (highPrecision ? ReadDouble() : ((double)ReadSingle()));
		if (connection == null)
		{
			throw new NetException("Cannot call ReadTime() on message without a connected sender (ie. unconnected messages)");
		}
		return num - connection.m_remoteTimeOffset;
	}

	public IPEndPoint ReadIPEndPoint()
	{
		byte numberOfBytes = ReadByte();
		byte[] bytes = ReadBytes(numberOfBytes);
		return new IPEndPoint(port: ReadUInt16(), address: NetUtility.CreateAddressFromBytes(bytes));
	}

	public void SkipPadBits()
	{
		m_readPosition = (m_readPosition + 7 >> 3) * 8;
	}

	public void ReadPadBits()
	{
		m_readPosition = (m_readPosition + 7 >> 3) * 8;
	}

	public void SkipPadBits(int numberOfBits)
	{
		m_readPosition += numberOfBits;
	}

	public void WriteAllFields(object ob)
	{
		WriteAllFields(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public void WriteAllFields(object ob, BindingFlags flags)
	{
		if (ob == null)
		{
			return;
		}
		MemberInfo[] fields;
		MemberInfo[] array = (fields = ob.GetType().GetFields(flags));
		NetUtility.SortMembersList(fields);
		FieldInfo[] array2 = (FieldInfo[])array;
		foreach (FieldInfo fieldInfo in array2)
		{
			object value = fieldInfo.GetValue(ob);
			if (s_writeMethods.TryGetValue(fieldInfo.FieldType, out var value2))
			{
				value2.Invoke(this, new object[1] { value });
				continue;
			}
			throw new NetException("Failed to find write method for type " + fieldInfo.FieldType);
		}
	}

	public void WriteAllProperties(object ob)
	{
		WriteAllProperties(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public void WriteAllProperties(object ob, BindingFlags flags)
	{
		if (ob == null)
		{
			return;
		}
		MemberInfo[] properties;
		MemberInfo[] array = (properties = ob.GetType().GetProperties(flags));
		NetUtility.SortMembersList(properties);
		PropertyInfo[] array2 = (PropertyInfo[])array;
		foreach (PropertyInfo propertyInfo in array2)
		{
			MethodInfo getMethod = propertyInfo.GetGetMethod();
			if (getMethod != null)
			{
				object obj = getMethod.Invoke(ob, null);
				if (s_writeMethods.TryGetValue(propertyInfo.PropertyType, out var value))
				{
					value.Invoke(this, new object[1] { obj });
				}
			}
		}
	}

	public void EnsureBufferSize(int numberOfBits)
	{
		int num = numberOfBits + 7 >> 3;
		if (m_data == null)
		{
			m_data = new byte[num + 4];
		}
		else if (m_data.Length < num)
		{
			Array.Resize(ref m_data, num + 4);
		}
	}

	internal void InternalEnsureBufferSize(int numberOfBits)
	{
		int num = numberOfBits + 7 >> 3;
		if (m_data == null)
		{
			m_data = new byte[num];
		}
		else if (m_data.Length < num)
		{
			Array.Resize(ref m_data, num);
		}
	}

	public void Write(bool value)
	{
		EnsureBufferSize(m_bitLength + 1);
		NetBitWriter.WriteByte((byte)(value ? 1 : 0), 1, m_data, m_bitLength);
		m_bitLength++;
	}

	public void Write(byte source)
	{
		EnsureBufferSize(m_bitLength + 8);
		NetBitWriter.WriteByte(source, 8, m_data, m_bitLength);
		m_bitLength += 8;
	}

	public void WriteAt(int offset, byte source)
	{
		int num = Math.Max(m_bitLength, offset + 8);
		EnsureBufferSize(num);
		NetBitWriter.WriteByte(source, 8, m_data, offset);
		m_bitLength = num;
	}

	public void Write(sbyte source)
	{
		EnsureBufferSize(m_bitLength + 8);
		NetBitWriter.WriteByte((byte)source, 8, m_data, m_bitLength);
		m_bitLength += 8;
	}

	public void Write(byte source, int numberOfBits)
	{
		EnsureBufferSize(m_bitLength + numberOfBits);
		NetBitWriter.WriteByte(source, numberOfBits, m_data, m_bitLength);
		m_bitLength += numberOfBits;
	}

	public void Write(byte[] source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		int num = source.Length * 8;
		EnsureBufferSize(m_bitLength + num);
		NetBitWriter.WriteBytes(source, 0, source.Length, m_data, m_bitLength);
		m_bitLength += num;
	}

	public void Write(byte[] source, int offsetInBytes, int numberOfBytes)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		int num = numberOfBytes * 8;
		EnsureBufferSize(m_bitLength + num);
		NetBitWriter.WriteBytes(source, offsetInBytes, numberOfBytes, m_data, m_bitLength);
		m_bitLength += num;
	}

	public void Write(ushort source)
	{
		EnsureBufferSize(m_bitLength + 16);
		NetBitWriter.WriteUInt16(source, 16, m_data, m_bitLength);
		m_bitLength += 16;
	}

	public void WriteAt(int offset, ushort source)
	{
		int num = Math.Max(m_bitLength, offset + 16);
		EnsureBufferSize(num);
		NetBitWriter.WriteUInt16(source, 16, m_data, offset);
		m_bitLength = num;
	}

	public void Write(ushort source, int numberOfBits)
	{
		EnsureBufferSize(m_bitLength + numberOfBits);
		NetBitWriter.WriteUInt16(source, numberOfBits, m_data, m_bitLength);
		m_bitLength += numberOfBits;
	}

	public void Write(short source)
	{
		EnsureBufferSize(m_bitLength + 16);
		NetBitWriter.WriteUInt16((ushort)source, 16, m_data, m_bitLength);
		m_bitLength += 16;
	}

	public void WriteAt(int offset, short source)
	{
		int num = Math.Max(m_bitLength, offset + 16);
		EnsureBufferSize(num);
		NetBitWriter.WriteUInt16((ushort)source, 16, m_data, offset);
		m_bitLength = num;
	}

	public void Write(int source)
	{
		EnsureBufferSize(m_bitLength + 32);
		NetBitWriter.WriteUInt32((uint)source, 32, m_data, m_bitLength);
		m_bitLength += 32;
	}

	public void WriteAt(int offset, int source)
	{
		int num = Math.Max(m_bitLength, offset + 32);
		EnsureBufferSize(num);
		NetBitWriter.WriteUInt32((uint)source, 32, m_data, offset);
		m_bitLength = num;
	}

	public void Write(uint source)
	{
		EnsureBufferSize(m_bitLength + 32);
		NetBitWriter.WriteUInt32(source, 32, m_data, m_bitLength);
		m_bitLength += 32;
	}

	public void WriteAt(int offset, uint source)
	{
		int num = Math.Max(m_bitLength, offset + 32);
		EnsureBufferSize(num);
		NetBitWriter.WriteUInt32(source, 32, m_data, offset);
		m_bitLength = num;
	}

	public void Write(uint source, int numberOfBits)
	{
		EnsureBufferSize(m_bitLength + numberOfBits);
		NetBitWriter.WriteUInt32(source, numberOfBits, m_data, m_bitLength);
		m_bitLength += numberOfBits;
	}

	public void Write(int source, int numberOfBits)
	{
		EnsureBufferSize(m_bitLength + numberOfBits);
		if (numberOfBits != 32)
		{
			int num = 1 << numberOfBits - 1;
			source = ((source >= 0) ? (source & ~num) : ((-source - 1) | num));
		}
		NetBitWriter.WriteUInt32((uint)source, numberOfBits, m_data, m_bitLength);
		m_bitLength += numberOfBits;
	}

	public void Write(ulong source)
	{
		EnsureBufferSize(m_bitLength + 64);
		NetBitWriter.WriteUInt64(source, 64, m_data, m_bitLength);
		m_bitLength += 64;
	}

	public void WriteAt(int offset, ulong source)
	{
		int num = Math.Max(m_bitLength, offset + 64);
		EnsureBufferSize(num);
		NetBitWriter.WriteUInt64(source, 64, m_data, offset);
		m_bitLength = num;
	}

	public void Write(ulong source, int numberOfBits)
	{
		EnsureBufferSize(m_bitLength + numberOfBits);
		NetBitWriter.WriteUInt64(source, numberOfBits, m_data, m_bitLength);
		m_bitLength += numberOfBits;
	}

	public void Write(long source)
	{
		EnsureBufferSize(m_bitLength + 64);
		NetBitWriter.WriteUInt64((ulong)source, 64, m_data, m_bitLength);
		m_bitLength += 64;
	}

	public void Write(long source, int numberOfBits)
	{
		EnsureBufferSize(m_bitLength + numberOfBits);
		NetBitWriter.WriteUInt64((ulong)source, numberOfBits, m_data, m_bitLength);
		m_bitLength += numberOfBits;
	}

	public void Write(float source)
	{
		SingleUIntUnion singleUIntUnion = default(SingleUIntUnion);
		singleUIntUnion.UIntValue = 0u;
		singleUIntUnion.SingleValue = source;
		Write(singleUIntUnion.UIntValue);
	}

	public void Write(double source)
	{
		byte[] bytes = BitConverter.GetBytes(source);
		Write(bytes);
	}

	public int WriteVariableUInt32(uint value)
	{
		int num = 1;
		uint num2 = value;
		while (num2 >= 128)
		{
			Write((byte)(num2 | 0x80u));
			num2 >>= 7;
			num++;
		}
		Write((byte)num2);
		return num;
	}

	public int WriteVariableInt32(int value)
	{
		uint value2 = (uint)((value << 1) ^ (value >> 31));
		return WriteVariableUInt32(value2);
	}

	public int WriteVariableInt64(long value)
	{
		ulong value2 = (ulong)((value << 1) ^ (value >> 63));
		return WriteVariableUInt64(value2);
	}

	public int WriteVariableUInt64(ulong value)
	{
		int num = 1;
		ulong num2 = value;
		while (num2 >= 128)
		{
			Write((byte)(num2 | 0x80));
			num2 >>= 7;
			num++;
		}
		Write((byte)num2);
		return num;
	}

	public void WriteSignedSingle(float value, int numberOfBits)
	{
		float num = (value + 1f) * 0.5f;
		int num2 = (1 << numberOfBits) - 1;
		uint source = (uint)(num * (float)num2);
		Write(source, numberOfBits);
	}

	public void WriteUnitSingle(float value, int numberOfBits)
	{
		int num = (1 << numberOfBits) - 1;
		uint source = (uint)(value * (float)num);
		Write(source, numberOfBits);
	}

	public void WriteRangedSingle(float value, float min, float max, int numberOfBits)
	{
		float num = max - min;
		float num2 = (value - min) / num;
		int num3 = (1 << numberOfBits) - 1;
		Write((uint)((float)num3 * num2), numberOfBits);
	}

	public int WriteRangedInteger(int min, int max, int value)
	{
		int num = NetUtility.BitsToHoldUInt((uint)(max - min));
		uint source = (uint)(value - min);
		Write(source, num);
		return num;
	}

	public int WriteRangedInteger(long min, long max, long value)
	{
		int num = NetUtility.BitsToHoldUInt64((ulong)(max - min));
		ulong source = (ulong)(value - min);
		Write(source, num);
		return num;
	}

	public void Write(string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			WriteVariableUInt32(0u);
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(source);
		EnsureBufferSize(m_bitLength + 8 + bytes.Length * 8);
		WriteVariableUInt32((uint)bytes.Length);
		Write(bytes);
	}

	public void Write(IPEndPoint endPoint)
	{
		byte[] addressBytes = endPoint.Address.GetAddressBytes();
		Write((byte)addressBytes.Length);
		Write(addressBytes);
		Write((ushort)endPoint.Port);
	}

	public void WriteTime(bool highPrecision)
	{
		double now = NetTime.Now;
		if (highPrecision)
		{
			Write(now);
		}
		else
		{
			Write((float)now);
		}
	}

	public void WriteTime(double localTime, bool highPrecision)
	{
		if (highPrecision)
		{
			Write(localTime);
		}
		else
		{
			Write((float)localTime);
		}
	}

	public void WritePadBits()
	{
		m_bitLength = (m_bitLength + 7 >> 3) * 8;
		EnsureBufferSize(m_bitLength);
	}

	public void WritePadBits(int numberOfBits)
	{
		m_bitLength += numberOfBits;
		EnsureBufferSize(m_bitLength);
	}

	public void Write(NetBuffer buffer)
	{
		EnsureBufferSize(m_bitLength + buffer.LengthBytes * 8);
		Write(buffer.m_data, 0, buffer.LengthBytes);
		int num = buffer.m_bitLength % 8;
		if (num != 0)
		{
			int num2 = 8 - num;
			m_bitLength -= num2;
		}
	}

	static NetBuffer()
	{
		s_readMethods = new Dictionary<Type, MethodInfo>();
		MethodInfo[] methods = typeof(NetIncomingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.GetParameters().Length == 0 && methodInfo.Name.StartsWith("Read", StringComparison.InvariantCulture) && methodInfo.Name.Substring(4) == methodInfo.ReturnType.Name)
			{
				s_readMethods[methodInfo.ReturnType] = methodInfo;
			}
		}
		s_writeMethods = new Dictionary<Type, MethodInfo>();
		methods = typeof(NetOutgoingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
		foreach (MethodInfo methodInfo2 in methods)
		{
			if (methodInfo2.Name.Equals("Write", StringComparison.InvariantCulture))
			{
				ParameterInfo[] parameters = methodInfo2.GetParameters();
				if (parameters.Length == 1)
				{
					s_writeMethods[parameters[0].ParameterType] = methodInfo2;
				}
			}
		}
	}

	public void Write(Vector2 vector)
	{
		Write(vector.x);
		Write(vector.y);
	}

	public Vector2 ReadVector2()
	{
		return new Vector2(ReadSingle(), ReadSingle());
	}

	public void Write(Vector3 vector)
	{
		Write(vector.x);
		Write(vector.y);
		Write(vector.z);
	}

	public Vector3 ReadVector3()
	{
		return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
	}

	public void Write(Vector4 vector)
	{
		Write(vector.x);
		Write(vector.y);
		Write(vector.z);
		Write(vector.w);
	}

	public Vector4 ReadVector4()
	{
		return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
	}

	public void Write(Quaternion quaternion)
	{
		Write(quaternion.x);
		Write(quaternion.y);
		Write(quaternion.z);
		Write(quaternion.w);
	}

	public Quaternion ReadQuaternion()
	{
		return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
	}

	public void WriteRgbColor(Color32 color)
	{
		Write(color.r);
		Write(color.g);
		Write(color.b);
	}

	public Color32 ReadRgbColor()
	{
		return new Color32(ReadByte(), ReadByte(), ReadByte(), byte.MaxValue);
	}

	public void WriteRgbaColor(Color32 color)
	{
		Write(color.r);
		Write(color.g);
		Write(color.b);
		Write(color.a);
	}

	public Color32 ReadRgbaColor()
	{
		return new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());
	}

	public void Write(Ray ray)
	{
		Write(ray.direction);
		Write(ray.origin);
	}

	public Ray ReadRay()
	{
		Vector3 direction = ReadVector3();
		return new Ray(ReadVector3(), direction);
	}

	public void Write(Plane plane)
	{
		Write(plane.normal);
		Write(plane.distance);
	}

	public Plane ReadPlane()
	{
		return new Plane(ReadVector3(), ReadSingle());
	}

	public void Write(Matrix4x4 matrix)
	{
		Write(matrix.m00);
		Write(matrix.m01);
		Write(matrix.m02);
		Write(matrix.m03);
		Write(matrix.m10);
		Write(matrix.m11);
		Write(matrix.m12);
		Write(matrix.m13);
		Write(matrix.m20);
		Write(matrix.m21);
		Write(matrix.m22);
		Write(matrix.m23);
		Write(matrix.m30);
		Write(matrix.m31);
		Write(matrix.m32);
		Write(matrix.m33);
	}

	public Matrix4x4 ReadMatrix4X4()
	{
		Matrix4x4 result = default(Matrix4x4);
		result.m00 = ReadSingle();
		result.m01 = ReadSingle();
		result.m02 = ReadSingle();
		result.m03 = ReadSingle();
		result.m10 = ReadSingle();
		result.m11 = ReadSingle();
		result.m12 = ReadSingle();
		result.m13 = ReadSingle();
		result.m20 = ReadSingle();
		result.m21 = ReadSingle();
		result.m22 = ReadSingle();
		result.m23 = ReadSingle();
		result.m30 = ReadSingle();
		result.m31 = ReadSingle();
		result.m32 = ReadSingle();
		result.m33 = ReadSingle();
		return result;
	}

	public void Write(Rect rect)
	{
		Write(rect.xMin);
		Write(rect.yMin);
		Write(rect.width);
		Write(rect.height);
	}

	public Rect ReadRect()
	{
		return new Rect(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
	}
}
