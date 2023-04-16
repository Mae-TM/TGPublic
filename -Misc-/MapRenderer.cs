using UnityEngine;

public class MapRenderer : MonoBehaviour
{
	[SerializeField]
	private FlatMap map;

	private void Awake()
	{
		map.Init();
	}

	private void OnDestroy()
	{
		map.Destroy();
	}
}
