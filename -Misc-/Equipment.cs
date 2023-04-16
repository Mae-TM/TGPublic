using UnityEngine;
using UnityEngine.UI;

public class Equipment : MonoBehaviour
{
	[SerializeField]
	private Transform spriteImage;

	[SerializeField]
	private Text playerName;

	[SerializeField]
	private Text playerTitle;

	private void OnEnable()
	{
		RefreshSprite();
		playerName.text = Player.player.sylladex.PlayerName;
		playerTitle.text = Player.player.classpect.ToString();
	}

	public void RefreshSprite()
	{
		foreach (Transform item in spriteImage)
		{
			Object.Destroy(item.gameObject);
		}
		ImageEffects.Imagify(Player.player.GetComponentInChildren<BillboardSprite>().transform, spriteImage);
	}
}
