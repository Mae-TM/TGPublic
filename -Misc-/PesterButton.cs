using UnityEngine;
using UnityEngine.UI;

public class PesterButton : MonoBehaviour
{
	public void OnClick()
	{
		PesterchumHandler.Pester(base.transform.parent.Find("Name").GetComponent<Text>().text);
	}
}
