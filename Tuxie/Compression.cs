using System.Collections;
using System.IO;
using System.Threading;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Tuxie;

internal class Compression
{
	private volatile bool isdone;

	private ICodeProgress callback;

	private Stream input;

	private Stream output;

	public void CompressLZMAThread()
	{
		bool flag = false;
		int num = 1048576;
		CoderPropID[] propIDs = new CoderPropID[8]
		{
			CoderPropID.DictionarySize,
			CoderPropID.PosStateBits,
			CoderPropID.LitContextBits,
			CoderPropID.LitPosBits,
			CoderPropID.Algorithm,
			CoderPropID.NumFastBytes,
			CoderPropID.MatchFinder,
			CoderPropID.EndMarker
		};
		object[] properties = new object[8] { num, 2, 3, 0, 2, 128, "bt4", flag };
		Encoder encoder = new Encoder();
		encoder.SetCoderProperties(propIDs, properties);
		encoder.WriteCoderProperties(output);
		long length = input.Length;
		for (int i = 0; i < 8; i++)
		{
			output.WriteByte((byte)(length >> 8 * i));
		}
		encoder.Code(input, output, -1L, -1L, callback);
		isdone = true;
	}

	public Compression(Stream input, Stream output, ICodeProgress callback)
	{
		this.callback = callback;
		this.input = input;
		this.output = output;
	}

	public IEnumerator CompressLZMA(ICodeProgress progresscallback)
	{
		new Thread(CompressLZMAThread).Start();
		while (!isdone)
		{
			yield return null;
		}
	}
}
