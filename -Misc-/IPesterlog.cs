public interface IPesterlog
{
	void OnStatusChanged(string chum, PesterchumStatusChange msg);

	void OnMessageReceived(PesterchumMessage message);
}
