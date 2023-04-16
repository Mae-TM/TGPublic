using Mirror;

public struct PesterchumStatusChange : NetworkMessage
{
	public string sender;

	public string receiver;

	public bool status;
}
