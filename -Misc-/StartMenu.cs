using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
	private string nextScene;

	public void Start()
	{
		AbstractAttachedSingletonManager<RichPresenceManager>.Instance.InMenus();
	}

	public void EnterScene(string targetScene)
	{
		Fader.instance.fadedToBlack = fadeFinished;
		nextScene = targetScene;
		Fader.instance.BeginFade(1);
		Time.timeScale = 1f;
	}

	private void fadeFinished()
	{
		SceneManager.LoadScene(nextScene);
	}

	public void StopGame()
	{
		Application.Quit();
	}

	public void OpenWebsite()
	{
		Application.OpenURL(Settings.HomeUrl);
	}
}
