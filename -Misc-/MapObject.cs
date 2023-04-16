using UnityEngine;

public class MapObject : MonoBehaviour
{
	public bool big = true;

	[SerializeField]
	private Sprite sprite;

	private void Start()
	{
		if (sprite != null && base.transform.root.GetComponent<House>() != null)
		{
			FlatMap.AddMarker(sprite, base.transform);
		}
		Object.Destroy(this);
	}
}
