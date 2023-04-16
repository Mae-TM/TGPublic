using System.IO;
using System.IO.Compression;
using ProtoBuf;

[ProtoContract]
public struct CompressedBytes
{
	[ProtoMember(1)]
	private byte[] bytes;

	public CompressedBytes(byte[] bytes = null)
	{
		this.bytes = bytes;
	}

	public static implicit operator byte[](CompressedBytes i)
	{
		return i.bytes;
	}

	public static implicit operator CompressedBytes(byte[] i)
	{
		return new CompressedBytes(i);
	}

	[ProtoBeforeSerialization]
	public void Pack()
	{
		if (bytes == null || bytes.Length == 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			deflateStream.Write(bytes, 0, bytes.Length);
		}
		bytes = memoryStream.ToArray();
	}

	[ProtoAfterDeserialization]
	public void Unpack()
	{
		if (bytes == null || bytes.Length == 0)
		{
			return;
		}
		using MemoryStream stream = new MemoryStream(bytes);
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
		{
			byte[] array = new byte[100];
			int count;
			while ((count = deflateStream.Read(array, 0, array.Length)) != 0)
			{
				memoryStream.Write(array, 0, count);
			}
		}
		bytes = memoryStream.ToArray();
	}
}
