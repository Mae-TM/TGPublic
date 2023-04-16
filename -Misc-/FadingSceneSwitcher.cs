using UnityEngine;
using UnityEngine.SceneManagement;

public class FadingSceneSwitcher : MonoBehaviour
{
	public Fader fader;

	private string nextScene;

	public void EnterScene(string targetScene)
	{
		fader.fadedToBlack = fadeFinished;
		nextScene = targetScene;
		fader.BeginFade(1);
		Time.timeScale = 1f;
	}

	private void fadeFinished()
	{
		SceneManager.LoadScene(nextScene);
	}
}
