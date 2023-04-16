using System;
using Mirror;
using UnityEngine;

public class Cruxtruder : MonoBehaviour
{
	private class OpenCruxtruderAction : SyncedInteractableAction
	{
		public Cruxtruder cruxTruder;

		private void Start()
		{
			sprite = Resources.Load<Sprite>("Spirograph");
			desc = "Pry Open";
		}

		protected override bool LocalExecute()
		{
			cruxTruder.OnOpen?.Invoke();
			return true;
		}

		protected override void ServerExecute(Player player)
		{
			cruxTruder.ServerOpen();
		}
	}

	[SerializeField]
	private TextMesh[] timerMesh;

	[SerializeField]
	private Transform spritePosition;

	private House house;

	public event Action OnOpen;

	private void OnValidate()
	{
		GetComponent<Animator>().keepAnimatorControllerStateOnDisable = true;
	}

	private void Start()
	{
		house = base.transform.root.GetComponent<House>();
		base.transform.Find("Cruxite").GetComponent<MeshRenderer>().material.color = house.cruxiteColor;
		if (house.Owner != null && house.Owner.self)
		{
			house.Owner.sylladex.quests.AddEntryQuest();
		}
		EntryCountdown component = house.GetComponent<EntryCountdown>();
		component.OnTimerUpdate += SetTimerText;
		SetTimerText(component.TimerText);
		component.OnOpen += Open;
		if (component.state == CruxtruderState.Closed)
		{
			base.gameObject.AddComponent<OpenCruxtruderAction>().cruxTruder = this;
		}
		else
		{
			Open();
		}
	}

	private void OnDestroy()
	{
		if ((bool)house)
		{
			EntryCountdown component = house.GetComponent<EntryCountdown>();
			component.OnTimerUpdate -= SetTimerText;
			component.OnOpen -= Open;
		}
	}

	private void SetTimerText(string text)
	{
		TextMesh[] array = timerMesh;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].text = text;
		}
	}

	[Server]
	private void ServerOpen()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Cruxtruder::ServerOpen()' called when server was not active");
			return;
		}
		house.Owner.MakeKernelSprite(spritePosition.position);
		base.transform.root.GetComponent<EntryCountdown>().Open();
	}

	private void Open()
	{
		if (TryGetComponent<OpenCruxtruderAction>(out var component))
		{
			UnityEngine.Object.Destroy(component);
		}
		if (TryGetComponent<Interactable>(out var component2))
		{
			UnityEngine.Object.Destroy(component2);
		}
		TextMesh[] array = timerMesh;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
		GetComponent<Animator>().SetBool("Cruxite", value: true);
	}

	public void Opened()
	{
		UnityEngine.Object.Destroy(base.transform.Find("Lid").gameObject);
		PickupItemAction pickupItemAction = base.transform.Find("Cruxite").gameObject.AddComponent<PickupItemAction>();
		pickupItemAction.targetItem = new Totem(new NormalItem("00000000"), house.cruxiteColor);
		pickupItemAction.objectIsItem = false;
		pickupItemAction.consume = false;
	}
}
