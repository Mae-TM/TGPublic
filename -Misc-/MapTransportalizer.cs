using UnityEngine;

public class MapTransportalizer : MonoBehaviour
{
	public MediumMap map;

	public Transform original;

	private void OnMouseUpAsButton()
	{
		if (map.transportalizerMode)
		{
			Player.player.SetPosition(original.localPosition, original.root.GetComponent<WorldArea>());
			map.transportalizerMode = false;
		}
	}
}
