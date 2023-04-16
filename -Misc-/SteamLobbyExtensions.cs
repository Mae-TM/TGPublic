using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

public static class SteamLobbyExtensions
{
	public static async Task<Lobby?> RefreshAsync(this Lobby lobby)
	{
		TaskCompletionSource<Lobby> resultWaiter = new TaskCompletionSource<Lobby>();
		SteamMatchmaking.OnLobbyDataChanged += EventHandler;
		lobby.Refresh();
		if (await Task.WhenAny(resultWaiter.Task, Task.Delay(TimeSpan.FromSeconds(3.0))) == resultWaiter.Task)
		{
			SteamMatchmaking.OnLobbyDataChanged -= EventHandler;
			return resultWaiter.Task.Result;
		}
		SteamMatchmaking.OnLobbyDataChanged -= EventHandler;
		return null;
		void EventHandler(Lobby queriedLobby)
		{
			if ((ulong)lobby.Id == (ulong)queriedLobby.Id)
			{
				resultWaiter.SetResult(queriedLobby);
			}
		}
	}
}
