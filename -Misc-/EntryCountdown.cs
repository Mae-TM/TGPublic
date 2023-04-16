using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.UI;

public class EntryCountdown : NetworkBehaviour
{
	private static Text timerUI;

	private static float prevSpeed;

	private House house;

	public CruxtruderState state;

	public float timerTime = 253f;

	public string TimerText => $"{(int)(timerTime / 60f)}:{(int)(timerTime % 60f):D2}";

	public event Action OnOpen;

	public event Action<string> OnTimerUpdate;

	private void Start()
	{
		house = GetComponent<House>();
		if (!timerUI)
		{
			timerUI = Player.Ui.Find("Timer").GetChild(0).GetComponent<Text>();
		}
	}

	private IEnumerator UpdateTimer()
	{
		while (state == CruxtruderState.Open)
		{
			timerTime -= Time.deltaTime;
			if (timerTime <= 0f)
			{
				SpawnMeteors();
				break;
			}
			string timerText = TimerText;
			this.OnTimerUpdate?.Invoke(timerText);
			if (Player.player.RegionChild.Area == house && MusicHolder.PlayTimerMusic(timerTime))
			{
				timerUI.transform.parent.gameObject.SetActive(value: true);
				timerUI.text = timerText;
				if (timerTime < 10f)
				{
					SoundEffects.Instance.Shake(0f, 75f / (5f + timerTime * timerTime), 0.5f);
				}
				else if (timerTime / 2f - 0.1f - Mathf.Floor(timerTime / 2f - 0.1f) > 0.9f)
				{
					SoundEffects.Instance.Shake(0f, (90f - timerTime) * (90f - timerTime) / 2700f, 0.2f);
				}
			}
			else
			{
				timerUI.transform.parent.gameObject.SetActive(value: false);
			}
			yield return null;
		}
		timerUI.transform.parent.gameObject.SetActive(value: false);
	}

	private void SpawnMeteors()
	{
		if (Player.player.RegionChild.Area == house)
		{
			BuildExploreSwitcher.Instance.SwitchToExplore();
			Death.Summon("meteors");
		}
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntryCountdown::Open()' called when server was not active");
			return;
		}
		state = CruxtruderState.Open;
		RpcOpen();
	}

	[ClientRpc]
	private void RpcOpen()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(EntryCountdown), "RpcOpen", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[Server]
	public void Enter()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntryCountdown::Enter()' called when server was not active");
			return;
		}
		if (base.isServerOnly)
		{
			state = CruxtruderState.Entered;
			house.Enter();
			EnterAction[] componentsInChildren = GetComponentsInChildren<EnterAction>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
		RpcEnter();
	}

	[ClientRpc]
	private void RpcEnter()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(EntryCountdown), "RpcEnter", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnFadeFinished()
	{
		Fader.instance.fadeSpeed = prevSpeed;
		Fader.instance.BeginFade(-1);
		timerUI.transform.parent.gameObject.SetActive(value: false);
		MusicHolder.EndTimerMusic();
		house.Enter();
		EnterAction[] componentsInChildren = GetComponentsInChildren<EnterAction>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	private void MirrorProcessed()
	{
	}

	private void UserCode_RpcOpen()
	{
		state = CruxtruderState.Open;
		this.OnOpen?.Invoke();
		StartCoroutine(UpdateTimer());
	}

	protected static void InvokeUserCode_RpcOpen(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOpen called on server.");
		}
		else
		{
			((EntryCountdown)obj).UserCode_RpcOpen();
		}
	}

	private void UserCode_RpcEnter()
	{
		state = CruxtruderState.Entered;
		Fader.instance.fadedToBlack = OnFadeFinished;
		prevSpeed = Fader.instance.fadeSpeed;
		Fader.instance.fadeSpeed = (Visibility.Get(base.gameObject) ? 0.5f : 100f);
		Fader.instance.BeginFade(1);
	}

	protected static void InvokeUserCode_RpcEnter(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnter called on server.");
		}
		else
		{
			((EntryCountdown)obj).UserCode_RpcEnter();
		}
	}

	static EntryCountdown()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(EntryCountdown), "RpcOpen", InvokeUserCode_RpcOpen);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EntryCountdown), "RpcEnter", InvokeUserCode_RpcEnter);
	}
}
