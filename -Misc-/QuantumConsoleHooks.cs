using QFSW.QC;
using UnityEngine;

public class QuantumConsoleHooks : MonoBehaviour
{
	[SerializeField]
	private QuantumConsole _qc;

	public static bool IsOpen { get; private set; }

	private void Start()
	{
		_qc = (_qc ? _qc : (GetComponent<QuantumConsole>() ?? QuantumConsole.Instance));
		if ((bool)_qc)
		{
			_qc.OnActivate += OnActivate;
			_qc.OnDeactivate += OnDeactivate;
		}
	}

	private void OnDeactivate()
	{
		IsOpen = false;
		if ((bool)Player.player)
		{
			KeyboardControl.Unblock();
		}
	}

	private void OnActivate()
	{
		IsOpen = true;
		if ((bool)Player.player)
		{
			KeyboardControl.Block();
		}
	}
}
