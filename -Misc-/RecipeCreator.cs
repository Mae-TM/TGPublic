using TheGenesisLib.Models;
using UnityEngine;
using UnityEngine.UI;

public class RecipeCreator : MonoBehaviour
{
	[SerializeField]
	private InputField item1;

	[SerializeField]
	private InputField item2;

	[SerializeField]
	private InputField result;

	[SerializeField]
	private Toggle method;

	public void SaveButton()
	{
		if (item1.text == "")
		{
			item1.text = "00000000";
		}
		if (item2.text == "")
		{
			item2.text = "00000000";
		}
		if (result.text == "")
		{
			result.text = "00000000";
		}
		AbstractSingletonManager<DatabaseManager>.Instance.CreateRecipe("inGameCreated", item1.text, item2.text, (!method.isOn) ? LDBRecipe.Methods.OR : LDBRecipe.Methods.AND, result.text);
	}
}
