using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
	public enum Step
	{
		Start,
		SelectChest,
		DropChest,
		SelectToilet,
		DropToilet,
		RepairFloor,
		ReplaceToilet,
		End
	}

	public GameObject panel;

	public Text text;

	private HouseBuilder houseBuilder;

	private Step step;

	private string[] texts = new string[7] { "Welcome to the SBurb tutorial. First, let's learn about select mode. Click on Select (first button), then try clicking on the chest.", "Good job! Now let's learn to move. Use WASD to move around on one floor, and the up and down arrow keys to move between floors. Try placing the chest on the roof.", "Now go back to the floor below. See if you can find the bathroom and pick up the toilet. You can click the magnifying glass or use the scroll wheel to zoom out if you have trouble.", "Uh oh! Better put that down somewhere else for now.", "Now let's try repairing the floor. Click revise (second button) and drag across the floor to fix it.", "You can also use that to extend rooms, and by holding control you can build staircases. Now put the toilet back.", "Good as new!" };

	public Step getStep()
	{
		return step;
	}

	public void proceed()
	{
		text.text = texts[(int)step];
		panel.SetActive(value: true);
	}

	public void ok()
	{
		step++;
		panel.SetActive(value: false);
	}

	public void cancel()
	{
		Object.Destroy(panel);
		Object.Destroy(this);
	}

	public void setHouseBuilder(HouseBuilder houseBuilder)
	{
		this.houseBuilder = houseBuilder;
	}

	private void Start()
	{
		text.text = texts[0];
	}

	private void Update()
	{
	}
}
