using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialDirector : MonoBehaviour
{
	public static bool isTutorial;

	public static void StartTutorial()
	{
		isTutorial = true;
		MultiplayerSettings.hosting = true;
		SessionRandom.seed = 413;
		NetcodeManager.LocalPlayerId = 0;
		WorldManager.colors = new Color[1] { Color.black };
		ChangeSpritePart.LoadCharacter("sleuth", "john", "long", "21", "21", HairHighlights.Pale, 29, 0f, new PBColor(0.333333f, 1f, 0.85098f), isRobot: false, "", "JohnGlss", "long", "ShrtPant", "Bootzies");
		ClassPick.chosen = Class.Heir;
		AspectPick.chosen = Aspect.Breath;
		HouseBuilder.saveAs = "John.bin";
		BuildExploreSwitcher.cheatMode = true;
		Fader.instance.fadedToBlack = delegate
		{
			SceneManager.LoadScene("HouseBuildingCombo");
		};
		Fader.instance.BeginFade(1);
		Time.timeScale = 1f;
	}

	private void Start()
	{
		if (!isTutorial)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Debug.Log("Starting tutorial");
		}
	}

	private void OnDestroy()
	{
		if (isTutorial)
		{
			isTutorial = false;
		}
	}

	private void Update()
	{
	}
}
