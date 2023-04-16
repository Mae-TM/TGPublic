using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyPlayerList : MonoBehaviour
{
	[SerializeField]
	private LobbyPlayer PlayerTemplate;

	[SerializeField]
	private LobbyPlayer LocalPlayer;

	[SerializeField]
	private ChangeSpritePart _changeSpritePart;

	[SerializeField]
	private LobbyClasspectWizard _roleWindow;

	[SerializeField]
	private ModusPicker _modusPicker;

	[SerializeField]
	private HousePickerComponent _housePicker;

	[SerializeField]
	private InLobbyComponent _inLobbyComponent;

	[FormerlySerializedAs("LobbyComponent")]
	[SerializeField]
	private LobbySceneComponent lobbySceneComponent;

	private Mask mask;

	private Dictionary<SteamId, LobbyPlayer> _lobbyListPlayers;

	private readonly ClasspectSolver classpectSolver = new ClasspectSolver();

	public int PlayerAmount => AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.MemberCount;

	private void OnEnable()
	{
		_lobbyListPlayers = new Dictionary<SteamId, LobbyPlayer>();
		mask = base.transform.parent.GetComponent<Mask>();
		LocalPlayer.parent.SetActive(value: false);
		ChangeSpritePart changeSpritePart = _changeSpritePart;
		changeSpritePart.OnDone = (ChangeSpritePart.OnDoneEvent)Delegate.Combine(changeSpritePart.OnDone, new ChangeSpritePart.OnDoneEvent(OnPlayerSaved));
		_roleWindow.OnDone += OnClasspectDone;
		_modusPicker.OnPickModus += OnModusPicked;
		HousePickerComponent housePicker = _housePicker;
		housePicker.OnDone = (HousePickerComponent.OnDoneEvent)Delegate.Combine(housePicker.OnDone, new HousePickerComponent.OnDoneEvent(OnHousePicked));
		AddPlayer(SteamClient.SteamId, SteamClient.Name, isLocalPlayer: true);
		_changeSpritePart.LoadCharacterFromPrefs();
		int[] playerClass = ClassPick.CalculateScore();
		int[] playerAspect = AspectPick.CalculateScore();
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerSprite(_changeSpritePart.SaveToBuffer());
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerClass(playerClass);
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerAspect(playerAspect);
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerHouse(PlayerPrefs.GetString("House", "???"));
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerModus(PlayerPrefs.GetString("Modus", "Queue"));
	}

	public void UpdateView()
	{
		foreach (TGPLobby.PerPlayerInformation value in AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.PlayerInformation.Values)
		{
			UpdateChangeSpritePart(value.Friend.Id, value.Sprite);
			UpdateChangeClasspect(value.Class, value.Aspect, value.Friend.Id);
			UpdateChangeHouse(value.Friend.Id, value.House);
			UpdateChangeModus(value.Friend.Id, value.Modus);
		}
		SetLobbyOwner(AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.IsOwnedBy(SteamClient.SteamId), AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.Owner.Id);
		classpectSolver.avoidDuplicateAspects = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.AllowDuplicateAspects;
		classpectSolver.avoidDuplicateClasses = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.AllowDuplicateClasses;
		classpectSolver.avoidDuplicateClasspects = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.AllowDuplicateClasspects;
	}

	private void OnPlayerSaved()
	{
		PlayerSpriteData playerSprite = _changeSpritePart.SaveToBuffer();
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerSprite(playerSprite);
	}

	private void OnClasspectDone(object sender, EventArgs e)
	{
		_roleWindow.gameObject.SetActive(value: false);
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerClass(ClassPick.score);
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerAspect(AspectPick.score);
	}

	private void OnHousePicked(string house)
	{
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerHouse(house);
	}

	private void OnModusPicked(string modus)
	{
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetPlayerModus(modus);
	}

	private void OnImageClick()
	{
		_changeSpritePart.LoadFromBuffer(AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.PlayerInformation[SteamClient.SteamId].Sprite);
		_changeSpritePart.transform.parent.gameObject.SetActive(value: true);
		_changeSpritePart.gameObject.SetActive(value: true);
	}

	public void OnClasspectClick()
	{
		_roleWindow.ResetWizard();
		_roleWindow.gameObject.SetActive(value: true);
	}

	public void SetLobbyOwner(bool isLocalPlayerNewOwner, SteamId user)
	{
		LocalPlayer.SetLobbyOwner(isLocalPlayerNewOwner, isLocalPlayerNewOwner);
		PlayerTemplate.SetLobbyOwner(isLocalPlayerNewOwner, isThisPlayer: false);
		foreach (KeyValuePair<SteamId, LobbyPlayer> lobbyListPlayer in _lobbyListPlayers)
		{
			lobbyListPlayer.Value.SetLobbyOwner(isLocalPlayerNewOwner, lobbyListPlayer.Key.Value == user.Value);
		}
	}

	public void SetAllowDuplicateClasses(bool to)
	{
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetAllowDuplicateClasses(!to);
	}

	public void SetAllowDuplicateAspect(bool to)
	{
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetAllowDuplicateAspects(!to);
	}

	public void SetAllowDuplicateClasspects(bool to)
	{
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetAllowDuplicateClasspects(!to);
	}

	public void SetLoadedSession(bool to)
	{
		LocalPlayer.SetReadOnly(to);
		PlayerTemplate.SetReadOnly(to);
	}

	public void RemoveAllPlayers()
	{
		foreach (LobbyPlayer value in _lobbyListPlayers.Values)
		{
			if (value != LocalPlayer)
			{
				UnityEngine.Object.Destroy(value.parent);
			}
		}
		_lobbyListPlayers.Clear();
		classpectSolver.Clear();
		LocalPlayer.parent.SetActive(value: false);
	}

	public void RemovePlayer(SteamId user)
	{
		if (_lobbyListPlayers.TryGetValue(user, out var value))
		{
			if (value != LocalPlayer)
			{
				UnityEngine.Object.Destroy(value.parent);
			}
			else
			{
				value.gameObject.SetActive(value: false);
			}
			_lobbyListPlayers.Remove(user);
			classpectSolver.RemovePlayer(user);
		}
	}

	public void AddPlayer(SteamId user, string steamName, bool isLocalPlayer)
	{
		if (_lobbyListPlayers.ContainsKey(user))
		{
			return;
		}
		LobbyPlayer newPlayer = (isLocalPlayer ? LocalPlayer : UnityEngine.Object.Instantiate(PlayerTemplate, base.transform));
		newPlayer.parent.SetActive(value: true);
		newPlayer.TxtUsername.text = steamName;
		_lobbyListPlayers.Add(user, newPlayer);
		if (!isLocalPlayer)
		{
			newPlayer.characterLook.MakeNewMaterials();
		}
		classpectSolver.AddPlayer(user, delegate(Classpect classpect)
		{
			newPlayer.TxtClasspect.text = classpect.ToString();
			if (isLocalPlayer)
			{
				ClassPick.chosen = classpect.role;
				AspectPick.chosen = classpect.aspect;
			}
		});
		newPlayer.characterLook.GetComponent<Button>().onClick.AddListener(OnImageClick);
		newPlayer.kickButton.onClick.AddListener(delegate
		{
			Debug.LogWarning("Currently not implemented in Steam!");
		});
		newPlayer.ownerButton.onClick.AddListener(delegate
		{
			AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetOwner(user);
		});
	}

	internal void UpdateChangeClasspect(int[] classScore, int[] aspectScore, SteamId user)
	{
		classpectSolver.SetClasspect(user, classScore, aspectScore);
	}

	public void UpdateChangeSpritePart(SteamId user, PlayerSpriteData data)
	{
		_lobbyListPlayers[user].characterLook.LoadFromBuffer(data);
		mask.enabled = false;
		mask.enabled = true;
	}

	public void UpdateChangeHouse(SteamId user, string house)
	{
		_lobbyListPlayers[user].housePicker.HouseChange(house);
	}

	public void UpdateChangeModus(SteamId user, string modus)
	{
		_lobbyListPlayers[user].modusPicker.ModusChange(modus);
	}
}
