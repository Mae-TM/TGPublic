using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
	private static Shop instance;

	public List<Button> options = new List<Button>();

	public Text title;

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public static void Open(string title, NormalItem[] items)
	{
		instance.OpenShop(title, items);
	}

	private void OpenShop(string title, NormalItem[] items)
	{
		this.title.text = title;
		int i;
		for (i = 0; i < items.Length; i++)
		{
			NormalItem item = items[i];
			Button button;
			if (i >= options.Count)
			{
				button = Object.Instantiate(options[0], options[0].transform.parent);
				options.Add(button);
			}
			else
			{
				button = options[i];
			}
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				float num = item.GetBoonCost();
				if (Player.player.boonBucks >= num && Player.player.sylladex.AddItem(item.Copy()))
				{
					Player.player.boonBucks -= num;
				}
			});
			Image component = button.transform.GetChild(0).GetComponent<Image>();
			Item.ClearImage(component);
			item.ApplyToImage(component);
			button.transform.GetChild(1).GetComponent<Text>().text = item.GetItemName();
			button.transform.GetChild(2).GetChild(1).GetComponent<Text>()
				.text = Sylladex.MetricFormat(item.GetBoonCost());
			button.gameObject.SetActive(value: true);
		}
		for (; i < options.Count; i++)
		{
			options[i].gameObject.SetActive(value: false);
		}
		base.transform.GetChild(0).gameObject.SetActive(value: true);
	}
}
