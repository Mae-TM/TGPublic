using UnityEngine;

public class Village : MonoBehaviour
{
	private bool isDiscovered;

	public static void Make(DisplacementMapFlat planet, Vector3 position, float sqrSize)
	{
		GameObject obj = new GameObject("Village")
		{
			layer = 2,
			tag = "Fixed Layer"
		};
		Transform obj2 = obj.transform;
		Transform parent = planet.GetChunk(position, fill: false).transform;
		obj2.SetParent(parent, worldPositionStays: false);
		obj2.localPosition = position;
		SphereCollider sphereCollider = obj.AddComponent<SphereCollider>();
		sphereCollider.isTrigger = true;
		sphereCollider.radius = Mathf.Sqrt(sqrSize);
		obj.AddComponent<Village>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.IsChildOf(Player.player.transform) && !isDiscovered)
		{
			FlatMap.AddVillage(base.transform);
			isDiscovered = true;
		}
	}
}
