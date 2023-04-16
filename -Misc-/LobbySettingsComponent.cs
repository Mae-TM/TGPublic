using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProtoBuf;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbySettingsComponent : MonoBehaviour
{
	[SerializeField]
	private InLobbyComponent _inLobbyComponent;

	public InputField _randomSeed;

	public Dropdown _loadFromFile;

	public InputField _sessionName;

	public Dropdown _visibility;

	public Toggle _allowDuplicateClasses;

	public Toggle _allowDuplicateAspects;

	public Toggle _allowDuplicateClasspects;

	[SerializeField]
	private Text _sessionNameWarning;

	public string loadFromDirectory;

	public void OnEnable()
	{
		loadFromDirectory = "";
		_loadFromFile.options.Clear();
		_sessionName.onValueChanged.AddListener(OnSessionNameChanged);
		_loadFromFile.onValueChanged.AddListener(OnPick);
		_visibility.onValueChanged.AddListener(OnVisChange);
		UpdateChangeability();
	}

	public void UpdateChangeability()
	{
		if (AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.IsOwnedBy(SteamClient.SteamId))
		{
			_randomSeed.interactable = true;
			_sessionName.interactable = true;
			_loadFromFile.interactable = true;
			_visibility.interactable = true;
			_allowDuplicateClasses.interactable = true;
			_allowDuplicateAspects.interactable = true;
			_allowDuplicateClasspects.interactable = true;
			_sessionName.text = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.Name;
			_visibility.value = (int)AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.Visibility;
			_loadFromFile.options.Clear();
			_loadFromFile.options.Add(new Dropdown.OptionData("Create new session..."));
			string[] directories = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, "SaveData"));
			foreach (string path in directories)
			{
				if (Directory.GetFiles(path).Any((string file) => file.EndsWith(".ses")))
				{
					string fileName = Path.GetFileName(path);
					_loadFromFile.options.Add(new Dropdown.OptionData(fileName));
				}
			}
		}
		else
		{
			_randomSeed.interactable = false;
			_sessionName.interactable = false;
			_loadFromFile.interactable = false;
			_visibility.interactable = false;
			_allowDuplicateClasses.interactable = false;
			_allowDuplicateAspects.interactable = false;
			_allowDuplicateClasspects.interactable = false;
		}
	}

	public void OnDisable()
	{
		_sessionName.onValueChanged.RemoveListener(OnSessionNameChanged);
		_loadFromFile.onValueChanged.RemoveListener(OnPick);
		_visibility.onValueChanged.RemoveListener(OnVisChange);
		_loadFromFile.options.Clear();
		_sessionName.text = "";
	}

	public SessionData? LoadSessionData(string sessionName)
	{
		string text = Path.Combine(Application.streamingAssetsPath, "SaveData", sessionName);
		if (!Directory.Exists(text))
		{
			return null;
		}
		if (!File.Exists(Path.Combine(text, "save.ses")))
		{
			return null;
		}
		SessionData? result = null;
		try
		{
			using FileStream source = File.OpenRead(Path.Combine(text, "save.ses"));
			result = Serializer.Deserialize<SessionData>(source);
			return result;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return result;
		}
	}

	public void OverrideGateOrder()
	{
		if (_sessionName.text == "" || _loadFromFile.value == 0)
		{
			return;
		}
		SessionData? sessionData = LoadSessionData(_sessionName.text);
		if (!sessionData.HasValue)
		{
			_loadFromFile.value = 0;
			loadFromDirectory = "";
			_sessionName.interactable = true;
			_sessionName.text = "";
			Debug.LogError("Unable to load session data, resetting settings...");
		}
		else
		{
			AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SteamLobby.SetData("GateOrder", JsonConvert.SerializeObject(sessionData.Value.gateOrder.Select(delegate(ulong i)
			{
				SteamId result = default(SteamId);
				result.Value = i;
				return result;
			}).ToList()));
		}
	}

	public bool LoadedSessionHasAllPlayers()
	{
		if (_loadFromFile.value == 0)
		{
			return true;
		}
		SessionData? sessionData = LoadSessionData(_sessionName.text);
		if (!sessionData.HasValue)
		{
			return true;
		}
		ulong[] gateOrder = sessionData.Value.gateOrder;
		ulong[] lobbyPlayers = AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.PlayerInformation.Keys.Select((SteamId k) => k.Value).ToArray();
		if (gateOrder.All((ulong p) => lobbyPlayers.Contains(p)))
		{
			return gateOrder.Length == lobbyPlayers.Length;
		}
		return false;
	}

	public void OnPick(int choice)
	{
		if (choice == 0)
		{
			loadFromDirectory = "";
			_sessionName.interactable = true;
			_sessionName.text = "";
		}
		else
		{
			loadFromDirectory = Path.Combine(Application.streamingAssetsPath, "SaveData", _sessionName.text);
			_sessionName.interactable = false;
			_sessionName.text = _loadFromFile.options[choice].text;
		}
	}

	private void OnSessionNameChanged(string newName)
	{
		if (!IsValidSessionName(_sessionName.text) && loadFromDirectory == "")
		{
			_sessionNameWarning.gameObject.SetActive(value: true);
			_inLobbyComponent.SetInteractableStart(to: false);
		}
		else
		{
			_sessionNameWarning.gameObject.SetActive(value: false);
			_inLobbyComponent.SetInteractableStart(to: true);
		}
	}

	public bool IsValidSessionName(string sessionName)
	{
		if (string.IsNullOrEmpty(sessionName) || string.IsNullOrWhiteSpace(sessionName))
		{
			return false;
		}
		string text = SanitizeFileName(sessionName.ToLower());
		if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		return !Directory.Exists(Path.Combine(Application.streamingAssetsPath, "SaveData", text));
	}

	public string SanitizeFileName(string fileName, char replacementChar = '_')
	{
		HashSet<char> hashSet = new HashSet<char>(Path.GetInvalidFileNameChars());
		char[] array = fileName.ToCharArray();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (hashSet.Contains(array[i]))
			{
				array[i] = replacementChar;
			}
		}
		return new string(array);
	}

	public void Load(LobbySettings settings)
	{
		_randomSeed.text = settings.RandomSeed.ToString();
	}

	private void OnVisChange(int visValue)
	{
		Debug.Log($"Received change to change visibility to {visValue}");
		AbstractSingletonManager<LobbyManager>.Instance.CurrentLobby.SetVisibility((TGPLobby.LobbyVisibility)visValue);
	}

	public LobbySettings? Save()
	{
		int result;
		bool flag = int.TryParse(_randomSeed.text, out result);
		if (string.IsNullOrEmpty(_randomSeed.text) || !flag)
		{
			result = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}
		if (string.IsNullOrEmpty(_sessionName.text))
		{
			return null;
		}
		LobbySettings value = default(LobbySettings);
		value.RandomSeed = result;
		value.SessionName = _sessionName.text;
		return value;
	}
}
