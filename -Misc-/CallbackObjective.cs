using System;
using MoonSharp.Interpreter;
using Quest.NET.Enums;
using Quest.NET.Interfaces;

public class CallbackObjective : QuestObjective
{
	private Closure RemoveCallback;

	private ScriptFunctionDelegate callbackGenerator;

	public CallbackObjective(string title, string description, bool isBonus)
		: base(title, description, isBonus)
	{
	}

	public void SetCallbackGenerator(ScriptFunctionDelegate callbackGenerator)
	{
		if (this.callbackGenerator != null)
		{
			throw new Exception("CallbackGenerator has already been set!");
		}
		if (RemoveCallback != null)
		{
			throw new Exception("RemoveCallback has already been set, so callbackGenerator is no longer necessary!");
		}
		this.callbackGenerator = callbackGenerator;
	}

	public override ObjectiveStatus CheckProgress()
	{
		if (RemoveCallback == null && callbackGenerator != null)
		{
			RemoveCallback = (Closure)callbackGenerator(new Action(Complete));
		}
		return base.CheckProgress();
	}

	public void Fail()
	{
		Status = ObjectiveStatus.Failed;
	}

	public void Update()
	{
		Status = ObjectiveStatus.Updated;
	}

	private void Complete()
	{
		Status = ObjectiveStatus.Completed;
		RemoveCallback.Call();
	}
}
