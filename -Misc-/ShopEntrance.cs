using System.Linq;
using TheGenesisLib.Models;
using UnityEngine;

public class ShopEntrance : MonoBehaviour, AutoPickup
{
	private NormalItem[] item;

	public void Pickup(Player player)
	{
		if (!player.self)
		{
			return;
		}
		if (item == null)
		{
			float num = base.transform.localPosition.sqrMagnitude / 460800f;
			int value = Mathf.RoundToInt(30f * (num * 2f + 1f) / 3f);
			Planet componentInParent = GetComponentInParent<Planet>();
			item = (from wwwItem in componentInParent.GenerateDungeonItems(6, value, 5)
				select new NormalItem(wwwItem)).ToArray();
		}
		Shop.Open("Shop", item);
	}
}
