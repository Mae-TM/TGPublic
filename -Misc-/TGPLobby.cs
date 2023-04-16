using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class TGPLobby
{
	public enum LobbyVisibility
	{
		Private,
		FriendsOnly,
		Public
	}

	public class PerPlayerInformation
	{
		public Friend Friend { get; set; }

		public PlayerSpriteData Sprite { get; set; }

		public int[] Class { get; set; }

		public int[] Aspect { get; set; }

		public string House { get; set; }

		public string Modus { get; set; }
	}

	public string Name { get; private set; }

	public Lobby SteamLobby { get; private set; }

	public LobbyVisibility Visibility { get; private set; }

	public Dictionary<SteamId, PerPlayerInformation> PlayerInformation { get; private set; }

	public List<SteamId> GateOrder { get; private set; }

	public IEnumerable<PerPlayerInformation> OrderedPlayerInformation
	{
		get
		{
			PerPlayerInformation value;
			return GateOrder.Select((SteamId player) => (!PlayerInformation.TryGetValue(player, out value)) ? null : value);
		}
	}

	public string Password { get; private set; }

	public string JoinSecret { get; private set; }

	public string SessionName { get; private set; }

	public int RandomSeed { get; private set; }

	public bool AllowDuplicateClasses { get; private set; }

	public bool AllowDuplicateAspects { get; private set; }

	public bool AllowDuplicateClasspects { get; private set; }

	public int GameBuild { get; private set; }

	public bool IsFull => SteamLobby.MemberCount >= SteamLobby.MaxMembers;

	public bool HasPassword => !string.IsNullOrEmpty(Password);

	public static TGPLobby FromLobbyFull(Lobby lobby)
	{
		TGPLobby tGPLobby = new TGPLobby();
		tGPLobby.SteamLobby = lobby;
		tGPLobby.PlayerInformation = new Dictionary<SteamId, PerPlayerInformation>();
		tGPLobby.Name = lobby.GetData("name");
		string data = lobby.GetData("visibility");
		tGPLobby.Visibility = (string.IsNullOrEmpty(data) ? LobbyVisibility.Public : ((LobbyVisibility)int.Parse(lobby.GetData("visibility"))));
		tGPLobby.Password = lobby.GetData("password");
		tGPLobby.GateOrder = JsonConvert.DeserializeObject<List<SteamId>>(lobby.GetData("GateOrder"));
		TGPLobby tGPLobby2 = tGPLobby;
		if (tGPLobby2.GateOrder == null)
		{
			List<SteamId> obj = new List<SteamId> { SteamClient.SteamId };
			List<SteamId> list = obj;
			tGPLobby2.GateOrder = obj;
		}
		string data2 = lobby.GetData("gameBuild");
		if (!string.IsNullOrEmpty(data2))
		{
			tGPLobby.GameBuild = int.Parse(data2);
		}
		string data3 = lobby.GetData("allowDuplicateClasses");
		tGPLobby.AllowDuplicateClasses = !string.IsNullOrEmpty(data3) && bool.Parse(data3);
		string data4 = lobby.GetData("allowDuplicateAspects");
		tGPLobby.AllowDuplicateAspects = !string.IsNullOrEmpty(data4) && bool.Parse(data4);
		string data5 = lobby.GetData("allowDuplicateClasspects");
		tGPLobby.AllowDuplicateClasspects = !string.IsNullOrEmpty(data5) && bool.Parse(data5);
		foreach (Friend member in lobby.Members)
		{
			PerPlayerInformation perPlayerInformation = new PerPlayerInformation
			{
				Friend = member
			};
			string memberData = lobby.GetMemberData(member, "sprite");
			if (!string.IsNullOrEmpty(memberData))
			{
				perPlayerInformation.Sprite = JsonConvert.DeserializeObject<PlayerSpriteData>(memberData);
			}
			string memberData2 = lobby.GetMemberData(member, "class");
			if (!string.IsNullOrEmpty(memberData2))
			{
				perPlayerInformation.Class = JsonConvert.DeserializeObject<int[]>(memberData2);
			}
			string memberData3 = lobby.GetMemberData(member, "aspect");
			if (!string.IsNullOrEmpty(memberData3))
			{
				perPlayerInformation.Aspect = JsonConvert.DeserializeObject<int[]>(memberData3);
			}
			perPlayerInformation.House = lobby.GetMemberData(member, "house");
			perPlayerInformation.Modus = lobby.GetMemberData(member, "modus");
			tGPLobby.PlayerInformation.Add(member.Id, perPlayerInformation);
		}
		return tGPLobby;
	}

	public void DataUpdated(Lobby lobby)
	{
		SteamLobby = lobby;
		SteamLobby = lobby;
		PlayerInformation = new Dictionary<SteamId, PerPlayerInformation>();
		Name = lobby.GetData("name");
		string data = lobby.GetData("visibility");
		Visibility = (string.IsNullOrEmpty(data) ? LobbyVisibility.Public : ((LobbyVisibility)int.Parse(lobby.GetData("visibility"))));
		Password = lobby.GetData("password");
		GateOrder = JsonConvert.DeserializeObject<List<SteamId>>(lobby.GetData("GateOrder"));
		if (GateOrder == null)
		{
			List<SteamId> obj = new List<SteamId> { SteamClient.SteamId };
			List<SteamId> list = obj;
			GateOrder = obj;
		}
		string data2 = lobby.GetData("gameBuild");
		if (!string.IsNullOrEmpty(data2))
		{
			GameBuild = int.Parse(data2);
		}
		string data3 = lobby.GetData("allowDuplicateClasses");
		AllowDuplicateClasses = !string.IsNullOrEmpty(data3) && bool.Parse(data3);
		string data4 = lobby.GetData("allowDuplicateAspects");
		AllowDuplicateAspects = !string.IsNullOrEmpty(data4) && bool.Parse(data4);
		string data5 = lobby.GetData("allowDuplicateClasspects");
		AllowDuplicateClasspects = !string.IsNullOrEmpty(data5) && bool.Parse(data5);
		string data6 = lobby.GetData("randomSeed");
		RandomSeed = ((!string.IsNullOrEmpty(data6)) ? int.Parse(data6) : 0);
		foreach (Friend member in lobby.Members)
		{
			PerPlayerInformation perPlayerInformation = new PerPlayerInformation
			{
				Friend = member
			};
			string memberData = lobby.GetMemberData(member, "sprite");
			if (!string.IsNullOrEmpty(memberData))
			{
				perPlayerInformation.Sprite = JsonConvert.DeserializeObject<PlayerSpriteData>(memberData);
			}
			string memberData2 = lobby.GetMemberData(member, "class");
			if (!string.IsNullOrEmpty(memberData2))
			{
				perPlayerInformation.Class = JsonConvert.DeserializeObject<int[]>(memberData2);
			}
			string memberData3 = lobby.GetMemberData(member, "aspect");
			if (!string.IsNullOrEmpty(memberData3))
			{
				perPlayerInformation.Aspect = JsonConvert.DeserializeObject<int[]>(memberData3);
			}
			perPlayerInformation.House = lobby.GetMemberData(member, "house");
			perPlayerInformation.Modus = lobby.GetMemberData(member, "modus");
			if (PlayerInformation.ContainsKey(member.Id))
			{
				PlayerInformation[member.Id] = perPlayerInformation;
			}
			else
			{
				PlayerInformation.Add(member.Id, perPlayerInformation);
			}
		}
	}

	public void SetName(string newName)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			Name = newName;
			SteamLobby.SetData("name", newName);
			Debug.Log("Name is now: " + SteamLobby.GetData("name"));
		}
	}

	public void SetSessionName(string newSessionName)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			SessionName = newSessionName;
			SteamLobby.SetData("sessionName", newSessionName);
			Debug.Log("SessionName is now: " + SteamLobby.GetData("sessionName"));
		}
	}

	public void SetRandomSeed(int seed)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			RandomSeed = seed;
			SteamLobby.SetData("randomSeed", seed.ToString());
			Debug.Log("RandomSeed is now: " + SteamLobby.GetData("randomSeed"));
		}
	}

	public void SetVisibility(LobbyVisibility newVisibility)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			Visibility = newVisibility;
			Lobby steamLobby = SteamLobby;
			int num = (int)newVisibility;
			steamLobby.SetData("visibility", num.ToString());
			switch (newVisibility)
			{
			case LobbyVisibility.Public:
				SteamLobby.SetPublic();
				break;
			case LobbyVisibility.FriendsOnly:
				SteamLobby.SetFriendsOnly();
				break;
			case LobbyVisibility.Private:
				SteamLobby.SetPrivate();
				break;
			default:
				throw new ArgumentOutOfRangeException("newVisibility", newVisibility, null);
			}
		}
	}

	public void SetPassword(string password)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(password);
			string value = (Password = Convert.ToBase64String(SHA256.Create().ComputeHash(bytes)));
			SteamLobby.SetData("password", value);
			string joinSecret = $"{SteamLobby.Id.Value};{Convert.ToBase64String(bytes)}";
			JoinSecret = joinSecret;
		}
	}

	public void SetAllowDuplicateClasses(bool allowDuplicateClasses)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			AllowDuplicateClasses = allowDuplicateClasses;
			SteamLobby.SetData("allowDuplicateClasses", allowDuplicateClasses.ToString());
		}
	}

	public void SetAllowDuplicateAspects(bool allowDuplicateAspects)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			AllowDuplicateAspects = allowDuplicateAspects;
			SteamLobby.SetData("allowDuplicateAspects", allowDuplicateAspects.ToString());
		}
	}

	public void SetAllowDuplicateClasspects(bool allowDuplicateClasspects)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			AllowDuplicateClasspects = allowDuplicateClasspects;
			SteamLobby.SetData("allowDuplicateClasspects", allowDuplicateClasspects.ToString());
		}
	}

	public void SetGameBuild()
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			SteamLobby.SetData("gameBuild", SteamApps.BuildId.ToString());
		}
	}

	public bool IsPasswordCorrect(string password)
	{
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			return true;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(password);
		string text = Convert.ToBase64String(SHA256.Create().ComputeHash(bytes));
		return Password == text;
	}

	public void SetPlayerSprite(PlayerSpriteData sprite)
	{
		string value = JsonConvert.SerializeObject(sprite);
		SteamLobby.SetMemberData("sprite", value);
	}

	public void SetPlayerClass(int[] classArray)
	{
		string value = JsonConvert.SerializeObject(classArray);
		SteamLobby.SetMemberData("class", value);
	}

	public void SetPlayerAspect(int[] aspectArray)
	{
		string value = JsonConvert.SerializeObject(aspectArray);
		SteamLobby.SetMemberData("aspect", value);
	}

	public void SetPlayerHouse(string house)
	{
		SteamLobby.SetMemberData("house", house);
	}

	public void SetPlayerModus(string modus)
	{
		SteamLobby.SetMemberData("modus", modus);
	}

	public void SetOwner(SteamId newOwner)
	{
		Lobby steamLobby = SteamLobby;
		if (steamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			steamLobby.Owner = new Friend
			{
				Id = newOwner
			};
		}
	}

	public void SendJoin()
	{
		SteamLobby.SetGameServer(SteamClient.SteamId);
	}

	public bool GetGameServer(out SteamId steamId)
	{
		uint ip = 0u;
		ushort port = 0;
		steamId = default(SteamId);
		return SteamLobby.GetGameServer(ref ip, ref port, ref steamId);
	}

	public void AddPlayer(Friend friend)
	{
		PerPlayerInformation perPlayerInformation = new PerPlayerInformation
		{
			Friend = friend
		};
		string memberData = SteamLobby.GetMemberData(friend, "sprite");
		if (!string.IsNullOrEmpty(memberData))
		{
			perPlayerInformation.Sprite = JsonConvert.DeserializeObject<PlayerSpriteData>(memberData);
		}
		string memberData2 = SteamLobby.GetMemberData(friend, "class");
		if (!string.IsNullOrEmpty(memberData2))
		{
			perPlayerInformation.Class = JsonConvert.DeserializeObject<int[]>(memberData2);
		}
		string memberData3 = SteamLobby.GetMemberData(friend, "aspect");
		if (!string.IsNullOrEmpty(memberData3))
		{
			perPlayerInformation.Aspect = JsonConvert.DeserializeObject<int[]>(memberData3);
		}
		perPlayerInformation.House = SteamLobby.GetMemberData(friend, "house");
		perPlayerInformation.Modus = SteamLobby.GetMemberData(friend, "modus");
		if (PlayerInformation.ContainsKey(friend.Id))
		{
			PlayerInformation[friend.Id] = perPlayerInformation;
		}
		else
		{
			PlayerInformation.Add(friend.Id, perPlayerInformation);
		}
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId) && !GateOrder.Contains(friend.Id))
		{
			GateOrder.Add(friend.Id);
			SteamLobby.SetData("GateOrder", JsonConvert.SerializeObject(GateOrder));
		}
	}

	public void RemovePlayer(SteamId friendId)
	{
		PlayerInformation.Remove(friendId);
		if (SteamLobby.IsOwnedBy(SteamClient.SteamId) && GateOrder.Contains(friendId))
		{
			GateOrder.Remove(friendId);
			SteamLobby.SetData("GateOrder", JsonConvert.SerializeObject(GateOrder));
		}
	}

	public static TGPLobby FromLobby(Lobby lobby)
	{
		TGPLobby tGPLobby = new TGPLobby();
		tGPLobby.SteamLobby = lobby;
		tGPLobby.PlayerInformation = new Dictionary<SteamId, PerPlayerInformation>();
		tGPLobby.Name = lobby.GetData("name");
		string data = lobby.GetData("visibility");
		tGPLobby.Visibility = (string.IsNullOrEmpty(data) ? LobbyVisibility.Public : ((LobbyVisibility)int.Parse(lobby.GetData("visibility"))));
		tGPLobby.Password = lobby.GetData("password");
		string data2 = lobby.GetData("gameBuild");
		if (!string.IsNullOrEmpty(data2))
		{
			tGPLobby.GameBuild = int.Parse(data2);
		}
		return tGPLobby;
	}
}
