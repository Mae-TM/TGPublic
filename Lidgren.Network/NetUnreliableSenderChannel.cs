namespace Lidgren.Network;

internal sealed class NetUnreliableSenderChannel : NetSenderChannelBase
{
	private NetConnection m_connection;

	private int m_windowStart;

	private int m_windowSize;

	private int m_sendStart;

	private bool m_doFlowControl;

	private NetBitVector m_receivedAcks;

	internal override int WindowSize => m_windowSize;

	internal NetUnreliableSenderChannel(NetConnection connection, int windowSize, NetDeliveryMethod method)
	{
		m_connection = connection;
		m_windowSize = windowSize;
		m_windowStart = 0;
		m_sendStart = 0;
		m_receivedAcks = new NetBitVector(1024);
		m_queuedSends = new NetQueue<NetOutgoingMessage>(8);
		m_doFlowControl = true;
		if (method == NetDeliveryMethod.Unreliable && connection.Peer.Configuration.SuppressUnreliableUnorderedAcks)
		{
			m_doFlowControl = false;
		}
	}

	internal override int GetAllowedSends()
	{
		if (!m_doFlowControl)
		{
			return 2;
		}
		return m_windowSize - (m_sendStart + 1024 - m_windowStart) % m_windowSize;
	}

	internal override void Reset()
	{
		m_receivedAcks.Clear();
		m_queuedSends.Clear();
		m_windowStart = 0;
		m_sendStart = 0;
	}

	internal override NetSendResult Enqueue(NetOutgoingMessage message)
	{
		int num = m_queuedSends.Count + 1;
		int allowedSends = GetAllowedSends();
		if (num > allowedSends || (message.LengthBytes > m_connection.m_currentMTU && m_connection.m_peerConfiguration.UnreliableSizeBehaviour == NetUnreliableSizeBehaviour.DropAboveMTU))
		{
			return NetSendResult.Dropped;
		}
		m_queuedSends.Enqueue(message);
		m_connection.m_peer.m_needFlushSendQueue = true;
		return NetSendResult.Sent;
	}

	internal override void SendQueuedMessages(double now)
	{
		int num = GetAllowedSends();
		if (num < 1)
		{
			return;
		}
		while (num > 0 && m_queuedSends.Count > 0)
		{
			if (m_queuedSends.TryDequeue(out var item))
			{
				ExecuteSend(item);
			}
			num--;
		}
	}

	private void ExecuteSend(NetOutgoingMessage message)
	{
		int sendStart = m_sendStart;
		m_sendStart = (m_sendStart + 1) % 1024;
		m_connection.QueueSendMessage(message, sendStart);
		if (message.m_recyclingCount <= 0)
		{
			m_connection.m_peer.Recycle(message);
		}
	}

	internal override void ReceiveAcknowledge(double now, int seqNr)
	{
		if (!m_doFlowControl)
		{
			m_connection.m_peer.LogWarning("SuppressUnreliableUnorderedAcks sender/receiver mismatch!");
			return;
		}
		int num = NetUtility.RelativeSequenceNumber(seqNr, m_windowStart);
		if (num < 0)
		{
			return;
		}
		if (num == 0)
		{
			m_receivedAcks[m_windowStart] = false;
			m_windowStart = (m_windowStart + 1) % 1024;
			return;
		}
		m_receivedAcks[seqNr] = true;
		while (m_windowStart != seqNr)
		{
			m_receivedAcks[m_windowStart] = false;
			m_windowStart = (m_windowStart + 1) % 1024;
		}
	}
}
