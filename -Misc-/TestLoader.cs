using System.Collections;
using Assets.Multiplayer.Scripts.commands;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestLoader : MonoBehaviour
{
	public enum LevelStage
	{
		BeforeEntering,
		Entered,
		Land,
		Dungeon
	}

	public string mapName = "john.bin";

	public bool fillInNameAutomatically = true;

	public string automaticName = "Horse";

	public bool teleportPlayer;

	public Vector3 teleportLocation = new Vector3(0f, 0f, 0f);

	public LevelStage levelStage;

	private IEnumerator coroutineTeleport;

	private IEnumerator coroutineDimension;

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void Start()
	{
		HouseBuilder.saveAs = mapName;
		SceneManager.LoadScene("HouseBuildingCombo", LoadSceneMode.Additive);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (fillInNameAutomatically)
		{
			InputField component = GameObject.Find("/Canvas/PlayerUI/NameInput/InputField").GetComponent<InputField>();
			component.text = automaticName;
			Player.player.sylladex.SetName(component);
		}
		if (teleportPlayer)
		{
			coroutineTeleport = TeleportPlayer(3f);
			StartCoroutine(coroutineTeleport);
		}
		coroutineDimension = RunDimensionCommands(1f);
		StartCoroutine(coroutineDimension);
	}

	private IEnumerator RunDimensionCommands(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		switch (levelStage)
		{
		case LevelStage.Dungeon:
			CommandBase.commands["dungeon"].RunCommand("dungeon");
			break;
		case LevelStage.Entered:
			CommandBase.commands["enter"].RunCommand("enter");
			break;
		case LevelStage.Land:
			CommandBase.commands["enter"].RunCommand("enter");
			yield return new WaitForSeconds(1f);
			Player.player.SetPosition(new Vector3(0f, 32f, 0f));
			break;
		}
	}

	private IEnumerator TeleportPlayer(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		Player.player.SetPosition(teleportLocation);
	}

	private void Update()
	{
	}
}
