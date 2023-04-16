using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network;

public abstract class NetCryptoProviderEncryption : NetEncryption
{
	public NetCryptoProviderEncryption(NetPeer peer)
		: base(peer)
	{
	}

	protected abstract CryptoStream GetEncryptStream(MemoryStream ms);

	protected abstract CryptoStream GetDecryptStream(MemoryStream ms);

	public override bool Encrypt(NetOutgoingMessage msg)
	{
		int lengthBits = msg.LengthBits;
		MemoryStream memoryStream = new MemoryStream();
		CryptoStream encryptStream = GetEncryptStream(memoryStream);
		encryptStream.Write(msg.m_data, 0, msg.LengthBytes);
		encryptStream.Close();
		byte[] array = memoryStream.ToArray();
		memoryStream.Close();
		msg.EnsureBufferSize((array.Length + 4) * 8);
		msg.LengthBits = 0;
		msg.Write((uint)lengthBits);
		msg.Write(array);
		msg.LengthBits = (array.Length + 4) * 8;
		return true;
	}

	public override bool Decrypt(NetIncomingMessage msg)
	{
		int num = (int)msg.ReadUInt32();
		MemoryStream ms = new MemoryStream(msg.m_data, 4, msg.LengthBytes - 4);
		CryptoStream decryptStream = GetDecryptStream(ms);
		byte[] storage = m_peer.GetStorage(num);
		decryptStream.Read(storage, 0, NetUtility.BytesToHoldBits(num));
		decryptStream.Close();
		msg.m_data = storage;
		msg.m_bitLength = num;
		return true;
	}
}
