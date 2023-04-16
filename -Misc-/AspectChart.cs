using System;
using UnityEngine.SceneManagement;

internal class AspectChart : FlowChart
{
	public static Aspect rAspect = Aspect.Count;

	public override void TestFinished(Enum result)
	{
		rAspect = (Aspect)(object)result;
		SceneManager.LoadScene("TestRoom");
	}

	private new void Start()
	{
		flowState = new FlowState("Basic", "You wake up in a void. You can see a mysterious object floating in front of you. What do you do?", new FlowState[2]
		{
			new FlowState("Touch it", "Basic", "You reach your arm out, but it instinictively darts out of the way.", new FlowState[2]
			{
				new FlowState("Chase it", Aspect.Space),
				new FlowState("Leave it", Aspect.Hope)
			}),
			new FlowState("Wait", Aspect.Time)
		});
		base.Start();
	}
}
