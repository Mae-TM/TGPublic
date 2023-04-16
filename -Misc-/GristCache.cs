using UnityEngine;
using UnityEngine.UI;

public class GristCache : MonoBehaviour
{
	[SerializeField]
	private GristImage grist;

	private Text[] gristCounts;

	private void Awake()
	{
		if (gristCounts == null)
		{
			MakeGrists();
		}
	}

	private void MakeGrists()
	{
		gristCounts = new Text[63];
		Transform child = base.transform.GetChild(1);
		for (int i = 0; i < 3; i++)
		{
			MakeGrist(child, i);
		}
		child = grist.transform.parent;
		for (int j = 0; j < 5; j++)
		{
			for (Aspect aspect = Aspect.Time; aspect < Aspect.Count; aspect++)
			{
				MakeGrist(child, Grist.GetIndex(j, aspect));
			}
		}
		Object.Destroy(grist.gameObject);
	}

	private void MakeGrist(Transform parent, int index)
	{
		GristImage gristImage = Object.Instantiate(grist, parent);
		gristImage.SetGrist(index);
		gristCounts[index] = gristImage.transform.GetChild(0).GetComponent<Text>();
	}

	private void OnEnable()
	{
		for (int i = 0; i < 63; i++)
		{
			gristCounts[i].transform.parent.gameObject.SetActive(Player.player.Grist[i] != 0);
			gristCounts[i].text = Sylladex.MetricFormat(Player.player.Grist[i]);
		}
	}
}
