using System.IO;

public class CrashableFileStream : FileStream
{
	public CrashableFileStream(string path)
		: base(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.SequentialScan)
	{
	}

	private void checkIfTooLong()
	{
		if (base.Position > base.Length)
		{
			throw new EndOfStreamException();
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = base.Read(buffer, offset, count);
		if (num != count)
		{
			throw new EndOfStreamException();
		}
		return num;
	}

	public override int ReadByte()
	{
		int num = base.ReadByte();
		if (num == -1)
		{
			throw new EndOfStreamException();
		}
		return num;
	}
}
