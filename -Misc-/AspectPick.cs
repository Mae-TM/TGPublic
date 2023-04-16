using UnityEngine;

public class AspectPick : MonoBehaviour
{
	public static Aspect chosen = Aspect.Count;

	public static int[] score = new int[12];

	[SerializeField]
	private Aspect aspect = Aspect.Count;

	public LobbyClasspectWizard LobbyClasspectWizard;

	public void Select()
	{
		chosen = aspect;
		for (int i = 0; i < 12; i++)
		{
			score[i] = ((chosen == (Aspect)i) ? 1 : 0);
		}
		LobbyClasspectWizard.TriggerOnDone();
	}

	public static int[] CalculateScore()
	{
		for (int i = 0; i < 12; i++)
		{
			score[i] = ((chosen == (Aspect)i) ? 1 : 0);
		}
		return score;
	}
}
