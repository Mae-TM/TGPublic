using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public static string scene;

	private const string loadingScreenScene = "LoadingScreen";

	private AsyncOperation async;

	[SerializeField]
	private Image loadingBar;

	[SerializeField]
	private Text text;

	private static string[] loadingTexts;

	private void Start()
	{
		if (loadingTexts == null)
		{
			loadingTexts = StreamingAssets.ReadAllLines("loadinglines.txt");
		}
		async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		async.completed += delegate
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
		};
		StartCoroutine(RandomizeText());
	}

	private void Update()
	{
		loadingBar.fillAmount = async.progress;
	}

	private IEnumerator RandomizeText()
	{
		while (!async.isDone)
		{
			text.text = loadingTexts[Random.Range(0, loadingTexts.Length)];
			yield return new WaitForSeconds(0.3f + (float)Random.Range(0, 1));
		}
	}

	public static void LoadScene(string sceneToLoad)
	{
		scene = sceneToLoad;
		SceneManager.LoadScene("LoadingScreen");
	}

	public static void FinishLoading()
	{
		Scene sceneByName = SceneManager.GetSceneByName("LoadingScreen");
		if (sceneByName.isLoaded)
		{
			SceneManager.UnloadSceneAsync(sceneByName);
		}
	}
}
